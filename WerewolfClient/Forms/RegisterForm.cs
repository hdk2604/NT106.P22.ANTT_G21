﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WerewolfClient.Forms
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
            txtPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;
            panelRegister.Anchor = AnchorStyles.None;

        }
        private void RegisterForm_Resize(object sender, EventArgs e)
        {
            panelRegister.Location = new Point(
                (this.ClientSize.Width - panelRegister.Width) / 2,
                (this.ClientSize.Height - panelRegister.Height) / 2
            );
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            this.Resize += RegisterForm_Resize;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;

        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            bool isSuccess = await FirebaseAuthHelper.RegisterAsync(email, password, username);
            if (isSuccess)
            {
                MessageBox.Show("Đăng ký thành công!");
                this.Hide();
                new LoginForm().Show();
            }
        }

        private void linkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            new LoginForm().Show();
        }

        private void lblLoginPrompt_Click(object sender, EventArgs e)
        {

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
