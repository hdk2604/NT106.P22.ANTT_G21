using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WerewolfServer
{
    public class GameServer
    {
        private TcpListener _server;
        private Dictionary<string, Room> _rooms = new Dictionary<string, Room>();
        private int _port = 8888;

        public void Start()
        {
            _server = new TcpListener(IPAddress.Any, _port);
            _server.Start();
            Console.WriteLine($"Server started on port {_port}...");

            while (true)
            {
                TcpClient client = _server.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];
            int bytesRead;
            Player player = null;

            try
            {
                while (client.Connected)
                {
                    if (!stream.DataAvailable)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        Console.WriteLine($"Received: {message}");

                        string[] messages = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string msg in messages)
                        {
                            ProcessClientMessage(client, msg, ref player);
                        }
                    }
                    else
                    {
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
                    player.CurrentRoom.RemovePlayer(player.Name);
                }
                client.Close();
            }
        }

        private void ProcessClientMessage(TcpClient client, string message, ref Player player)
        {
            string[] parts = message.Split(':');
            string command = parts[0];
            string response = "";

            try
            {
                switch (command)
                {
                    case "CREATE_ROOM":
                        if (parts.Length >= 3)
                        {
                            string roomName = parts[1];
                            string creatorName = parts[2];
                            string roomId = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                            _rooms[roomId] = new Room(roomId, roomName);

                            player = new Player(client, creatorName);
                            _rooms[roomId].AddPlayer(player);

                            response = $"ROOM_CREATED:{roomId}";
                        }
                        break;

                        // Các case khác giữ nguyên nhưng thêm kiểm tra độ dài parts
                        // ...
                }

                if (!string.IsNullOrEmpty(response))
                {
                    byte[] data = Encoding.UTF8.GetBytes(response + "\n");
                    client.GetStream().Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                try
                {
                    byte[] errorData = Encoding.UTF8.GetBytes($"ERROR:{ex.Message}\n");
                    client.GetStream().Write(errorData, 0, errorData.Length);
                }
                catch { }
            }
        }

        public class Room
    {
        public string Id { get; }
        public string Name { get; }
        public List<Player> Players { get; } = new List<Player>();

        public Room(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public void AddPlayer(Player player)
        {
            player.CurrentRoom = this;
            Players.Add(player);
            Broadcast($"PLAYER_JOINED:{player.Name}", "");
            Broadcast($"PLAYER_LIST:{string.Join(",", GetPlayerNames())}", "");
        }

        public void RemovePlayer(string playerName)
        {
            var player = Players.Find(p => p.Name == playerName);
            if (player != null)
            {
                Players.Remove(player);
                Broadcast($"PLAYER_LEFT:{playerName}", "");
                Broadcast($"PLAYER_LIST:{string.Join(",", GetPlayerNames())}", "");
            }
        }

        public void Broadcast(string message, string excludePlayerName)
        {
            foreach (var player in Players)
            {
                try
                {
                    if (player.Name != excludePlayerName)
                    {
                        byte[] data = Encoding.UTF8.GetBytes(message);
                        player.Client.GetStream().Write(data, 0, data.Length);
                    }
                }
                catch { /* Bỏ qua lỗi gửi */ }
            }
        }

        public List<string> GetPlayerNames()
        {
            return Players.ConvertAll(p => p.Name);
        }
    }

    public class Player
    {
        public TcpClient Client { get; }
        public string Name { get; }
        public Room CurrentRoom { get; set; }

        public Player(TcpClient client, string name)
        {
            Client = client;
            Name = name;
        }
    }
    }
}