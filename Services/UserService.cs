using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FansVoice.UserService.Data;
using FansVoice.UserService.DTOs;
using FansVoice.UserService.Interfaces;
using FansVoice.UserService.Models;
using Microsoft.Extensions.Logging;

namespace FansVoice.UserService.Services
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;
        private readonly IMessageBusService _messageBus;
        private readonly ILogger<UserService> _logger;
        private readonly ICacheService _cache;

        public UserService(
            UserDbContext context,
            IMessageBusService messageBus,
            ILogger<UserService> logger,
            ICacheService cache)
        {
            _context = context;
            _messageBus = messageBus;
            _logger = logger;
            _cache = cache;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var cacheKey = $"user:{id}";
            var cachedUser = await _cache.GetAsync<UserDto>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("User {UserId} retrieved from cache", id);
                return cachedUser;
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            var userDto = MapToDto(user);
            await _cache.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(10));
            return userDto;
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var cacheKey = $"user:username:{username}";
            var cachedUser = await _cache.GetAsync<UserDto>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("User with username {Username} retrieved from cache", username);
                return cachedUser;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            var userDto = MapToDto(user);
            await _cache.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(10));
            return userDto;
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;
            user.Bio = updateUserDto.Bio ?? user.Bio;
            user.TwitterHandle = updateUserDto.TwitterHandle ?? user.TwitterHandle;
            user.InstagramHandle = updateUserDto.InstagramHandle ?? user.InstagramHandle;
            user.FacebookHandle = updateUserDto.FacebookHandle ?? user.FacebookHandle;
            user.FavoriteTeamId = updateUserDto.FavoriteTeamId ?? user.FavoriteTeamId;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                var userDto = MapToDto(user);

                // Invalidate cache
                await _cache.RemoveAsync($"user:{id}");
                await _cache.RemoveAsync($"user:username:{user.Username}");

                // Publish user update event
                _messageBus.PublishMessage("user", "user.updated", userDto);
                _logger.LogInformation("User {UserId} updated successfully", id);

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                throw;
            }
        }

        public async Task<UserDto> UpdateProfilePictureAsync(Guid id, UpdateProfilePictureDto updateProfilePictureDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.ProfilePictureUrl = updateProfilePictureDto.ProfilePictureUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<bool> DeactivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();

                // Invalidate cache
                await _cache.RemoveAsync($"user:{id}");
                await _cache.RemoveAsync($"user:username:{user.Username}");

                _messageBus.PublishMessage("user", "user.deactivated", new { UserId = id });
                _logger.LogInformation("User {UserId} deactivated successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                throw;
            }
        }

        public async Task<bool> VerifyUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsVerified = true;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _messageBus.PublishMessage("user", "user.verified", new { UserId = id });
                _logger.LogInformation("User {UserId} verified successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying user {UserId}", id);
                throw;
            }
        }

        public async Task<UserDto> UpdateUserPointsAsync(Guid id, UpdateUserPointsDto pointsDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.SocialLeaguePoints += pointsDto.PointsToAdd;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                var userDto = MapToDto(user);

                _messageBus.PublishMessage("user", "user.points.updated", new
                {
                    UserId = id,
                    PointsAdded = pointsDto.PointsToAdd,
                    ActivityType = pointsDto.ActivityType,
                    Description = pointsDto.Description,
                    NewTotalPoints = user.SocialLeaguePoints
                });

                _logger.LogInformation("User {UserId} points updated: {Points} for {Activity}",
                    id, pointsDto.PointsToAdd, pointsDto.ActivityType);

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating points for user {UserId}", id);
                throw;
            }
        }

        public async Task<UserDto> RecordEventParticipationAsync(Guid id, UserEventParticipationDto participationDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.EventParticipationCount++;
            user.LastEventParticipation = participationDto.ParticipationTime;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                var userDto = MapToDto(user);

                _messageBus.PublishMessage("user", "user.event.participated", new
                {
                    UserId = id,
                    EventId = participationDto.EventId,
                    ParticipationType = participationDto.ParticipationType,
                    ParticipationTime = participationDto.ParticipationTime
                });

                _logger.LogInformation("User {UserId} participated in event {EventId}",
                    id, participationDto.EventId);

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording event participation for user {UserId}", id);
                throw;
            }
        }

        public async Task<UserDto> UpdateUserBadgeAsync(Guid id, UserBadgeUpdateDto badgeDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.CurrentBadge = badgeDto.BadgeName;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                var userDto = MapToDto(user);

                _messageBus.PublishMessage("user", "user.badge.updated", new
                {
                    UserId = id,
                    BadgeName = badgeDto.BadgeName,
                    AchievementType = badgeDto.AchievementType,
                    AwardedAt = badgeDto.AwardedAt
                });

                _logger.LogInformation("User {UserId} earned badge: {Badge}",
                    id, badgeDto.BadgeName);

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating badge for user {UserId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetUsersByTeamIdAsync(Guid teamId)
        {
            var users = await _context.Users
                .Where(u => u.FavoriteTeamId == teamId && u.IsActive)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        public async Task<IEnumerable<UserDto>> GetActiveEventParticipantsAsync(Guid eventId)
        {
            // Bu metod Event Service ile entegre çalışacak
            // Şimdilik sadece aktif kullanıcıları döndürüyoruz
            var users = await _context.Users
                .Where(u => u.IsActive && u.LastEventParticipation != null)
                .OrderByDescending(u => u.LastEventParticipation)
                .Take(100)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        public async Task<bool> UpdateNotificationPreferencesAsync(
            Guid id,
            bool matchNotifications,
            bool teamNotifications,
            bool chantNotifications)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.ReceiveMatchNotifications = matchNotifications;
            user.ReceiveTeamNotifications = teamNotifications;
            user.ReceiveChantNotifications = chantNotifications;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();

                _messageBus.PublishMessage("user", "user.notifications.updated", new
                {
                    UserId = id,
                    MatchNotifications = matchNotifications,
                    TeamNotifications = teamNotifications,
                    ChantNotifications = chantNotifications
                });

                _logger.LogInformation("User {UserId} notification preferences updated", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for user {UserId}", id);
                throw;
            }
        }

        public async Task<int> GetUserRankInSocialLeagueAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return -1;

            var rank = await _context.Users
                .Where(u => u.IsActive && u.SocialLeaguePoints > user.SocialLeaguePoints)
                .CountAsync();

            return rank + 1;
        }

        public async Task<IEnumerable<UserDto>> GetTopUsersInSocialLeagueAsync(int count)
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.SocialLeaguePoints)
                .Take(count)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        public async Task<UserDto> JoinChantSessionAsync(Guid userId, Guid sessionId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.IsInChantSession = true;
            user.CurrentChantSessionId = sessionId;
            user.ChantSessionJoinedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                var userDto = MapToDto(user);

                _messageBus.PublishMessage("chant", "user.chant.joined", new
                {
                    UserId = userId,
                    SessionId = sessionId,
                    JoinedAt = user.ChantSessionJoinedAt
                });

                _logger.LogInformation("User {UserId} joined chant session {SessionId}", userId, sessionId);

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining chant session for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto> LeaveChantSessionAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var sessionId = user.CurrentChantSessionId;
            user.IsInChantSession = false;
            user.CurrentChantSessionId = null;
            user.ChantSessionJoinedAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                var userDto = MapToDto(user);

                _messageBus.PublishMessage("chant", "user.chant.left", new
                {
                    UserId = userId,
                    SessionId = sessionId,
                    LeftAt = DateTime.UtcNow
                });

                _logger.LogInformation("User {UserId} left chant session {SessionId}", userId, sessionId);

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chant session for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto> UpdateChantSessionStatusAsync(Guid userId, UserChantSessionUpdateDto updateDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.LastKnownChantLatency = updateDto.Latency;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                var userDto = MapToDto(user);

                _messageBus.PublishMessage("chant", "user.chant.status", new
                {
                    UserId = userId,
                    SessionId = updateDto.SessionId,
                    Latency = updateDto.Latency,
                    Status = updateDto.ConnectionStatus,
                    Timestamp = updateDto.Timestamp
                });

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chant status for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto> UpdateChantStreakAsync(Guid userId, ChantStreakUpdateDto streakDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.ConsecutiveChantDays = streakDto.ConsecutiveDays;
            user.TotalChantDuration += streakDto.DurationInMinutes;
            user.LastChantStreak = streakDto.LastStreakDate;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                var userDto = MapToDto(user);

                _messageBus.PublishMessage("chant", "user.chant.streak", new
                {
                    UserId = userId,
                    ConsecutiveDays = streakDto.ConsecutiveDays,
                    TotalDuration = user.TotalChantDuration,
                    LastStreakDate = streakDto.LastStreakDate
                });

                _logger.LogInformation(
                    "User {UserId} updated chant streak: {Days} days, total duration: {Duration} minutes",
                    userId, streakDto.ConsecutiveDays, user.TotalChantDuration);

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chant streak for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetActiveChantParticipantsAsync(Guid sessionId)
        {
            var users = await _context.Users
                .Where(u => u.IsActive && u.IsInChantSession && u.CurrentChantSessionId == sessionId)
                .OrderBy(u => u.ChantSessionJoinedAt)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        public async Task<ChantSessionDto> GetUserCurrentChantSessionAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsInChantSession || !user.CurrentChantSessionId.HasValue)
                return null;

            // Bu kısım Event Service ile entegre çalışacak
            // Şimdilik mock data dönüyoruz
            return new ChantSessionDto
            {
                SessionId = user.CurrentChantSessionId.Value,
                ChantName = "Mock Chant",
                TeamName = "Mock Team",
                StartTime = user.ChantSessionJoinedAt ?? DateTime.UtcNow,
                ParticipantCount = await _context.Users
                    .CountAsync(u => u.CurrentChantSessionId == user.CurrentChantSessionId),
                SessionStatus = "Active"
            };
        }

        public async Task<int> GetUserChantStreakAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.ConsecutiveChantDays ?? 0;
        }

        public async Task<IEnumerable<UserDto>> GetTopChantContributorsAsync(int count)
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.TotalChantDuration)
                .ThenByDescending(u => u.ConsecutiveChantDays)
                .Take(count)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                IsVerified = user.IsVerified,
                TwitterHandle = user.TwitterHandle,
                InstagramHandle = user.InstagramHandle,
                FacebookHandle = user.FacebookHandle,
                FavoriteTeamId = user.FavoriteTeamId,
                ReceiveMatchNotifications = user.ReceiveMatchNotifications,
                ReceiveTeamNotifications = user.ReceiveTeamNotifications,
                ReceiveChantNotifications = user.ReceiveChantNotifications,
                SocialLeaguePoints = user.SocialLeaguePoints,
                ChantParticipationCount = user.ChantParticipationCount,
                EventParticipationCount = user.EventParticipationCount,
                CurrentBadge = user.CurrentBadge,
                LastEventParticipation = user.LastEventParticipation,
                LastChantParticipation = user.LastChantParticipation,
                PreferredLanguage = user.PreferredLanguage,
                DarkModeEnabled = user.DarkModeEnabled,
                TimeZone = user.TimeZone
            };
        }
    }
}