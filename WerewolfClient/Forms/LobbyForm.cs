using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WerewolfClient;
using WerewolfClient.Models;
using WerewolfClient.Services;
using System.Net.Sockets;

namespace WerewolfClient.Forms
{
    public partial class LobbyForm : Form
    {
        private string _email; // Renamed the private field to avoid ambiguity  
        private FirebaseHelper _firebaseHelper;
        private void LobbyForm_Resize(object sender, EventArgs e)
        {
            int margin = 50;

            // Căn chữ WEREWOLF sát phải nhưng không bị tràn
            int gameNameX = this.ClientSize.Width - gameName.Width - margin;
            gameName.Location = new Point(gameNameX, gameName.Location.Y);

            // Tính vị trí nút sao cho nằm giữa dưới chữ WEREWOLF
            int centerX = gameName.Location.X + (gameName.Width - btnCreateRoom.Width) / 2;

            btnCreateRoom.Location = new Point(centerX, btnCreateRoom.Location.Y);
            btnFindRoom.Location = new Point(centerX, btnFindRoom.Location.Y);
            btnRoles.Location = new Point(centerX, btnRoles.Location.Y);
            btnSetting.Location = new Point(centerX, btnSetting.Location.Y);
        }
        public LobbyForm(string email)
        {
            InitializeComponent();
            this._email = email; // Updated to use the renamed private field  
            _firebaseHelper = new FirebaseHelper();
            playerName.Text = CurrentUserManager.CurrentUser.Username;
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Resize += LobbyForm_Resize;
            this.WindowState = FormWindowState.Maximized;

        }

        private void btnRoles_Click(object sender, EventArgs e)
        {
            RolesForm rolesForm = new RolesForm();
            rolesForm.ShowDialog();
        }

       private async void btnCreateRoom_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Kết nối tới server
                TcpClient client = new TcpClient("localhost", 8888);
                NetworkStream stream = client.GetStream();

                // 2. Gửi CREATE_ROOM:<roomName>:<creatorName>
                string roomName = "Phong Ma Soi"; // hoặc cho người dùng nhập tên phòng
                string creatorName = CurrentUserManager.CurrentUser.Username;
                string createRoomMsg = $"CREATE_ROOM:{roomName}:{creatorName}\n";
                byte[] data = Encoding.UTF8.GetBytes(createRoomMsg);
                stream.Write(data, 0, data.Length);

                // 3. Nhận phản hồi ROOM_CREATED:<roomId>
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                string roomCreatedLine = lines.FirstOrDefault(l => l.StartsWith("ROOM_CREATED:"));
                if (roomCreatedLine != null)
                {
                    // Lấy roomId từ server để lưu vào Firebase và truyền cho form
                    string serverRoomId = roomCreatedLine.Split(':')[1];

                    // 4. Lưu vào Firebase (tạo game mới, không có trường name)
                    string firebaseRoomId = await _firebaseHelper.CreateGame(8, CurrentUserManager.CurrentUser.Id, serverRoomId);

                    // 5. Thêm người chơi đầu tiên vào game
                    var player = new Player
                    {
                        Id = CurrentUserManager.CurrentUser.Id,
                        UserId = CurrentUserManager.CurrentUser.Id,
                        Name = CurrentUserManager.CurrentUser.Username,
                        Role = "villager",
                        IsAlive = true,
                        IsConnected = true,
                        IsReady = true
                    };
                    await _firebaseHelper.AddPlayer(firebaseRoomId, player);

                    // 6. Hiện GameRoomForm (truyền roomId từ server và client đã mở)
                    this.Hide();
                    GameRoomForm gameRoomForm = new GameRoomForm(creatorName, serverRoomId, true, client);
                    gameRoomForm.FormClosed += (s, args) => this.Show();
                    gameRoomForm.Show();
                }
                else
                {
                    MessageBox.Show("Không tạo được phòng!\nServer response: " + response, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo phòng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnFindRoom_Click(object sender, EventArgs e)
        {
            using (Form inputForm = new Form())
            {
                inputForm.Text = "Tìm phòng";
                inputForm.Size = new Size(300, 150);
                inputForm.StartPosition = FormStartPosition.CenterScreen;

                Label label = new Label();
                label.Text = "Nhập ID phòng:";
                label.Location = new Point(20, 20);
                label.AutoSize = true;

                TextBox textBox = new TextBox();
                textBox.Location = new Point(20, 50);
                textBox.Size = new Size(240, 20);

                Button okButton = new Button();
                okButton.Text = "OK";
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new Point(100, 80);

                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(okButton);

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string roomId = textBox.Text.Trim();
                    if (!string.IsNullOrEmpty(roomId))
                    {
                        try
                        {
                            // Kết nối tới server và gửi JOIN_ROOM
                            TcpClient client = new TcpClient("localhost", 8888);
                            NetworkStream stream = client.GetStream();
                            string joinMsg = $"JOIN_ROOM:{roomId}:{CurrentUserManager.CurrentUser.Username}\n";
                            byte[] data = Encoding.UTF8.GetBytes(joinMsg);
                            stream.Write(data, 0, data.Length);

                            // Chờ phản hồi từ server
                            byte[] buffer = new byte[1024];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                            string firstLine = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                            if (firstLine == "JOIN_SUCCESS")
                            {
                                try
                                {
                                    // 1. Tìm gameId từ roomId (lọc ở client)
                                    var games = await _firebaseHelper.firebase
                                        .Child("games")
                                        .OnceAsync<Game>();
                                    
                                    var game = games.FirstOrDefault(g => g.Object.RoomId == roomId);
                                    if (game != null)
                                    {
                                        // 2. Tạo player object
                                        var player = new Player
                                        {
                                            Id = CurrentUserManager.CurrentUser.Id,
                                            UserId = CurrentUserManager.CurrentUser.Id,
                                            Name = CurrentUserManager.CurrentUser.Username,
                                            Role = "villager",
                                            IsAlive = true,
                                            IsConnected = true,
                                            IsReady = true
                                        };

                                        // 3. Join game
                                        await _firebaseHelper.JoinGame(game.Key, player);

                                        // 4. Mở GameRoomForm
                                        this.Hide();
                                        GameRoomForm gameRoomForm = new GameRoomForm(CurrentUserManager.CurrentUser.Username, roomId, false, client);
                                        gameRoomForm.FormClosed += (s, args) => this.Show();
                                        gameRoomForm.Show();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Không tìm thấy phòng game với ID này!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        client.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Lỗi khi tham gia phòng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    client.Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Phòng không tồn tại hoặc không thể tham gia!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                client.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Không thể vào phòng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Xử lý đăng xuất khỏi Firebase nếu cần  
            // FirebaseAuthHelper.SignOut();  

            this.Hide();
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }

        private void LobbyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
