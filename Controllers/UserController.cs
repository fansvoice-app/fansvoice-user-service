using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FansVoice.UserService.DTOs;
using FansVoice.UserService.Interfaces;

namespace FansVoice.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}/profile-picture")]
        public async Task<ActionResult<UserDto>> UpdateProfilePicture(Guid id, UpdateProfilePictureDto updateProfilePictureDto)
        {
            var user = await _userService.UpdateProfilePictureAsync(id, updateProfilePictureDto);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeactivateUser(Guid id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> VerifyUser(Guid id)
        {
            var result = await _userService.VerifyUserAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/points")]
        public async Task<ActionResult<UserDto>> UpdateUserPoints(Guid id, UpdateUserPointsDto pointsDto)
        {
            var user = await _userService.UpdateUserPointsAsync(id, pointsDto);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("{id}/event-participation")]
        public async Task<ActionResult<UserDto>> RecordEventParticipation(Guid id, UserEventParticipationDto participationDto)
        {
            var user = await _userService.RecordEventParticipationAsync(id, participationDto);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("{id}/badge")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> UpdateUserBadge(Guid id, UserBadgeUpdateDto badgeDto)
        {
            var user = await _userService.UpdateUserBadgeAsync(id, badgeDto);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("team/{teamId}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByTeam(Guid teamId)
        {
            var users = await _userService.GetUsersByTeamIdAsync(teamId);
            return Ok(users);
        }

        [HttpGet("event/{eventId}/participants")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetEventParticipants(Guid eventId)
        {
            var users = await _userService.GetActiveEventParticipantsAsync(eventId);
            return Ok(users);
        }

        [HttpPut("{id}/notification-preferences")]
        public async Task<ActionResult> UpdateNotificationPreferences(
            Guid id,
            [FromQuery] bool matchNotifications,
            [FromQuery] bool teamNotifications,
            [FromQuery] bool chantNotifications)
        {
            var result = await _userService.UpdateNotificationPreferencesAsync(
                id, matchNotifications, teamNotifications, chantNotifications);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/social-league-rank")]
        public async Task<ActionResult<int>> GetUserRankInSocialLeague(Guid id)
        {
            var rank = await _userService.GetUserRankInSocialLeagueAsync(id);
            if (rank == -1) return NotFound();
            return Ok(rank);
        }

        [HttpGet("social-league/top/{count}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetTopUsersInSocialLeague(int count)
        {
            if (count <= 0 || count > 100) count = 10;
            var users = await _userService.GetTopUsersInSocialLeagueAsync(count);
            return Ok(users);
        }

        [HttpPost("{id}/chant/join/{sessionId}")]
        public async Task<ActionResult<UserDto>> JoinChantSession(Guid id, Guid sessionId)
        {
            var user = await _userService.JoinChantSessionAsync(id, sessionId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("{id}/chant/leave")]
        public async Task<ActionResult<UserDto>> LeaveChantSession(Guid id)
        {
            var user = await _userService.LeaveChantSessionAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("{id}/chant/status")]
        public async Task<ActionResult<UserDto>> UpdateChantStatus(Guid id, UserChantSessionUpdateDto updateDto)
        {
            var user = await _userService.UpdateChantSessionStatusAsync(id, updateDto);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("{id}/chant/streak")]
        public async Task<ActionResult<UserDto>> UpdateChantStreak(Guid id, ChantStreakUpdateDto streakDto)
        {
            var user = await _userService.UpdateChantStreakAsync(id, streakDto);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("chant/{sessionId}/participants")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetChantParticipants(Guid sessionId)
        {
            var users = await _userService.GetActiveChantParticipantsAsync(sessionId);
            return Ok(users);
        }

        [HttpGet("{id}/chant/current")]
        public async Task<ActionResult<ChantSessionDto>> GetCurrentChantSession(Guid id)
        {
            var session = await _userService.GetUserCurrentChantSessionAsync(id);
            if (session == null) return NotFound();
            return Ok(session);
        }

        [HttpGet("{id}/chant/streak")]
        public async Task<ActionResult<int>> GetChantStreak(Guid id)
        {
            var streak = await _userService.GetUserChantStreakAsync(id);
            return Ok(streak);
        }

        [HttpGet("chant/top-contributors/{count}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetTopChantContributors(int count)
        {
            if (count <= 0 || count > 100) count = 10;
            var users = await _userService.GetTopChantContributorsAsync(count);
            return Ok(users);
        }
    }
}