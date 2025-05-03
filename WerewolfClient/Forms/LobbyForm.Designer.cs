using System.Drawing;

namespace WerewolfClient.Forms
{
    partial class LobbyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LobbyForm));
            this.btnCreateRoom = new System.Windows.Forms.Button();
            this.btnFindRoom = new System.Windows.Forms.Button();
            this.gameName = new System.Windows.Forms.Label();
            this.btnSetting = new System.Windows.Forms.Button();
            this.btnRoles = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.playerInfo = new System.Windows.Forms.Label();
            this.btnFriends = new System.Windows.Forms.Button();
            this.playerName = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCreateRoom
            // 
            this.btnCreateRoom.BackColor = System.Drawing.Color.Transparent;
            this.btnCreateRoom.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreateRoom.FlatAppearance.BorderColor = System.Drawing.Color.LightCoral;
            this.btnCreateRoom.FlatAppearance.BorderSize = 2;
            this.btnCreateRoom.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
            this.btnCreateRoom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnCreateRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreateRoom.Font = new System.Drawing.Font("Georgia", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCreateRoom.ForeColor = System.Drawing.Color.DarkRed;
            this.btnCreateRoom.Location = new System.Drawing.Point(1170, 306);
            this.btnCreateRoom.Margin = new System.Windows.Forms.Padding(2);
            this.btnCreateRoom.Name = "btnCreateRoom";
            this.btnCreateRoom.Size = new System.Drawing.Size(280, 61);
            this.btnCreateRoom.TabIndex = 0;
            this.btnCreateRoom.Text = "TẠO PHÒNG";
            this.btnCreateRoom.UseVisualStyleBackColor = false;
            this.btnCreateRoom.Click += new System.EventHandler(this.btnCreateRoom_Click);
            // 
            // btnFindRoom
            // 
            this.btnFindRoom.BackColor = System.Drawing.Color.Transparent;
            this.btnFindRoom.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFindRoom.FlatAppearance.BorderColor = System.Drawing.Color.LightCoral;
            this.btnFindRoom.FlatAppearance.BorderSize = 2;
            this.btnFindRoom.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
            this.btnFindRoom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnFindRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFindRoom.Font = new System.Drawing.Font("Georgia", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFindRoom.ForeColor = System.Drawing.Color.DarkRed;
            this.btnFindRoom.Location = new System.Drawing.Point(1170, 405);
            this.btnFindRoom.Margin = new System.Windows.Forms.Padding(2);
            this.btnFindRoom.Name = "btnFindRoom";
            this.btnFindRoom.Size = new System.Drawing.Size(280, 61);
            this.btnFindRoom.TabIndex = 0;
            this.btnFindRoom.Text = "TÌM PHÒNG";
            this.btnFindRoom.UseVisualStyleBackColor = false;
            this.btnFindRoom.Click += new System.EventHandler(this.btnFindRoom_Click);
            // 
            // gameName
            // 
            this.gameName.AutoSize = true;
            this.gameName.BackColor = System.Drawing.Color.Transparent;
            this.gameName.Font = new System.Drawing.Font("Palatino Linotype", 42F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameName.ForeColor = System.Drawing.Color.Maroon;
            this.gameName.Location = new System.Drawing.Point(1113, 182);
            this.gameName.Name = "gameName";
            this.gameName.Size = new System.Drawing.Size(342, 76);
            this.gameName.TabIndex = 1;
            this.gameName.Text = "Werewolves";
            // 
            // btnSetting
            // 
            this.btnSetting.BackColor = System.Drawing.Color.Transparent;
            this.btnSetting.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetting.FlatAppearance.BorderColor = System.Drawing.Color.LightCoral;
            this.btnSetting.FlatAppearance.BorderSize = 2;
            this.btnSetting.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
            this.btnSetting.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetting.Font = new System.Drawing.Font("Georgia", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetting.ForeColor = System.Drawing.Color.DarkRed;
            this.btnSetting.Location = new System.Drawing.Point(1170, 603);
            this.btnSetting.Margin = new System.Windows.Forms.Padding(2);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(280, 61);
            this.btnSetting.TabIndex = 2;
            this.btnSetting.Text = "SETTING";
            this.btnSetting.UseVisualStyleBackColor = false;
            // 
            // btnRoles
            // 
            this.btnRoles.BackColor = System.Drawing.Color.Transparent;
            this.btnRoles.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRoles.FlatAppearance.BorderColor = System.Drawing.Color.LightCoral;
            this.btnRoles.FlatAppearance.BorderSize = 2;
            this.btnRoles.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
            this.btnRoles.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnRoles.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRoles.Font = new System.Drawing.Font("Georgia", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRoles.ForeColor = System.Drawing.Color.DarkRed;
            this.btnRoles.Location = new System.Drawing.Point(1170, 504);
            this.btnRoles.Margin = new System.Windows.Forms.Padding(2);
            this.btnRoles.Name = "btnRoles";
            this.btnRoles.Size = new System.Drawing.Size(280, 61);
            this.btnRoles.TabIndex = 3;
            this.btnRoles.Text = "ROLES";
            this.btnRoles.UseVisualStyleBackColor = false;
            this.btnRoles.Click += new System.EventHandler(this.btnRoles_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.playerInfo);
            this.panel1.Controls.Add(this.btnFriends);
            this.panel1.Controls.Add(this.playerName);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(324, 164);
            this.panel1.TabIndex = 4;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::WerewolfClient.Properties.Resources._1329565;
            this.pictureBox1.Location = new System.Drawing.Point(156, 34);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(143, 110);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // playerInfo
            // 
            this.playerInfo.AutoSize = true;
            this.playerInfo.Font = new System.Drawing.Font("Georgia", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playerInfo.Location = new System.Drawing.Point(68, 8);
            this.playerInfo.Name = "playerInfo";
            this.playerInfo.Size = new System.Drawing.Size(198, 23);
            this.playerInfo.TabIndex = 2;
            this.playerInfo.Text = "Player Information";
            // 
            // btnFriends
            // 
            this.btnFriends.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.btnFriends.Font = new System.Drawing.Font("Georgia", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFriends.Location = new System.Drawing.Point(14, 74);
            this.btnFriends.Name = "btnFriends";
            this.btnFriends.Size = new System.Drawing.Size(75, 23);
            this.btnFriends.TabIndex = 1;
            this.btnFriends.Text = "Friends";
            this.btnFriends.UseVisualStyleBackColor = false;
            // 
            // playerName
            // 
            this.playerName.AutoSize = true;
            this.playerName.Font = new System.Drawing.Font("Georgia", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playerName.Location = new System.Drawing.Point(11, 40);
            this.playerName.Name = "playerName";
            this.playerName.Size = new System.Drawing.Size(100, 18);
            this.playerName.TabIndex = 0;
            this.playerName.Text = "Player Name";
            // 
            // LobbyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1520, 941);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnRoles);
            this.Controls.Add(this.btnSetting);
            this.Controls.Add(this.gameName);
            this.Controls.Add(this.btnFindRoom);
            this.Controls.Add(this.btnCreateRoom);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "LobbyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LobbyForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCreateRoom;
        private System.Windows.Forms.Button btnFindRoom;
        private System.Windows.Forms.Label gameName;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.Button btnRoles;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label playerName;
        private System.Windows.Forms.Label playerInfo;
        private System.Windows.Forms.Button btnFriends;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}