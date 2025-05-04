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
            roomId = serverRoomId // Thêm trường roomId là id từ server
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
            .Child("role")
            .PutAsync(role);
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
        var players = await GetPlayers(gameId);
        var werewolves = players.Where(p => p.Role == "werewolf" && p.IsAlive).ToList();
        var alivePlayers = players.Where(p => p.IsAlive).ToList();

        // Tính toán vote
        var votes = werewolves
            .Where(w => !string.IsNullOrEmpty(w.VotedFor))
            .GroupBy(w => w.VotedFor)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (votes != null)
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
    }
}
