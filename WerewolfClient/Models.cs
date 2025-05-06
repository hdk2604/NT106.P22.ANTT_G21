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
        // Vai trò và thuộc tính liên quan
        public string Role { get; set; } = "villager";
        public string Team { get; set; } // "villagers" hoặc "werewolves"
        public bool IsRevealed { get; set; } = false; // Đã bị lộ vai trò chưa
        public bool IsAlive { get; set; } = true;
        public bool IsHost { get; set; } = false; // Có phải chủ phòng không
        public bool IsConnected { get; set; }
        public bool IsReady { get; set; }
        // Hành động ban đêm
        public string VotedFor { get; set; } // Người chơi đã vote (dùng cho cả sói và vote treo cổ)
        public string ProtectedPlayerId { get; set; } // Người được bảo vệ (bodyguard)
        public string LastProtectedPlayerId { get; set; } // Người được bảo vệ đêm trước (bodyguard)
        public bool HasUsedHealPotion { get; set; } = false; // Đã dùng thuốc cứu (witch)
        public bool HasUsedKillPotion { get; set; } = false; // Đã dùng thuốc độc (witch)
        public bool IsProtectedByWitch { get; set; } = false; // Được phù thủy cứu
        public bool IsPoisonedByWitch { get; set; } = false; // Bị phù thủy đầu độc
        public CheckResult CheckedPlayer { get; set; } // Kết quả kiểm tra (seer)
        public bool CanShoot { get; set; } = false; // Có thể bắn khi chết (hunter)

        // Trạng thái đặc biệt
        public bool IsSilenced { get; set; } = false; // Bị câm (không thể nói)
        public bool IsProtected { get; set; } = false; // Được bảo vệ khỏi sói
        public DateTime LastActionTime { get; set; } // Thời gian hành động gần nhất

        // Thống kê
        public int VotesReceived { get; set; } = 0; // Số phiếu bầu nhận được
        public int KillCount { get; set; } = 0; // Số lần giết người (sói/thợ săn)

        public class CheckResult
        {
            public string PlayerId { get; set; }
            public string Role { get; set; }
            public bool IsWerewolf { get; set; }
        }

        public bool CanPerformNightAction(string currentPhase)
        {
            if (!IsAlive) return false;

            if (currentPhase == "night")
            {
                if (Role == "werewolf" || Role == "seer" || Role == "witch" || Role == "bodyguard")
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanVote(string currentPhase)
        {
            return IsAlive && currentPhase == "day";
        }

        public void ResetNightActions()
        {
            VotedFor = null;
            ProtectedPlayerId = null;
            IsProtectedByWitch = false;
            IsPoisonedByWitch = false;
            CheckedPlayer = null;
        }
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