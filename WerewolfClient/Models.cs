using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WerewolfClient.Models
{
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
        public string Name { get; set; }
        public string Status { get; set; } = "waiting";
        public string CurrentPhase { get; set; } = "night";
        public int RoundNumber { get; set; } = 1;
        public int MaxPlayers { get; set; } = 8;
        public string CreatorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class GameLog
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Phase { get; set; }
    }
}