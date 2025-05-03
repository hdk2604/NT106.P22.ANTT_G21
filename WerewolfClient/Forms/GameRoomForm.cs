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

namespace WerewolfClient.Forms
{
    public partial class GameRoomForm : Form
    {
        private string playerName;
        private string roomCode;
        private bool isHost;
        private List<string> players = new List<string>();

        public GameRoomForm(string playerName, string roomCode, bool isHost)
        {
            InitializeComponent();
            this.playerName = playerName;
            this.roomCode = roomCode;
            this.isHost = isHost;
            InitializePlayers();
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
                AddChatMessage(playerName, message);
                txtMessage.Clear();

                // Gửi tin nhắn tới server
                // NetworkManager.SendChatMessage(roomCode, playerName, message);
            }
        }

        private void btnLeaveRoom_Click(object sender, EventArgs e)
        {
            // Thông báo rời phòng
            // NetworkManager.LeaveRoom(roomCode, playerName);

            AddChatMessage("Hệ thống", $"Bạn đã rời khỏi phòng {roomCode}");

            this.Hide();
            new LobbyForm(playerName).Show();
            this.Close();
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            if (players.Count < 4)
            {
                MessageBox.Show("Cần ít nhất 4 người chơi để bắt đầu game",
                    "Không thể bắt đầu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gửi yêu cầu bắt đầu game tới server
            // NetworkManager.StartGame(roomCode, playerName);

            AddChatMessage("Hệ thống", "Trò chơi đang bắt đầu...");

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
            if (e.CloseReason == CloseReason.UserClosing)
            {
                btnLeaveRoom.PerformClick();
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