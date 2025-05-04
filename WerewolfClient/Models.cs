using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WerewolfClient.Models
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string IdToken { get; set; }
        public string Username { get; set; }
    }

    public class Player
    {
        public string Id { get; set; }
        public string UserId { get; set; }  // Liên kết với tài khoản
        public string Name { get; set; }
        public string Role { get; set; } = "villager";
        public bool IsAlive { get; set; } = true;
        public string VotedFor { get; set; }
        public bool IsConnected { get; set; }
        public bool IsReady { get; set; }
    }

    public class Game
    {
        public string Id { get; set; }
        public string Status { get; set; } = "waiting";
        public string CurrentPhase { get; set; } = "night";
        public int RoundNumber { get; set; } = 1;
        public int MaxPlayers { get; set; } = 8;
        public string CreatorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CurrentPlayerCount { get; set; } // Thêm dòng này
        public string RoomId { get; set; }
    }

    public class GameLog
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Phase { get; set; }
    }

    public static class CurrentUserManager
    {
        private static UserInfo _currentUser;

        public static UserInfo CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        public static void SetCurrentUser(string id, string email, string idToken)
        {
            _currentUser = new UserInfo
            {
                Id = id,
                Email = email,
                IdToken = idToken
            };
        }

        public static void ClearCurrentUser()
        {
            _currentUser = null;
        }
    }
}