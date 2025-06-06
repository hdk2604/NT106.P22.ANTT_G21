using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WerewolfClient.Models;

namespace WerewolfClient.Forms
{
    public partial class LoginForm: Form
    {
        public LoginForm()
        {
            InitializeComponent();
            txtPassword.UseSystemPasswordChar = true;
            panelLogin.Anchor = AnchorStyles.None;

            this.Resize += LoginForm_Resize;
        }
        private void LoginForm_Resize(object sender, EventArgs e)
        {
            panelLogin.Location = new Point(
         (this.ClientSize.Width - panelLogin.Width) / 2,
         (this.ClientSize.Height - panelLogin.Height) / 2
     );
        }
        private void LoginForm_Load(object sender, EventArgs e)
        {
            this.Resize += LoginForm_Resize;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ email và mật khẩu!");
                return;
            }

            bool isSuccess = await FirebaseAuthHelper.LoginAsync(email, password);
            if (isSuccess)
            {
                MessageBox.Show("Đăng nhập thành công!");
                this.Hide();
                new LobbyForm(CurrentUserManager.CurrentUser.Email).Show(); // Sử dụng email từ CurrentUserManager
            }
        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            new RegisterForm().Show();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // Đóng ứng dụng
            Application.Exit();

        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            // Thu nhỏ cửa sổ xuống thanh taskbar
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
