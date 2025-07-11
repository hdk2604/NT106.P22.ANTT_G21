using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace WerewolfServer
{
    public class GameServer
    {   
        // ==================== PROPERTIES & FIELDS ====================
        private TcpListener _server;
        private static Dictionary<string, Room> _rooms = new Dictionary<string, Room>();
        private static readonly object _roomsLock = new object();
        private int _port = 8888;
        private readonly ConnectionLimiter _connectionLimiter;
        private readonly ConnectionPool _connectionPool;
        private bool _isRunning = false;

        // ==================== CONSTRUCTOR ====================
        public GameServer()
        {
            _connectionLimiter = new ConnectionLimiter(maxConnections: 1000);
            _connectionPool = new ConnectionPool();
        }

        
        // ==================== MAIN SERVER METHODS ====================
        public async Task StartAsync()
        {
            Console.WriteLine("[DEBUG] StartAsync() bắt đầu");

            _server = new TcpListener(IPAddress.Any, _port);
            _server.Start();
            _isRunning = true;
            Console.WriteLine("[DEBUG] TcpListener đã Start()");

            Console.WriteLine($"=== WEREWOLF SERVER STARTED ===");
            Console.WriteLine($"Port: {_port}");
            Console.WriteLine($"Max Connections: {_connectionLimiter.MaxConnections}");
            Console.WriteLine($"Connection Pool Size: 100");
            Console.WriteLine($"No Timeout - Clients stay connected");
            Console.WriteLine($"================================");

            try
            {
                while (_isRunning)
                {
                    // Chờ connection mới với timeout

                   // var acceptTask = _server.AcceptTcpClientAsync();
                    var timeoutTask = Task.Delay(1000); // 1 second timeout
                    
                 //   var completedTask = await Task.WhenAny(acceptTask, timeoutTask);
                    //
                      //  if (completedTask == acceptTask)
                    {

                        var client = await _server.AcceptTcpClientAsync();
                        // Kiểm tra connection limit
                        if (await _connectionLimiter.TryAcquireConnectionAsync())
                        {

                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await HandleClientAsync(client);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[ERROR] Lỗi khi xử lý client trong Task.Run: {ex.Message}");
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
                _server?.Stop();
                _connectionPool.Cleanup();
            }
        }

        public void Start()
        {
            StartAsync().Wait();
        }

        // ==================== CLIENT HANDLING METHODS ====================
        private async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine("[DEBUG] >>> Đã vào HandleClientAsync");

            NetworkStream stream = client.GetStream();
            Console.WriteLine("[SERVER] New client connected!");

            byte[] buffer = new byte[4096];
            Player player = null;
            StringBuilder receiveBuffer = new StringBuilder();

            try
            {
                                while (client.Connected)
                {
                    // Async read không có timeout
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    Console.WriteLine($"[DEBUG] >>> Đã đọc {bytesRead} bytes");

                    if (bytesRead > 0)
                    {
                        receiveBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                        string content = receiveBuffer.ToString();
                        int newlineIndex;
                        while ((newlineIndex = content.IndexOf('\n')) >= 0)
                        {
                            string completeMessage = content.Substring(0, newlineIndex).Trim();
                            content = content.Substring(newlineIndex + 1);

                            if (!string.IsNullOrEmpty(completeMessage))
                            {
                                player = await ProcessClientMessageAsync(client, completeMessage, player);
                            }
                        }
                        receiveBuffer.Clear().Append(content);
                    }
                    else
                    {
                        // Client đã ngắt kết nối
                        Console.WriteLine("[SERVER] Client disconnected");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                if (player != null && player.CurrentRoom != null)
                {
                    try
                    {
                        player.CurrentRoom.RemovePlayer(player.Name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SERVER] Loi khi remove player {player.Name}: {ex.Message}");
                    }
                }
                
                // Release connection limit
                _connectionLimiter.ReleaseConnection();
                
                // Return connection to pool
                _connectionPool.ReturnConnection(client);
                
                try
                {
                    client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER] Loi khi close client: {ex.Message}");
                }
            }
        }
        
        private void HandleClient(object obj)
        {
            HandleClientAsync((TcpClient)obj).Wait();
        }

        // ==================== MESSAGE PROCESSING METHODS ====================
        private async Task<Player> ProcessClientMessageAsync(TcpClient client, string message, Player player)
        {
            string[] parts = message.Split(':');
            string command = parts[0];
            string response = "";

            try
            {
                switch (command)
                {
                    case "CREATE_ROOM":
                        Console.WriteLine("[SERVER] Bat dau tao phong...");
                        if (parts.Length >= 3)
                        {
                            string creatorName = parts[2];
                            string roomId = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                            Console.WriteLine($"[SERVER] roomId={roomId}, creatorName={creatorName}");
                            try
                            {
                                lock (_roomsLock)
                                {
                                    _rooms[roomId] = new Room(roomId);
                                }
                                Console.WriteLine("[SERVER] Da tao room object");
                                player = new Player(client, creatorName);
                                Console.WriteLine("[SERVER] Da tao player object");
                                response = $"ROOM_CREATED:{roomId}\n";
                                Console.WriteLine("[SERVER] response=" + response);
                                if (!string.IsNullOrEmpty(response))
                                {
                                    byte[] data = Encoding.UTF8.GetBytes(response + "\n");
                                    await client.GetStream().WriteAsync(data, 0, data.Length);
                                }
                                
                                Room room;
                                lock (_roomsLock)
                                {
                                    room = _rooms[roomId];
                                }
                                room.AddPlayer(player);
                                Console.WriteLine("[SERVER] Da add player vào room");
                                response = "";
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[SERVER] Loi khi tao phong: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                        break;
                    case "JOIN_ROOM":
                        if (parts.Length >= 3)
                        {
                            string roomId = parts[1];
                            string joinName = parts[2];
                            
                            Room room;
                            lock (_roomsLock)
                            {
                                if (_rooms.ContainsKey(roomId))
                                {
                                    room = _rooms[roomId];
                                }
                                else
                                {
                                    room = null;
                                }
                            }
                            
                            if (room != null)
                            {
                                if (room.Players.Any(p => p != null && p.Name == joinName))
                                {
                                    return player;
                                }
                                player = new Player(client, joinName);
                                byte[] joinSuccess = Encoding.UTF8.GetBytes("JOIN_SUCCESS\n");
                                await client.GetStream().WriteAsync(joinSuccess, 0, joinSuccess.Length);
                                room.AddPlayer(player);
                                response = "";
                            }
                            else
                            {
                                response = "JOIN_FAIL\n";
                            }
                        }
                        break;
                    case "CHAT_MESSAGE":
                        if (parts.Length >= 4)
                        {
                            string roomId = parts[1];
                            string sender = parts[2];
                            string msg = string.Join(":", parts.Skip(3));

                            Room room;
                            lock (_roomsLock)
                            {
                                if (_rooms.ContainsKey(roomId))
                                {
                                    room = _rooms[roomId];
                                }
                                else
                                {
                                    room = null;
                                }
                            }
                            
                            if (room != null)
                            {
                                Console.WriteLine($"[SERVER] Gui toi {roomId}: {sender}:{msg}");
                                // Chỉ dùng QueueBroadcast, không gọi Broadcast trực tiếp
                                room.QueueBroadcast($"CHAT_MESSAGE:{roomId}:{sender}:{msg}", "");
                            }
                        }
                        break;
                    case "START_GAME":
                        if (parts.Length >= 2)
                        {
                            string roomId = parts[1];
                            
                            Room room;
                            lock (_roomsLock)
                            {
                                if (_rooms.ContainsKey(roomId))
                                {
                                    room = _rooms[roomId];
                                }
                                else
                                {
                                    room = null;
                                }
                            }
                            
                            if (room != null)
                            {
                                // Chỉ dùng QueueBroadcast, không gọi Broadcast trực tiếp
                                room.QueueBroadcast($"GAME_STARTED:{roomId}", "");
                            }
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(response))
                {
                    byte[] data = Encoding.UTF8.GetBytes(response + "\n");
                    await client.GetStream().WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                try
                {
                    byte[] errorData = Encoding.UTF8.GetBytes($"ERROR:{ex.Message}\n");
                    await client.GetStream().WriteAsync(errorData, 0, errorData.Length);
                }
                catch { }
            }
            await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes("JOIN_DUPLICATE\n"));
            return player;
        }

        private void ProcessClientMessage(TcpClient client, string message, ref Player player)
        {
            player = ProcessClientMessageAsync(client, message, player).Result;
        }

        // ==================== UTILITY METHODS ====================
        public static void RemoveRoom(string roomId)
        {
            lock (_roomsLock)
            {
                if (_rooms.ContainsKey(roomId))
                {
                    _rooms.Remove(roomId);
                    Console.WriteLine($"[SERVER] Đã xóa phòng {roomId} vì không còn người chơi.");
                }
            }
        }

        // ==================== INNER CLASSES ====================
        // Room class
        public class Room
        {   
            // ==================== PROPERTIES & FIELDS ===================
            public string Id { get; }
            public List<Player> Players { get; } = new List<Player>();
            private readonly object _lock = new object();
            private readonly Queue<string> _broadcastQueue = new Queue<string>();
            private bool _isBroadcasting = false;
            private static readonly object _globalRoomLock = new object();

            // ==================== CONSTRUCTOR ====================
            public Room(string id)
            {
                Id = id;
            }

            // ==================== PLAYER MANAGEMENT ====================
            public void AddPlayer(Player player)
            {
                if (player == null) return;
                
                lock (_globalRoomLock)
                {
                    bool added = false;
                    List<string> activePlayerNames = null;
                    
                    lock (_lock)
                    {
                        // Kiểm tra xem player đã tồn tại và chưa bị removed chưa
                        var existingPlayer = Players.FirstOrDefault(p => p != null && p.Name == player.Name);
                        if (existingPlayer == null || existingPlayer.IsRemoved)
                        {
                            // Nếu player chưa tồn tại hoặc đã bị removed, thêm mới
                            if (existingPlayer != null)
                            {
                                // Nếu player đã tồn tại nhưng bị removed, reset lại
                                existingPlayer.IsRemoved = false;
                                existingPlayer.CurrentRoom = this;
                            }
                            else
                            {
                                // Thêm player mới
                                player.CurrentRoom = this;
                                Players.Add(player);
                            }
                            added = true;
                            
                            // Lấy danh sách player đang active (chưa bị removed)
                            activePlayerNames = Players
                                .Where(p => p != null && !p.IsRemoved)
                                .Select(p => p.Name)
                                .ToList();
                        }
                    }
                    
                    if (added)
                    {
                        QueueBroadcast($"PLAYER_JOINED:{player.Name}", "");
                        if (activePlayerNames != null)
                            QueueBroadcast($"PLAYER_LIST:{string.Join(",", activePlayerNames)}", "");
                    }
                }
            }

            public void RemovePlayer(string playerName)
            {
                if (string.IsNullOrEmpty(playerName)) return;

                lock (_globalRoomLock)
                {
                    Player playerToRemove = null;
                    List<string> remainingPlayerNames = null;

                    lock (_lock)
                    {
                        playerToRemove = Players.FirstOrDefault(p => p != null && p.Name == playerName);
                        if (playerToRemove != null)
                        {
                            // Đánh dấu player là removed
                            playerToRemove.IsRemoved = true;
                            
                            // Lấy danh sách player còn lại (chưa bị removed)
                            remainingPlayerNames = Players
                                .Where(p => p != null && !p.IsRemoved)
                                .Select(p => p.Name)
                                .ToList();
                        }
                    }

                    if (playerToRemove != null)
                    {
                        // Broadcast thông báo player left
                        QueueBroadcast($"PLAYER_LEFT:{playerName}", "");
                        
                        // Broadcast danh sách player còn lại
                        if (remainingPlayerNames != null && remainingPlayerNames.Count > 0)
                        {
                            QueueBroadcast($"PLAYER_LIST:{string.Join(",", remainingPlayerNames)}", "");
                        }
                        else
                        {
                            // Nếu không còn player nào, có thể xóa room
                            QueueBroadcast($"PLAYER_LIST:", "");
                        }
                    }
                }
            }

            // ==================== BROADCASTING SYSTEM ====================
            public void QueueBroadcast(string message, string excludePlayerName)
            {
                lock (_globalRoomLock)
                {
                    lock (_lock)
                    {
                        _broadcastQueue.Enqueue($"{message}|{excludePlayerName}");
                    }
                    
                    if (!_isBroadcasting)
                    {
                        _ = Task.Run(async () => await ProcessBroadcastQueueAsync());
                    }
                }
            }

            private async Task ProcessBroadcastQueueAsync()
            {
                lock (_globalRoomLock)
                {
                    lock (_lock)
                    {
                        if (_isBroadcasting || _broadcastQueue.Count == 0)
                        {
                            return;
                        }
                        _isBroadcasting = true;
                    }
                }

                try
                {
                    while (true)
                    {
                        string queuedMessage = null;
                        lock (_globalRoomLock)
                        {
                            lock (_lock)
                            {
                                                        if (_broadcastQueue.Count > 0)
                        {
                            queuedMessage = _broadcastQueue.Dequeue();
                        }
                        else
                        {
                            break;
                        }
                            }
                        }

                        if (queuedMessage != null)
                        {
                            var parts = queuedMessage.Split('|');
                            if (parts.Length == 2)
                            {
                                await BroadcastInternalAsync(parts[0], parts[1]);
                            }
                        }
                    }
                }
                finally
                {
                    lock (_globalRoomLock)
                    {
                        lock (_lock)
                        {
                            _isBroadcasting = false;
                        }
                    }
                }
            }

            // Method cũ để backward compatibility
            private void ProcessBroadcastQueue()
            {
                ProcessBroadcastQueueAsync().Wait();
            }
            // ... các broadcast methods khác
            private async Task BroadcastInternalAsync(string message, string excludePlayerName)
            {
                List<Player> playersCopy;
                lock (_lock)
                {
                    // Tạo copy hoàn toàn để tránh modify trong lúc enumerate
                    playersCopy = new List<Player>();
                    foreach (var p in Players)
                    {
                        if (p != null && !p.IsRemoved)
                        {
                            playersCopy.Add(p);
                        }
                    }
                }

                // Tạo tasks cho tất cả broadcast operations
                var broadcastTasks = new List<Task>();
                
                foreach (var player in playersCopy)
                {
                    if (player != null && player.Name != excludePlayerName)
                    {
                        broadcastTasks.Add(BroadcastToPlayerAsync(player, message));
                    }
                }

                // Chờ tất cả broadcast hoàn thành
                await Task.WhenAll(broadcastTasks);
                
                // Dọn dẹp player đã rời sau khi broadcast xong
                lock (_lock)
                {
                    Players.RemoveAll(p => p == null || p.IsRemoved);
                }
            }

            private async Task BroadcastToPlayerAsync(Player player, string message)
            {
                try
                {
                    if (player != null && player.Client != null && player.Client.Connected)
                    {
                        Console.WriteLine($"[SERVER] Gui toi {player.Name}: {message}");
                        byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                        await player.Client.GetStream().WriteAsync(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER] Loi gui toi {player?.Name ?? "Unknown"}: {ex.Message}");
                }
            }

            // Method cũ để backward compatibility
            private void BroadcastInternal(string message, string excludePlayerName)
            {
                BroadcastInternalAsync(message, excludePlayerName).Wait();
            }

            public List<string> GetPlayerNames()
            {
                lock (_lock)
                {
                    return Players.Where(p => p != null && !p.IsRemoved).Select(p => p.Name).ToList();
                }
            }

            // Method để cleanup những player đã bị removed
            public void CleanupRemovedPlayers()
            {
                lock (_lock)
                {
                    Players.RemoveAll(p => p != null && p.IsRemoved);
                }
            }
        }
        // Player class
        public class Player
        {
            public TcpClient Client { get; }
            public string Name { get; }
            public Room CurrentRoom { get; set; }
            public bool IsRemoved { get; set; } = false;

            public Player(TcpClient client, string name)
            {
                Client = client;
                Name = name;
            }
        }
        // ConnectionLimiter class
        public class ConnectionLimiter
        {
            private readonly SemaphoreSlim _connectionSemaphore;
            private readonly int _maxConnections;
            private int _currentConnections = 0;

            public ConnectionLimiter(int maxConnections)
            {
                _maxConnections = maxConnections;
                _connectionSemaphore = new SemaphoreSlim(maxConnections, maxConnections);
            }

            public async Task<bool> TryAcquireConnectionAsync(int timeoutMs = 1000)
            {
                try
                {
                    bool acquired = await _connectionSemaphore.WaitAsync(timeoutMs);
                    if (acquired)
                    {
                        Interlocked.Increment(ref _currentConnections);
                    }
                    return acquired;
                }
                catch
                {
                    return false;
                }
            }

            public void ReleaseConnection()
            {
                _connectionSemaphore.Release();
                Interlocked.Decrement(ref _currentConnections);
            }

            public int CurrentConnections => _currentConnections;
            public int MaxConnections => _maxConnections;
        }

        // ConnectionPool class
        public class ConnectionPool
        {
            private readonly ConcurrentQueue<TcpClient> _pool = new ConcurrentQueue<TcpClient>();
            private readonly int _maxPoolSize = 100;
            private int _currentPoolSize = 0;

            public async Task<TcpClient> GetConnectionAsync()
            {
                if (_pool.TryDequeue(out var client))
                {
                    return client;
                }

                var newClient = new TcpClient();
                return newClient;
            }

            public void ReturnConnection(TcpClient client)
            {
                if (client != null && _currentPoolSize < _maxPoolSize)
                {
                    try
                    {
                        // Reset connection state
                        if (client.Connected)
                        {
                            client.Close();
                        }
                        
                        _pool.Enqueue(client);
                        Interlocked.Increment(ref _currentPoolSize);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[POOL] Error returning connection: {ex.Message}");
                    }
                }
            }

            public void Cleanup()
            {
                while (_pool.TryDequeue(out var client))
                {
                    try
                    {
                        client?.Close();
                        client?.Dispose();
                    }
                    catch { }
                }
                _currentPoolSize = 0;
            }
        }

    }
}