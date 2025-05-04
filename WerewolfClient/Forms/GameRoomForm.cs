using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace WerewolfClient.Forms
{
    public partial class GameRoomForm : Form
    {
        private string playerName;
        private string roomCode;
        private bool isHost;
        private List<string> players = new List<string>();
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected = false;
        private SynchronizationContext uiContext;

        public GameRoomForm(string playerName, string roomCode, bool isHost, TcpClient existingClient = null)
        {
            InitializeComponent();
            this.playerName = playerName;
            this.roomCode = roomCode;
            this.isHost = isHost;
            lblRoomId.Text = $"Mã phòng: {roomCode}";
            lblPlayerName.Text = playerName;
            uiContext = SynchronizationContext.Current;
            
            try 
            {
                ConnectToServer(existingClient);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối đến server: {ex.Message}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
        }

        private void ConnectToServer(TcpClient existingClient = null)
        {
            try
            {
                if (existingClient != null)
                {
                    client = existingClient;
                    stream = client.GetStream();
                    isConnected = true;
                }
                else
                {
                    client = new TcpClient("localhost", 8888);
                    stream = client.GetStream();
                    isConnected = true;
                    SendMessage($"JOIN_ROOM:{roomCode}:{playerName}");
                }

                // Start receive thread
                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                InitializePlayers();
            }
            catch (Exception ex)
            {
                isConnected = false;
                if (client != null)
                {
                    try
                    {
                        client.Close();
                    }
                    catch { }
                }
                MessageBox.Show($"Không thể kết nối đến server: {ex.Message}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void SendMessage(string message)
        {
            if (!isConnected || client == null || stream == null) return;

            try
            {
                if (!client.Connected)
                {
                    isConnected = false;
                    return;
                }

                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                isConnected = false;
                if (!this.IsDisposed)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"Lỗi khi gửi tin nhắn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[4096];
            int bytesRead;

            try
            {
                while (isConnected)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        var messages = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var msg in messages)
                        {
                            if (!string.IsNullOrWhiteSpace(msg))
                            {
                                string finalMsg = msg.Trim();
                                uiContext.Post(_ => ProcessServerMessage(finalMsg), null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (isConnected)
                {
                    uiContext.Post(_ => 
                    {
                        MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                    }, null);
                }
            }
        }

        private void ProcessServerMessage(string message)
        {
            string[] parts = message.Split(':');
            string command = parts[0];

            switch (command)
            {
                case "PLAYER_JOINED":
                    if (parts.Length >= 2)
                    {
                        string newPlayer = parts[1];
                        OnPlayerJoined(newPlayer);
                    }
                    break;

                case "PLAYER_LEFT":
                    if (parts.Length >= 2)
                    {
                        string leftPlayer = parts[1];
                        OnPlayerLeft(leftPlayer);
                    }
                    break;

                case "PLAYER_LIST":
                    if (parts.Length >= 2)
                    {
                        string[] playerList = parts[1].Split(',');
                        players = playerList.ToList();
                        UpdatePlayerList();
                    }
                    break;

                case "CHAT_MESSAGE":
                    if (parts.Length >= 3)
                    {
                        string sender = parts[1];
                        string msg = string.Join(":", parts.Skip(2));
                        OnChatMessageReceived(sender, msg);
                    }
                    break;

                case "GAME_STARTED":
                    OnGameStarted();
                    break;
            }
        }

        private void InitializePlayers()
        {
            players.Add(playerName);
            UpdatePlayerList();
            AddChatMessage("Hệ thống", $"Bạn đã tham gia phòng {roomCode}");
        }

        private void UpdatePlayerList()
        {
            lstPlayers.Items.Clear();
            foreach (var player in players)
            {
                lstPlayers.Items.Add(player);
            }
            lblPlayerCount.Text = $"Số người chơi: {players.Count}";
        }

        private void AddChatMessage(string sender, string message)
        {
            string formattedMessage = $"[{DateTime.Now:HH:mm}] {sender}: {message}";
            txtChat.AppendText(formattedMessage + Environment.NewLine);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();

            if (!string.IsNullOrEmpty(message))
            {
                SendMessage($"CHAT_MESSAGE:{roomCode}:{playerName}:{message}");
                txtMessage.Clear();
            }
        }

        private void btnLeaveRoom_Click(object sender, EventArgs e)
        {
          isConnected = false;
            if (client != null)
            {
                try
                {
                    SendMessage($"LEAVE_ROOM:{roomCode}:{playerName}");
                    client.Close();
                }
                catch { }
            }
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join(500);
            }
            this.Close();
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {   
             if (!isHost)
            {
                MessageBox.Show("Chỉ chủ phòng mới có thể bắt đầu game", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (players.Count < 4)
            {
                MessageBox.Show("Cần ít nhất 4 người chơi để bắt đầu game",
                    "Không thể bắt đầu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gửi yêu cầu bắt đầu game tới server
            // NetworkManager.StartGame(roomCode, playerName);

             SendMessage($"START_GAME:{roomCode}");

            // Chuyển sang màn hình game chính
            // this.Hide();
            // new GamePlayForm(playerName, roomCode, players).Show();
            // this.Close();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSendChat.PerformClick();
            }
        }

        private void GameRoomForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                isConnected = false;
                if (client != null)
                {
                    try
                    {
                        SendMessage($"LEAVE_ROOM:{roomCode}:{playerName}");
                    }
                    catch { }
                    
                    try
                    {
                        client.Close();
                    }
                    catch { }
                }
                
                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Join(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during form closing: {ex.Message}");
            }
        }

        // Các phương thức xử lý sự kiện từ server
        public void OnPlayerJoined(string playerName)
        {
            players.Add(playerName);
            UpdatePlayerList();
            AddChatMessage("Hệ thống", $"{playerName} đã tham gia phòng");
        }

        public void OnPlayerLeft(string playerName)
        {
            players.Remove(playerName);
            UpdatePlayerList();
            AddChatMessage("Hệ thống", $"{playerName} đã rời khỏi phòng");
        }

        public void OnChatMessageReceived(string sender, string message)
        {
            AddChatMessage(sender, message);
        }

        public void OnGameStarted()
        {
            MessageBox.Show("Trò chơi bắt đầu!", "Werewolf",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Chuyển sang màn hình game chính
            // this.Hide();
            // new GamePlayForm(playerName, roomCode, players).Show();
            // this.Close();
        }
    }
}