﻿using System;
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
        private static Dictionary<string, Room> _rooms = new Dictionary<string, Room>();
        private int _port = 8888;

        public void Start()
        {
            _server = new TcpListener(IPAddress.Any, _port);
            _server.Start();
            Console.WriteLine($"Server da duoc tao tren port {_port}...");

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
            StringBuilder receiveBuffer = new StringBuilder();

            try
            {
                while (client.Connected)
                {

                    bytesRead = stream.Read(buffer, 0, buffer.Length);
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
                                ProcessClientMessage(client, completeMessage, ref player);
                            }
                        }
                        receiveBuffer.Clear().Append(content); // giữ lại phần còn lại chưa đủ
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
                        Console.WriteLine("[SERVER] Bat dau tao phong...");
                        if (parts.Length >= 3)
                        {
                            string creatorName = parts[2];
                            string roomId = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                            Console.WriteLine($"[SERVER] roomId={roomId}, creatorName={creatorName}");
                            try
                            {
                                _rooms[roomId] = new Room(roomId);
                                Console.WriteLine("[SERVER] Da tao room object");
                                player = new Player(client, creatorName);
                                Console.WriteLine("[SERVER] Da tao player object");
                                response = $"ROOM_CREATED:{roomId}";
                                Console.WriteLine("[SERVER] response=" + response);
                                if (!string.IsNullOrEmpty(response))
                                {
                                    byte[] data = Encoding.UTF8.GetBytes(response + "\n");
                                    client.GetStream().Write(data, 0, data.Length);
                                }
                                _rooms[roomId].AddPlayer(player);
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
                            if (_rooms.ContainsKey(roomId))
                            {
                                player = new Player(client, joinName);
                                byte[] joinSuccess = Encoding.UTF8.GetBytes("JOIN_SUCCESS\n");
                                client.GetStream().Write(joinSuccess, 0, joinSuccess.Length);
                                _rooms[roomId].AddPlayer(player);
                                response = "";
                            }
                            else
                            {
                                response = "JOIN_FAIL";
                            }
                        }
                        break;
                    case "CHAT_MESSAGE":
                        if (parts.Length >= 4)
                        {
                            string roomId = parts[1];
                            string sender = parts[2];
                            string msg = string.Join(":", parts.Skip(3)); // hỗ trợ dấu ":" trong tin nhắn

                            if (_rooms.ContainsKey(roomId))
                            {
                                Console.WriteLine($"[SERVER] Gui toi {roomId}: {sender}:{msg}");
                                _rooms[roomId].Broadcast($"CHAT_MESSAGE:{roomId}:{sender}:{msg}", "");
                            }
                        }
                        break;
                    case "START_GAME":
                        if (parts.Length >= 2)
                        {
                            string roomId = parts[1];
                            if (_rooms.ContainsKey(roomId))
                            {
                                _rooms[roomId].Broadcast($"GAME_STARTED:{roomId}", "");
                            }
                        }
                        break;
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
            public List<Player> Players { get; } = new List<Player>();

            public Room(string id)
            {
                Id = id;
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
                if (Players.Count == 0)
                {
                    GameServer.RemoveRoom(Id);
                }
            }

            public void Broadcast(string message, string excludePlayerName)
            {
                var disconnectedPlayers = new List<Player>();
                foreach (var player in Players)
                {
                    try
                    {
                        if (player.Name != excludePlayerName)
                        {
                            if (player.Client.Connected)
                            {
                                Console.WriteLine($"[SERVER] Gui toi {player.Name}: {message}");
                                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                                player.Client.GetStream().Write(data, 0, data.Length);
                            }
                            else
                            {
                                disconnectedPlayers.Add(player);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SERVER] Loi gui toi {player.Name}: {ex.Message}");
                        disconnectedPlayers.Add(player);
                    }
                }
                foreach (var p in disconnectedPlayers)
                {
                    Players.Remove(p);
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

        public static void RemoveRoom(string roomId)
        {
            if (_rooms.ContainsKey(roomId))
            {
                _rooms.Remove(roomId);
                Console.WriteLine($"[SERVER] Đã xóa phòng {roomId} vì không còn người chơi.");
            }
        }
    }
}