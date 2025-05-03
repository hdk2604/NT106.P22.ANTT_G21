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

namespace WerewolfClient.Forms
{
    public partial class LobbyForm : Form
    {
        private string _email; // Renamed the private field to avoid ambiguity  
        private FirebaseHelper _firebaseHelper;

        public LobbyForm(string email)
        {
            InitializeComponent();
            this._email = email; // Updated to use the renamed private field  
            _firebaseHelper = new FirebaseHelper();
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
                // Generate random 6-digit room ID
                Random random = new Random();
                string roomId = random.Next(100000, 999999).ToString();

                // Create room using FirebaseHelper with current user's ID
                await _firebaseHelper.CreateGame(roomId, 8, CurrentUserManager.CurrentUser.Id);

                MessageBox.Show($"Phòng {roomId} đã được tạo thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo phòng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFindRoom_Click(object sender, EventArgs e)
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
                    string roomId = textBox.Text;
                    if (!string.IsNullOrEmpty(roomId))
                    {
                        // TODO: Implement find room logic
                        MessageBox.Show($"Đang tìm phòng {roomId}...", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }
    }
}
