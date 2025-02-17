using System;

namespace FansVoice.UserService.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
        public bool IsVerified { get; set; }
        public string TwitterHandle { get; set; }
        public string InstagramHandle { get; set; }
        public string FacebookHandle { get; set; }
        public Guid? FavoriteTeamId { get; set; }
        public bool ReceiveMatchNotifications { get; set; }
        public bool ReceiveTeamNotifications { get; set; }
        public bool ReceiveChantNotifications { get; set; }
        public int SocialLeaguePoints { get; set; }
        public int ChantParticipationCount { get; set; }
        public int EventParticipationCount { get; set; }
        public string CurrentBadge { get; set; }
        public DateTime? LastEventParticipation { get; set; }
        public DateTime? LastChantParticipation { get; set; }
        public bool IsInChantSession { get; set; }
        public Guid? CurrentChantSessionId { get; set; }
        public DateTime? ChantSessionJoinedAt { get; set; }
        public string LastKnownChantLatency { get; set; }
        public int TotalChantDuration { get; set; }
        public int ConsecutiveChantDays { get; set; }
        public DateTime? LastChantStreak { get; set; }
        public string PreferredLanguage { get; set; }
        public bool DarkModeEnabled { get; set; }
        public string TimeZone { get; set; }
    }

    public class UpdateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string TwitterHandle { get; set; }
        public string InstagramHandle { get; set; }
        public string FacebookHandle { get; set; }
        public Guid? FavoriteTeamId { get; set; }
        public bool? ReceiveMatchNotifications { get; set; }
        public bool? ReceiveTeamNotifications { get; set; }
        public bool? ReceiveChantNotifications { get; set; }
        public string PreferredLanguage { get; set; }
        public bool? DarkModeEnabled { get; set; }
        public string TimeZone { get; set; }
    }

    public class UpdateProfilePictureDto
    {
        public string ProfilePictureUrl { get; set; }
    }

    public class UpdateUserPointsDto
    {
        public int PointsToAdd { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
    }

    public class UserEventParticipationDto
    {
        public Guid EventId { get; set; }
        public DateTime ParticipationTime { get; set; }
        public string ParticipationType { get; set; }
    }

    public class UserBadgeUpdateDto
    {
        public string BadgeName { get; set; }
        public string AchievementType { get; set; }
        public DateTime AwardedAt { get; set; }
    }

    public class ChantSessionDto
    {
        public Guid SessionId { get; set; }
        public string ChantName { get; set; }
        public string TeamName { get; set; }
        public DateTime StartTime { get; set; }
        public int ParticipantCount { get; set; }
        public string SessionStatus { get; set; }
    }

    public class UserChantSessionUpdateDto
    {
        public Guid SessionId { get; set; }
        public string Latency { get; set; }
        public DateTime Timestamp { get; set; }
        public string ConnectionStatus { get; set; }
    }

    public class ChantStreakUpdateDto
    {
        public int ConsecutiveDays { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime LastStreakDate { get; set; }
    }
}