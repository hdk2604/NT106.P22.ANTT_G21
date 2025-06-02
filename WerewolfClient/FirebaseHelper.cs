using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WerewolfClient.Models; 

public class FirebaseHelper
{
    public readonly FirebaseClient firebase;

    public FirebaseHelper()
    {
        firebase = new FirebaseClient(
            "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(CurrentUserManager.CurrentUser?.IdToken)
            });
    }

    // Cập nhật trạng thái game
    public async Task UpdateGameStatus(string gameId, string status)
    {
        await firebase
            .Child("games")
            .Child(gameId)
            .Child("status")
            .PutAsync($"\"{status}\"");
    }

    // Lấy thông tin game
    public async Task<Game> GetGame(string gameId)
    {
        return await firebase
            .Child("games")
            .Child(gameId)
            .OnceSingleAsync<Game>();
    }

    // Lấy danh sách người chơi trong game
    public async Task<List<Player>> GetPlayers(string gameId)
    {
        var playersData = await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .OnceAsync<Player>();

        return playersData.Select(p =>
        {
            var playerObject = p.Object;
            playerObject.Id = p.Key; // Firebase Key is the Player's ID
            return playerObject;
        }).ToList();
    }

    // Xóa game khi kết thúc
    public async Task DeleteGame(string gameId)
    {
        await firebase
            .Child("games")
            .Child(gameId)
            .DeleteAsync();
    }

    // Lắng nghe thay đổi trạng thái game
    public IDisposable ListenForGameChanges(string gameId, Action<Game> callback)
    {
        return firebase
            .Child("games")
            .Child(gameId)
            .AsObservable<Game>()
            .Subscribe(gameEvent => callback(gameEvent.Object));
    }

    // Lắng nghe thay đổi danh sách người chơi
    public IDisposable ListenForPlayers(string gameId, Action<List<Player>> callback)
    {
        List<Player> currentPlayers = new List<Player>();
        Task.Run(async () => {
            currentPlayers = await GetPlayers(gameId);
            callback(new List<Player>(currentPlayers));
        });

        return firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .AsObservable<Player>()
            .Subscribe(async playerEvent =>
            {
                currentPlayers = await GetPlayers(gameId); 
                callback(new List<Player>(currentPlayers));
            });
    }


    // Tạo game mới
    public async Task<string> CreateGame(int maxPlayers, string creatorId, string serverRoomId)
    {
        var game = new Game
        {
            Status = "waiting",
            MaxPlayers = maxPlayers,
            CreatedAt = DateTime.UtcNow,
            CreatorId = creatorId,
            CurrentPhase = "night", 
            RoundNumber = 1,
            CurrentPlayerCount = 0,
            RoomId = serverRoomId,
            PhaseStartTime = DateTime.UtcNow.ToString("o")
        };

        var result = await firebase
            .Child("games")
            .PostAsync(game);

        return result.Key;
    }

    // Thêm người chơi vào game
    public async Task AddPlayer(string gameId, Player player)
    {
        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(player.Id)
            .PutAsync(player);
    }

    // Cập nhật vai trò người chơi
    public async Task UpdatePlayerRole(string gameId, string playerId, string role)
    {
        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(playerId)
            .Child("Role")
            .PutAsync($"\"{role}\""); 
    }

    // Thêm log sự kiện game
    public async Task AddGameLog(string gameId, string message, string phase)
    {
        var gameLog = new GameLog
        {
            Timestamp = DateTime.UtcNow,
            Message = message,
            Phase = phase
        };

        await firebase
            .Child("gameLogs")
            .Child(gameId)
            .PostAsync(gameLog); 
    }
    public static async Task SaveUsernameAsync(string userId, string username, string email, string idToken)
    {
        var firebaseClient = new FirebaseClient(
            "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(idToken)
            });
        await firebaseClient
            .Child("users")
            .Child(userId)
            .PutAsync(new { username = username, email = email }); 
    }

    public static async Task<string> GetUsernameAsync(string userId, string idToken)
    {
        var firebaseClient = new FirebaseClient(
            "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(idToken)
            });
        var userNode = await firebaseClient
            .Child("users")
            .Child(userId)
            .OnceSingleAsync<UserInfo>();
        return userNode?.Username ?? string.Empty;
    }
    public async Task WereWolfAction(string gameId, string werewolfId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "night")
        {
            throw new InvalidOperationException("Sói chỉ có thể chọn vào ban đêm.");
        }
        var players = await GetPlayers(gameId);
        var werewolf = players.FirstOrDefault(p => p.Id == werewolfId);
        var target = players.FirstOrDefault(p => p.Id == targetPlayerId);

        if (werewolf == null || !werewolf.IsAlive || werewolf.Role != "werewolf")
        {
            throw new InvalidOperationException("Người thực hiện không phải là Sói hoặc đã chết.");
        }
        if (target == null || !target.IsAlive)
        {
            throw new InvalidOperationException("Mục tiêu không hợp lệ hoặc đã chết.");
        }
        if (werewolfId == targetPlayerId)
        {
            throw new InvalidOperationException("Sói không thể tự cắn mình.");
        }

        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(werewolfId)
            .Child(nameof(Player.VotedFor))
            .PutAsync($"\"{targetPlayerId}\""); 

        await AddGameLog(gameId, $"Sói đã chọn giết {target.Name}.", "night");
    }


    // === TIÊN TRI (SEER) ===
    public async Task<Player.CheckResult> SeerCheckPlayer(string gameId, string seerId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "night")
        {
            throw new InvalidOperationException("Tiên tri chỉ có thể kiểm tra vào ban đêm.");
        }

        var players = await GetPlayers(gameId);
        var seer = players.FirstOrDefault(p => p.Id == seerId);
        var targetPlayer = players.FirstOrDefault(p => p.Id == targetPlayerId);

        if (seer == null || !seer.IsAlive || seer.Role != "seer")
        {
            throw new InvalidOperationException("Người thực hiện không phải là Tiên tri hoặc đã chết.");
        }
        if (targetPlayer == null)
        {
            throw new ArgumentException("Người chơi mục tiêu không tồn tại.");
        }
        if (seerId == targetPlayerId)
        {
            throw new InvalidOperationException("Tiên tri không thể tự soi mình.");
        }

        var checkResult = new Player.CheckResult
        {
            PlayerId = targetPlayerId,
            Role = targetPlayer.Role,
            IsWerewolf = (targetPlayer.Role == "werewolf" || targetPlayer.Team == "werewolves")
        };

        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(seerId)
            .Child(nameof(Player.CheckedPlayer))
            .PutAsync(checkResult); 

        await AddGameLog(gameId, $"Tiên tri đã soi {targetPlayer.Name}.", "night");
        return checkResult;
    }

    // === PHÙ THỦY (WITCH) ===
    public async Task WitchUseHealPotion(string gameId, string witchId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "night")
        {
            throw new InvalidOperationException("Phù thủy chỉ có thể dùng thuốc vào ban đêm.");
        }

        var players = await GetPlayers(gameId);
        var witch = players.FirstOrDefault(p => p.Id == witchId);
        var targetPlayer = players.FirstOrDefault(p => p.Id == targetPlayerId);

        if (witch == null || witch.Role != "witch" || !witch.IsAlive)
        {
            throw new ArgumentException("Người chơi không phải là Phù thủy hoặc đã chết.");
        }
        if (witch.HasUsedHealPotion)
        {
            throw new InvalidOperationException("Phù thủy đã dùng hết thuốc cứu.");
        }
        if (targetPlayer == null || !targetPlayer.IsAlive)
        {
            throw new InvalidOperationException("Mục tiêu không hợp lệ hoặc đã chết.");
        }

        await firebase
            .Child("games").Child(gameId).Child("players").Child(witchId)
            .Child(nameof(Player.HasUsedHealPotion)).PutAsync(true); 
        await firebase
            .Child("games").Child(gameId).Child("players").Child(targetPlayerId)
            .Child(nameof(Player.IsProtectedByWitch)).PutAsync(true); 

        await AddGameLog(gameId, $"Phù thủy đã dùng thuốc cứu cho {targetPlayer.Name}.", "night");
    }

    public async Task WitchUseKillPotion(string gameId, string witchId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "night")
        {
            throw new InvalidOperationException("Phù thủy chỉ có thể dùng thuốc vào ban đêm.");
        }
        var players = await GetPlayers(gameId);
        var witch = players.FirstOrDefault(p => p.Id == witchId);
        var targetPlayer = players.FirstOrDefault(p => p.Id == targetPlayerId);

        if (witch == null || witch.Role != "witch" || !witch.IsAlive)
        {
            throw new ArgumentException("Người chơi không phải là Phù thủy hoặc đã chết.");
        }
        if (witch.HasUsedKillPotion)
        {
            throw new InvalidOperationException("Phù thủy đã dùng hết thuốc độc.");
        }
        if (targetPlayer == null || !targetPlayer.IsAlive)
        {
            throw new InvalidOperationException("Mục tiêu không hợp lệ hoặc đã chết.");
        }
        if (witchId == targetPlayerId)
        {
            throw new InvalidOperationException("Phù thủy không thể tự đầu độc mình.");
        }

        await firebase
            .Child("games").Child(gameId).Child("players").Child(witchId)
            .Child(nameof(Player.HasUsedKillPotion)).PutAsync(true); // << LƯU Ý: Boolean là JSON hợp lệ
        await firebase
            .Child("games").Child(gameId).Child("players").Child(targetPlayerId)
            .Child(nameof(Player.IsPoisonedByWitch)).PutAsync(true); // << LƯU Ý: Boolean là JSON hợp lệ

        await AddGameLog(gameId, $"Phù thủy đã dùng thuốc độc lên {targetPlayer.Name}.", "night");
    }

    // === BẢO VỆ (BODYGUARD) ===
    public async Task BodyguardProtectPlayer(string gameId, string bodyguardId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "night")
        {
            throw new InvalidOperationException("Bảo vệ chỉ có thể bảo vệ vào ban đêm.");
        }
        var players = await GetPlayers(gameId);
        var bodyguard = players.FirstOrDefault(p => p.Id == bodyguardId);
        var targetPlayer = players.FirstOrDefault(p => p.Id == targetPlayerId);

        if (bodyguard == null || bodyguard.Role != "bodyguard" || !bodyguard.IsAlive)
        {
            throw new ArgumentException("Người chơi không phải là Bảo vệ hoặc đã chết.");
        }
        if (bodyguard.LastProtectedPlayerId == targetPlayerId)
        {
            throw new InvalidOperationException("Bảo vệ không thể bảo vệ cùng một người 2 đêm liên tiếp.");
        }
        if (targetPlayer == null || !targetPlayer.IsAlive)
        {
            throw new InvalidOperationException("Mục tiêu không hợp lệ hoặc đã chết.");
        }

        await firebase
            .Child("games").Child(gameId).Child("players").Child(bodyguardId)
            .Child(nameof(Player.ProtectedPlayerId)).PutAsync($"\"{targetPlayerId}\"");

        await AddGameLog(gameId, $"Bảo vệ {bodyguard.Name} đang bảo vệ {targetPlayer.Name}.", "night");
    }

    // === THỢ SĂN (HUNTER) ===
    public async Task HunterShootPlayer(string gameId, string hunterId, string targetPlayerId)
    {
        var players = await GetPlayers(gameId);
        var hunter = players.FirstOrDefault(p => p.Id == hunterId);
        var targetPlayer = players.FirstOrDefault(p => p.Id == targetPlayerId);

        if (hunter == null || hunter.Role != "hunter")
        {
            throw new ArgumentException("Người chơi không phải là Thợ săn.");
        }
        if (hunter.IsAlive || !hunter.CanShoot)
        {
            throw new InvalidOperationException("Thợ săn chỉ có thể bắn khi bị giết và có quyền bắn.");
        }
        if (targetPlayer == null || !targetPlayer.IsAlive)
        {
            throw new ArgumentException("Mục tiêu không hợp lệ hoặc đã chết.");
        }
        if (hunterId == targetPlayerId)
        {
            throw new InvalidOperationException("Thợ săn không thể tự bắn mình.");
        }

        await firebase
            .Child("games").Child(gameId).Child("players").Child(targetPlayerId)
            .Child(nameof(Player.IsAlive)).PutAsync(false); 
        await firebase
            .Child("games").Child(gameId).Child("players").Child(hunterId)
            .Child(nameof(Player.CanShoot)).PutAsync(false); 

        await AddGameLog(gameId, $"Thợ săn đã bắn chết {targetPlayer.Name}!", "hunter_shot");
        await CheckGameEndCondition(gameId);
    }

    public async Task JoinGame(string gameId, Player player)
    {
        await firebase
            .Child("games").Child(gameId).Child("players").Child(player.Id)
            .PutAsync(player); 

        var game = await GetGame(gameId);
        if (game != null)
        {
            await firebase
                .Child("games").Child(gameId)
                .Child(nameof(Game.CurrentPlayerCount))
                .PutAsync(game.CurrentPlayerCount + 1); 
        }
    }

    public async Task LeaveGame(string gameId, string playerId)
    {
        var players = await GetPlayers(gameId);
        var playerLeaving = players.FirstOrDefault(p => p.Id == playerId);

        if (playerLeaving != null)
        {
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                [nameof(Player.IsAlive)] = false,
                [nameof(Player.IsConnected)] = false
            };
            await firebase
                .Child("games").Child(gameId).Child("players").Child(playerId)
                .PatchAsync(updates);

            await AddGameLog(gameId, $"Người chơi {playerLeaving.Name} đã rời game.", "system");
        }
        await CheckGameEndCondition(gameId);
    }

    public async Task<string> NextPhase(string gameId, string currentPhaseStrFirebase)
    {
        try
        {
            string nextPhase = "night";
            int newRoundNumber = (await GetGame(gameId))?.RoundNumber ?? 1;

            switch (currentPhaseStrFirebase.ToLower())
            {
                case "night":
                    nextPhase = "day_discussion";
                    break;
                case "day_discussion":
                    nextPhase = "day_vote";
                    break;
                case "day_vote":
                    nextPhase = "night";
                    newRoundNumber++;
                    break;
                default:
                    nextPhase = "night";
                    break;
            }

            Dictionary<string, object> phaseUpdates = new Dictionary<string, object>
            {
                [nameof(Game.CurrentPhase)] = nextPhase,
                [nameof(Game.PhaseStartTime)] = DateTime.UtcNow.ToString("o"),
                [nameof(Game.RoundNumber)] = newRoundNumber 
            };

            await firebase
                .Child("games")
                .Child(gameId)
                .PatchAsync(phaseUpdates); 

            await AddGameLog(gameId, $"Chuyển sang giai đoạn: {nextPhase} (Vòng {newRoundNumber}).", "system");

            await CheckGameEndCondition(gameId);
            var game = await GetGame(gameId);
            if (!string.IsNullOrEmpty(game?.Status) && (game.Status == "villagers_win" || game.Status == "werewolves_win"))
            {
                return game.Status;
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in NextPhase for game {gameId}: {ex.ToString()}");
            await AddGameLog(gameId, $"Lỗi khi chuyển phase: {ex.Message}", "SystemError");
            return "error_in_next_phase";
        }
    }

    // GHI NHẬN PHIẾU BẦU BAN NGÀY
    public async Task RecordDayVote(string gameId, string voterId, string targetId)
    {
        var players = await GetPlayers(gameId);
        var voter = players.FirstOrDefault(p => p.Id == voterId);
        var target = players.FirstOrDefault(p => p.Id == targetId);

        if (voter != null && voter.IsAlive && !voter.HasVotedToday)
        {
            if (target == null || !target.IsAlive)
            {
                throw new InvalidOperationException("Mục tiêu bỏ phiếu không hợp lệ hoặc đã chết.");
            }
            if (voterId == targetId)
            {
                throw new InvalidOperationException("Không thể tự bỏ phiếu cho chính mình.");
            }

            await firebase
                .Child("games").Child(gameId).Child("players").Child(voterId)
                .Child(nameof(Player.VotedForDay)).PutAsync($"\"{targetId}\""); 
            await firebase
                .Child("games").Child(gameId).Child("players").Child(voterId)
                .Child(nameof(Player.HasVotedToday)).PutAsync(true);
            await AddGameLog(gameId, $"Người chơi {voter.Name} đã bỏ phiếu treo cổ {target.Name}.", "day_vote");
        }
        else if (voter != null && voter.HasVotedToday)
        {
            throw new InvalidOperationException("Bạn đã bỏ phiếu trong ngày hôm nay rồi.");
        }
        else
        {
            throw new InvalidOperationException("Người bỏ phiếu không hợp lệ.");
        }
    }

    // XỬ LÝ KẾT QUẢ BỎ PHIẾU BAN NGÀY (HOST GỌI)
    public async Task ProcessDayVoteResults(string gameId)
    {
        try
        {
            var players = await GetPlayers(gameId);
            var alivePlayers = players.Where(p => p.IsAlive).ToList();

            var voteCounts = alivePlayers
                .Where(p => !string.IsNullOrEmpty(p.VotedForDay))
                .GroupBy(p => p.VotedForDay)
                .Select(g => new { TargetId = g.Key, TargetName = players.FirstOrDefault(p => p.Id == g.Key)?.Name, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            Dictionary<string, object> updates = new Dictionary<string, object>();
            string logMessage = "Không có ai bị treo cổ (không có phiếu bầu hợp lệ hoặc hòa phiếu).";

            if (voteCounts.Any())
            {
                var topVote = voteCounts.First();
                var tiedVotes = voteCounts.Where(v => v.Count == topVote.Count).ToList();

                if (tiedVotes.Count == 1)
                {
                    string executedPlayerId = topVote.TargetId;
                    var executedPlayer = players.FirstOrDefault(p => p.Id == executedPlayerId);
                    if (executedPlayer != null)
                    {
                        updates[$"players/{executedPlayerId}/{nameof(Player.IsAlive)}"] = false;
                        logMessage = $"Người chơi {executedPlayer.Name} đã bị treo cổ theo kết quả bỏ phiếu.";
                        await AddGameLog(gameId, logMessage, "day_vote_result");

                        if (executedPlayer.Role == "hunter")
                        {
                            updates[$"players/{executedPlayerId}/{nameof(Player.CanShoot)}"] = true;
                            await AddGameLog(gameId, $"Thợ săn {executedPlayer.Name} có thể thực hiện phát bắn cuối cùng!", "day_vote_result");
                        }
                    }
                }
                else
                {
                    logMessage = "Kết quả bỏ phiếu hòa, không ai bị treo cổ.";
                    await AddGameLog(gameId, logMessage, "day_vote_result");
                }
            }
            else
            {
                await AddGameLog(gameId, logMessage, "day_vote_result");
            }

            foreach (var player in players)
            {
                updates[$"players/{player.Id}/{nameof(Player.VotedForDay)}"] = null;
                updates[$"players/{player.Id}/{nameof(Player.HasVotedToday)}"] = false;
            }

            if (updates.Any())
            {
                await firebase.Child("games").Child(gameId).PatchAsync(updates);
            }
            await CheckGameEndCondition(gameId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ProcessDayVoteResults for game {gameId}: {ex.ToString()}");
            await AddGameLog(gameId, $"Lỗi xử lý kết quả bỏ phiếu: {ex.Message}", "SystemError");
        }
    }

    // XỬ LÝ KẾT QUẢ ĐÊM
    public async Task ProcessNightResults(string gameId)
    {
        try
        {
            var players = await GetPlayers(gameId);
            Dictionary<string, object> updates = new Dictionary<string, object>();
            List<string> killedTonightNames = new List<string>();

            string werewolfTargetId = null;
            var werewolfVotesGroups = players
                .Where(p => p.IsAlive && p.Role == "werewolf" && !string.IsNullOrEmpty(p.VotedFor))
                .GroupBy(p => p.VotedFor)
                .OrderByDescending(g => g.Count())
                .ToList();

            if (werewolfVotesGroups.Any())
            {
                var topVoteGroup = werewolfVotesGroups.First();
                if (werewolfVotesGroups.Count > 1 && werewolfVotesGroups[1].Count() == topVoteGroup.Count())
                {
                    await AddGameLog(gameId, "Sói không thống nhất được mục tiêu (hòa phiếu), không ai bị Sói cắn.", "night_result");
                }
                else
                {
                    werewolfTargetId = topVoteGroup.Key;
                }
            }

            string bodyguardProtectedId = players.FirstOrDefault(p => p.IsAlive && p.Role == "bodyguard" && !string.IsNullOrEmpty(p.ProtectedPlayerId))?.ProtectedPlayerId;

            var witch = players.FirstOrDefault(p => p.IsAlive && p.Role == "witch");
            string witchHealTargetId = null;
            string witchPoisonTargetId = null;

            if (witch != null)
            {
                if (witch.HasUsedHealPotion)
                {
                    witchHealTargetId = players.FirstOrDefault(p => p.IsAlive && p.IsProtectedByWitch)?.Id;
                }
                if (witch.HasUsedKillPotion)
                {
                    witchPoisonTargetId = players.FirstOrDefault(p => p.IsAlive && p.IsPoisonedByWitch && p.Id != witch.Id)?.Id;
                }
            }
            List<string> diedThisNightPlayerIds = new List<string>();

            if (witchPoisonTargetId != null)
            {
                if (witchPoisonTargetId == bodyguardProtectedId)
                {
                    await AddGameLog(gameId, $"{players.FirstOrDefault(p => p.Id == witchPoisonTargetId)?.Name} bị Phù thủy đầu độc nhưng được Bảo vệ cứu mạng!", "night_result");
                }
                else
                {
                    diedThisNightPlayerIds.Add(witchPoisonTargetId);
                }
            }

            if (werewolfTargetId != null && !diedThisNightPlayerIds.Contains(werewolfTargetId))
            {
                bool savedByBodyguard = (werewolfTargetId == bodyguardProtectedId);
                bool savedByWitchHeal = (werewolfTargetId == witchHealTargetId && witch != null && witch.HasUsedHealPotion && players.FirstOrDefault(p => p.Id == werewolfTargetId)?.IsProtectedByWitch == true);

                if (savedByBodyguard)
                {
                    await AddGameLog(gameId, $"{players.FirstOrDefault(p => p.Id == werewolfTargetId)?.Name} bị Sói tấn công nhưng được Bảo vệ cứu mạng!", "night_result");
                }
                else if (savedByWitchHeal)
                {
                    await AddGameLog(gameId, $"{players.FirstOrDefault(p => p.Id == werewolfTargetId)?.Name} bị Sói tấn công nhưng được Phù thủy cứu!", "night_result");
                }
                else
                {
                    diedThisNightPlayerIds.Add(werewolfTargetId);
                }
            }
            else if (werewolfTargetId != null && diedThisNightPlayerIds.Contains(werewolfTargetId))
            {
                await AddGameLog(gameId, $"Sói nhắm vào {players.FirstOrDefault(p => p.Id == werewolfTargetId)?.Name} nhưng người này đã chết vì lý do khác.", "night_result");
            }
            else if (werewolfTargetId == null && werewolfVotesGroups.Any())
            {
                // Hòa phiếu sói đã log
            }
            else
            {
                await AddGameLog(gameId, "Đêm nay Sói không cắn ai.", "night_result");
            }

            foreach (var deadPlayerId in diedThisNightPlayerIds.Distinct())
            {
                updates[$"players/{deadPlayerId}/{nameof(Player.IsAlive)}"] = false;
                var deadPlayerName = players.FirstOrDefault(p => p.Id == deadPlayerId)?.Name;
                if (!string.IsNullOrEmpty(deadPlayerName)) killedTonightNames.Add(deadPlayerName);
            }

            foreach (var player in players)
            {
                updates[$"players/{player.Id}/{nameof(Player.VotedFor)}"] = null;
                updates[$"players/{player.Id}/{nameof(Player.CheckedPlayer)}"] = null;
                updates[$"players/{player.Id}/{nameof(Player.IsProtectedByWitch)}"] = false;
                updates[$"players/{player.Id}/{nameof(Player.IsPoisonedByWitch)}"] = false;
                updates[$"players/{player.Id}/{nameof(Player.NightActionTarget)}"] = null;
                updates[$"players/{player.Id}/{nameof(Player.NightActionType)}"] = null;

                if (player.Role == "bodyguard")
                {
                    updates[$"players/{player.Id}/{nameof(Player.LastProtectedPlayerId)}"] = player.ProtectedPlayerId;
                    updates[$"players/{player.Id}/{nameof(Player.ProtectedPlayerId)}"] = null;
                }
            }

            if (updates.Any())
            {
                await firebase.Child("games").Child(gameId).PatchAsync(updates);
            }

            if (killedTonightNames.Any())
            {
                await AddGameLog(gameId, "Những người đã chết đêm nay: " + string.Join(", ", killedTonightNames.Distinct()) + ".", "night_summary");
            }
            else
            {
                await AddGameLog(gameId, "Không có ai chết đêm nay.", "night_summary");
            }

            await CheckGameEndCondition(gameId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ProcessNightResults for game {gameId}: {ex.ToString()}");
            await AddGameLog(gameId, $"Lỗi xử lý kết quả đêm: {ex.Message}", "SystemError");
        }
    }

    public async Task CheckGameEndCondition(string gameId)
    {
        try
        {
            var game = await GetGame(gameId);
            if (game != null && game.Status != "villagers_win" && game.Status != "werewolves_win" && game.Status != "ended")
            {
                var players = await GetPlayers(gameId);
                var alivePlayers = players.Where(p => p.IsAlive && p.IsConnected != false).ToList();

                long aliveWerewolves = alivePlayers.Count(p => p.Role == "werewolf" || p.Team == "werewolves");
                long aliveNonWerewolves = alivePlayers.Count(p => p.Role != "werewolf" && p.Team != "werewolves");

                string newStatus = null;

                if (aliveWerewolves == 0 && aliveNonWerewolves > 0)
                {
                    newStatus = "villagers_win";
                }
                else if (aliveWerewolves > 0 && aliveWerewolves >= aliveNonWerewolves)
                {
                    newStatus = "werewolves_win";
                }

                if (newStatus != null)
                {
                    await firebase
                        .Child("games").Child(gameId).Child(nameof(Game.Status))
                        .PutAsync($"\"{newStatus}\""); 
                    await AddGameLog(gameId, $"Trò chơi kết thúc! {newStatus.Replace("_", " ").ToUpper()}", "game_end");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckGameEndCondition for game {gameId}: {ex.ToString()}");
            await AddGameLog(gameId, $"Lỗi kiểm tra kết thúc game: {ex.Message}", "SystemError");
        }
    }
}