using System;
using System.ComponentModel.DataAnnotations;

namespace FansVoice.UserService.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }

        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [StringLength(500)]
        public required string Bio { get; set; }

        [StringLength(200)]
        public required string ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }

        // Sosyal medya bağlantıları
        [StringLength(100)]
        public required string TwitterHandle { get; set; }

        [StringLength(100)]
        public required string InstagramHandle { get; set; }

        [StringLength(100)]
        public required string FacebookHandle { get; set; }

        // Takım ve Etkinlik İlişkileri
        public Guid? FavoriteTeamId { get; set; }
        public bool ReceiveMatchNotifications { get; set; } = true;
        public bool ReceiveTeamNotifications { get; set; } = true;
        public bool ReceiveChantNotifications { get; set; } = true;

        // Sosyal Lig Bilgileri
        public int SocialLeaguePoints { get; set; }
        public int ChantParticipationCount { get; set; }
        public int EventParticipationCount { get; set; }
        public required string CurrentBadge { get; set; }
        public DateTime? LastEventParticipation { get; set; }
        public DateTime? LastChantParticipation { get; set; }

        // Marş Senkronizasyonu Bilgileri
        public bool IsInChantSession { get; set; }
        public Guid? CurrentChantSessionId { get; set; }
        public DateTime? ChantSessionJoinedAt { get; set; }
        public required string LastKnownChantLatency { get; set; }
        public int TotalChantDuration { get; set; } // Dakika cinsinden
        public int ConsecutiveChantDays { get; set; }
        public DateTime? LastChantStreak { get; set; }

        // Kullanıcı Tercihleri
        public string PreferredLanguage { get; set; } = "tr";
        public bool DarkModeEnabled { get; set; }
        public string TimeZone { get; set; } = "Europe/Istanbul";
    }
}