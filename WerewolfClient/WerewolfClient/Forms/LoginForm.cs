using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WereWolf
{
    public partial class LoginForm: Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

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
              //  new MainGameForm().Show(); // Mở form game chính
            }
        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            new RegisterForm().Show();
        }
    }
}
