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
            .PutAsync(status);
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
        var players = await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .OnceAsync<Player>();

        return players.Select(p => p.Object).ToList();
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
            .Subscribe(game => callback(game.Object));
    }

    // Lắng nghe thay đổi danh sách người chơi
    public IDisposable ListenForPlayers(string gameId, Action<List<Player>> callback)
    {
        List<Player> currentPlayers = new List<Player>();

        return firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .AsObservable<Player>()
            .Subscribe(playerEvent =>
            {
                if (playerEvent.Object != null)
                {
                    var existing = currentPlayers.FirstOrDefault(p => p.Id == playerEvent.Key);
                    if (playerEvent.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                    {
                        if (existing != null)
                            currentPlayers.Remove(existing);
                    }
                    else
                    {
                        if (existing != null)
                            currentPlayers.Remove(existing);

                        var updatedPlayer = playerEvent.Object;
                        updatedPlayer.Id = playerEvent.Key;
                        currentPlayers.Add(updatedPlayer);
                    }

                    callback(currentPlayers.ToList());
                }
            });
    }


    // Tạo game mới
    public async Task<string> CreateGame( int maxPlayers,string creatorId, string serverRoomId)
    {
        var game = new
        {
            status = "waiting",
            maxPlayers = maxPlayers,
            createdAt = DateTime.UtcNow.ToString("o"),
            creatorId = creatorId,
            currentPhase = "night",
            roundNumber = 1,
            currentPlayerCount = 1, // Người tạo phòng là người chơi đầu tiên
            roomId = serverRoomId, // Thêm trường roomId là id từ server'
            phaseStartTime = DateTime.UtcNow.ToString("o")
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
            .PutAsync<string>(role);
    }

    // Thêm log sự kiện game
    public async Task AddGameLog(string gameId, string message, string phase)
    {
        var log = new
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            message = message,
            phase = phase
        };

        await firebase
            .Child("gameLogs")
            .Child(gameId)
            .PostAsync(log);
    }
    public static async Task SaveUsernameAsync(string userId, string username, string email, string idToken)
    {
        var firebase = new FirebaseClient(
            "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(idToken)
            });
        await firebase
            .Child("users")
            .Child(userId)
            .PutAsync(new { username = username, email = email });
    }

    public static async Task<string> GetUsernameAsync(string userId, string idToken)
    {
        var firebase = new FirebaseClient(
            "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(idToken)
            });
        var user = await firebase
            .Child("users")
            .Child(userId)
            .OnceSingleAsync<dynamic>();
        return user?.username ?? string.Empty;
    }
    public async Task WereWolfAction(string gameId, string werewwolfId, string tagetplayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase!="night")
        {
            throw new InvalidOperationException("Sói chỉ có thể chọn vào ban đêm");
        }
        await firebase
            .Child("game")
            .Child(gameId)
            .Child("player")
            .Child(werewwolfId)
            .Child("VoteFor")
            .PutAsync(tagetplayerId);
        await AddGameLog(gameId, $"Werewolf {werewwolfId} has voted to kill {tagetplayerId}", "night");
    }


    // Trong FirebaseHelper.cs
    public async Task ProcessNightResults(string gameId)
    {
        try
        {
            var players = await GetPlayers(gameId);
            var werewolves = players.Where(p => p.Role == "werewolf" && p.IsAlive).ToList();
            var alivePlayers = players.Where(p => p.IsAlive).ToList();

            // Tính toán vote
            var votes = werewolves
                .Where(w => !string.IsNullOrEmpty(w.VotedFor))
                .GroupBy(w => w.VotedFor)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (votes != null && votes.Any())
            {
                string targetId = votes.Key;
                await firebase
                    .Child("games")
                    .Child(gameId)
                    .Child("players")
                    .Child(targetId)
                    .Child("IsAlive")
                    .PutAsync(false);

                await AddGameLog(gameId, $"Player {targetId} was killed by werewolves!", "night");
            }
            else
            {
                await AddGameLog(gameId, "No one was killed tonight - werewolves didn't vote!", "night");
            }

            // Reset votes
            foreach (var werewolf in werewolves)
            {
                await firebase
                    .Child("games")
                    .Child(gameId)
                    .Child("players")
                    .Child(werewolf.Id)
                    .Child("VotedFor")
                    .PutAsync(null);
            }

            // Kiểm tra điều kiện thắng sau khi xử lý kết quả đêm
            await CheckGameEndCondition(gameId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in ProcessNightResults: {ex.Message}");
        }
    }

    public async Task CheckGameEndCondition(string gameId)
    {
        try
        {
            var players = await GetPlayers(gameId);
            var aliveWerewolves = players.Count(p => p.IsAlive && p.Role == "werewolf");
            var aliveVillagers = players.Count(p => p.IsAlive && p.Role != "werewolf");

            if (aliveWerewolves == 0)
            {
                await firebase
                    .Child($"games/{gameId}/status")
                    .PutAsync($"\"villagers_win\"");
            }
            else if (aliveWerewolves >= aliveVillagers)
            {
                await firebase
                    .Child($"games/{gameId}/status")
                    .PutAsync($"\"werewolves_win\"");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in CheckGameEndCondition: {ex.Message}");
        }
    }

    // === TIÊN TRI (SEER) ===
    /// Hành động kiểm tra vai trò của người chơi khác (cho Tiên tri)
    public async Task SeerCheckPlayer(string gameId, string seerId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "night")
        {
            throw new InvalidOperationException("Tiên tri chỉ có thể kiểm tra vào ban đêm");
        }

        var targetPlayer = (await GetPlayers(gameId)).FirstOrDefault(p => p.Id == targetPlayerId);
        if (targetPlayer == null)
        {
            throw new ArgumentException("Người chơi không tồn tại");
        }

        // Lưu kết quả kiểm tra cho Tiên tri
        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(seerId)
            .Child("CheckedPlayer")
            .PutAsync(new
            {
                PlayerId = targetPlayerId,
                Role = targetPlayer.Role,
                IsWerewolf = targetPlayer.Role == "werewolf"
            });

        await AddGameLog(gameId, $"Seer {seerId} has checked player {targetPlayerId}", "night");
    }

    // === PHÙ THỦY (WITCH) ===
    /// Phù thủy sử dụng thuốc cứu
    public async Task WitchUseHealPotion(string gameId, string witchId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "night")
        {
            throw new InvalidOperationException("Phù thủy chỉ có thể dùng thuốc vào ban đêm");
        }

        var witch = (await GetPlayers(gameId)).FirstOrDefault(p => p.Id == witchId);
        if (witch == null || witch.Role != "witch")
        {
            throw new ArgumentException("Người chơi không phải là Phù thủy");
        }

        if (witch.HasUsedHealPotion)
        {
            throw new InvalidOperationException("Phù thủy đã dùng hết thuốc cứu");
        }

        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(witchId)
            .Child("HasUsedHealPotion")
            .PutAsync(true);

        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(targetPlayerId)
            .Child("IsProtectedByWitch")
            .PutAsync(true);

        await AddGameLog(gameId, $"Witch {witchId} has used heal potion on {targetPlayerId}", "night");
    }

    /// Phù thủy sử dụng thuốc độc
    public async Task WitchUseKillPotion(string gameId, string witchId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "night")
        {
            throw new InvalidOperationException("Phù thủy chỉ có thể dùng thuốc vào ban đêm");
        }

        var witch = (await GetPlayers(gameId)).FirstOrDefault(p => p.Id == witchId);
        if (witch == null || witch.Role != "witch")
        {
            throw new ArgumentException("Người chơi không phải là Phù thủy");
        }

        if (witch.HasUsedKillPotion)
        {
            throw new InvalidOperationException("Phù thủy đã dùng hết thuốc độc");
        }

        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(witchId)
            .Child("HasUsedKillPotion")
            .PutAsync(true);

        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(targetPlayerId)
            .Child("IsPoisonedByWitch")
            .PutAsync(true);

        await AddGameLog(gameId, $"Witch {witchId} has used kill potion on {targetPlayerId}", "night");
    }

    // === BẢO VỆ (BODYGUARD) ===
    /// Bảo vệ chọn người để bảo vệ
    public async Task BodyguardProtectPlayer(string gameId, string bodyguardId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "night")
        {
            throw new InvalidOperationException("Bảo vệ chỉ có thể bảo vệ vào ban đêm");
        }

        var bodyguard = (await GetPlayers(gameId)).FirstOrDefault(p => p.Id == bodyguardId);
        if (bodyguard == null || bodyguard.Role != "bodyguard")
        {
            throw new ArgumentException("Người chơi không phải là Bảo vệ");
        }

        // Không thể bảo vệ cùng người 2 đêm liên tiếp
        if (bodyguard.LastProtectedPlayerId == targetPlayerId)
        {
            throw new InvalidOperationException("Bảo vệ không thể bảo vệ cùng người 2 đêm liên tiếp");
        }

        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(bodyguardId)
            .Child("ProtectedPlayerId")
            .PutAsync(targetPlayerId);

        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(bodyguardId)
            .Child("LastProtectedPlayerId")
            .PutAsync(targetPlayerId);

        await AddGameLog(gameId, $"Bodyguard {bodyguardId} is protecting {targetPlayerId}", "night");
    }

    // === THỢ SĂN (HUNTER) ===
    /// Thợ săn bắn ai đó khi chết (ban ngày)
    public async Task HunterShootPlayer(string gameId, string hunterId, string targetPlayerId)
    {
        var game = await GetGame(gameId);
        if (game.CurrentPhase != "day")
        {
            throw new InvalidOperationException("Thợ săn chỉ có thể bắn vào ban ngày khi bị treo cổ");
        }

        var hunter = (await GetPlayers(gameId)).FirstOrDefault(p => p.Id == hunterId);
        if (hunter == null || hunter.Role != "hunter")
        {
            throw new ArgumentException("Người chơi không phải là Thợ săn");
        }

        if (hunter.IsAlive || !hunter.CanShoot)
        {
            throw new InvalidOperationException("Thợ săn chỉ có thể bắn khi bị giết");
        }

        var targetPlayer = (await GetPlayers(gameId)).FirstOrDefault(p => p.Id == targetPlayerId);
        if (targetPlayer == null || !targetPlayer.IsAlive)
        {
            throw new ArgumentException("Mục tiêu không hợp lệ");
        }

        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(targetPlayerId)
            .Child("IsAlive")
            .PutAsync(false);

        await AddGameLog(gameId, $"Hunter {hunterId} has shot {targetPlayerId} before dying!", "day");
    }

    public async Task JoinGame(string gameId, Player player)
    {
        // 1. Thêm người chơi vào danh sách players
        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(player.Id)
            .PutAsync(player);

        // 2. Tăng currentPlayerCount
        var game = await GetGame(gameId);
        await firebase
            .Child("games")
            .Child(gameId)
            .Child("currentPlayerCount")
            .PutAsync(game.CurrentPlayerCount + 1);
    }

    public async Task LeaveGame(string gameId, string playerId)
    {
        // Xóa player khỏi danh sách
        await firebase
            .Child("games")
            .Child(gameId)
            .Child("players")
            .Child(playerId)
            .DeleteAsync();

        // Giảm currentPlayerCount
        var game = await GetGame(gameId);
        int newCount = Math.Max(0, game.CurrentPlayerCount - 1);
        await firebase
            .Child("games")
            .Child(gameId)
            .Child("currentPlayerCount")
            .PutAsync(newCount);
    }

    public async Task<string> NextPhase(string gameId, string currentPhase)
    {
        try
        {
            string nextPhase = "night";
            switch (currentPhase)
            {
                case "night":
                    nextPhase = "day_discussion";
                    break;
                case "day_discussion":
                    nextPhase = "day_vote";
                    break;
                case "day_vote":
                    nextPhase = "night";
                    break;
                default:
                    nextPhase = "night";
                    break;
            }

            // Update phase trước
            await firebase
                .Child($"games/{gameId}/currentPhase")
                .PutAsync($"\"{nextPhase}\"");  // Gửi dưới dạng JSON string
            
            // Sau đó update time
            var newStartTime = DateTime.UtcNow.ToString("o");
            await firebase
                .Child($"games/{gameId}/phaseStartTime")
                .PutAsync($"\"{newStartTime}\"");  // Gửi dưới dạng JSON string

            // Kiểm tra điều kiện thắng sau mỗi lần chuyển phase
            await CheckGameEndCondition(gameId);

            // Kiểm tra status game trực tiếp
            var game = await GetGame(gameId);
            if (!string.IsNullOrEmpty(game.Status))
            {
                return game.Status;
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in NextPhase: {ex.Message}");
        }
    }
}
