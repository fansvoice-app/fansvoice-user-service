using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FansVoice.UserService.DTOs;
using FansVoice.UserService.Models;

namespace FansVoice.UserService.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid id);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<UserDto> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
        Task<UserDto> UpdateProfilePictureAsync(Guid id, UpdateProfilePictureDto updateProfilePictureDto);
        Task<bool> DeactivateUserAsync(Guid id);
        Task<bool> VerifyUserAsync(Guid id);

        // Yeni metodlar
        Task<UserDto> UpdateUserPointsAsync(Guid id, UpdateUserPointsDto pointsDto);
        Task<UserDto> RecordEventParticipationAsync(Guid id, UserEventParticipationDto participationDto);
        Task<UserDto> UpdateUserBadgeAsync(Guid id, UserBadgeUpdateDto badgeDto);
        Task<IEnumerable<UserDto>> GetUsersByTeamIdAsync(Guid teamId);
        Task<IEnumerable<UserDto>> GetActiveEventParticipantsAsync(Guid eventId);
        Task<bool> UpdateNotificationPreferencesAsync(Guid id, bool matchNotifications, bool teamNotifications, bool chantNotifications);
        Task<int> GetUserRankInSocialLeagueAsync(Guid id);
        Task<IEnumerable<UserDto>> GetTopUsersInSocialLeagueAsync(int count);

        // Marş Senkronizasyonu Metodları
        Task<UserDto> JoinChantSessionAsync(Guid userId, Guid sessionId);
        Task<UserDto> LeaveChantSessionAsync(Guid userId);
        Task<UserDto> UpdateChantSessionStatusAsync(Guid userId, UserChantSessionUpdateDto updateDto);
        Task<UserDto> UpdateChantStreakAsync(Guid userId, ChantStreakUpdateDto streakDto);
        Task<IEnumerable<UserDto>> GetActiveChantParticipantsAsync(Guid sessionId);
        Task<ChantSessionDto> GetUserCurrentChantSessionAsync(Guid userId);
        Task<int> GetUserChantStreakAsync(Guid userId);
        Task<IEnumerable<UserDto>> GetTopChantContributorsAsync(int count);
    }
}