namespace WerewolfClient.Forms
{
    partial class GameRoomForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblRoomId = new System.Windows.Forms.Label();
            this.lblPlayerCount = new System.Windows.Forms.Label();
            this.lstPlayers = new System.Windows.Forms.ListBox();
            this.txtChat = new System.Windows.Forms.RichTextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSendChat = new System.Windows.Forms.Button();
            this.btnStartGame = new System.Windows.Forms.Button();
            this.btnLeaveRoom = new System.Windows.Forms.Button();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlChatContainer = new System.Windows.Forms.Panel();
            this.pnlChatInput = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.pnlPlayerList = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlChatContainer.SuspendLayout();
            this.pnlChatInput.SuspendLayout();
            this.pnlPlayerList.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRoomId
            // 
            this.lblRoomId.AutoSize = true;
            this.lblRoomId.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRoomId.ForeColor = System.Drawing.Color.White;
            this.lblRoomId.Location = new System.Drawing.Point(30, 20);
            this.lblRoomId.Name = "lblRoomId";
            this.lblRoomId.Size = new System.Drawing.Size(164, 38);
            this.lblRoomId.TabIndex = 0;
            this.lblRoomId.Text = "Mã phòng: ";
            // 
            // lblPlayerCount
            // 
            this.lblPlayerCount.AutoSize = true;
            this.lblPlayerCount.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlayerCount.ForeColor = System.Drawing.Color.White;
            this.lblPlayerCount.Location = new System.Drawing.Point(30, 70);
            this.lblPlayerCount.Name = "lblPlayerCount";
            this.lblPlayerCount.Size = new System.Drawing.Size(174, 32);
            this.lblPlayerCount.TabIndex = 1;
            this.lblPlayerCount.Text = "Số người chơi: ";
            // 
            // lstPlayers
            // 
            this.lstPlayers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.lstPlayers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstPlayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstPlayers.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstPlayers.FormattingEnabled = true;
            this.lstPlayers.ItemHeight = 30;
            this.lstPlayers.Location = new System.Drawing.Point(15, 50);
            this.lstPlayers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstPlayers.Name = "lstPlayers";
            this.lstPlayers.Size = new System.Drawing.Size(370, 565);
            this.lstPlayers.TabIndex = 2;
            // 
            // txtChat
            // 
            this.txtChat.BackColor = System.Drawing.Color.White;
            this.txtChat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtChat.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtChat.Location = new System.Drawing.Point(15, 50);
            this.txtChat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtChat.Name = "txtChat";
            this.txtChat.ReadOnly = true;
            this.txtChat.Size = new System.Drawing.Size(770, 505);
            this.txtChat.TabIndex = 3;
            this.txtChat.Text = "";
            // 
            // txtMessage
            // 
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtMessage.Location = new System.Drawing.Point(5, 5);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(10);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(658, 48);
            this.txtMessage.TabIndex = 0;
            // 
            // btnSendChat
            // 
            this.btnSendChat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(255)))));
            this.btnSendChat.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSendChat.FlatAppearance.BorderSize = 0;
            this.btnSendChat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendChat.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendChat.ForeColor = System.Drawing.Color.White;
            this.btnSendChat.Location = new System.Drawing.Point(663, 5);
            this.btnSendChat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSendChat.Name = "btnSendChat";
            this.btnSendChat.Size = new System.Drawing.Size(100, 48);
            this.btnSendChat.TabIndex = 5;
            this.btnSendChat.Text = "Gửi";
            this.btnSendChat.UseVisualStyleBackColor = false;
            this.btnSendChat.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnStartGame
            // 
            this.btnStartGame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnStartGame.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnStartGame.FlatAppearance.BorderSize = 0;
            this.btnStartGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartGame.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartGame.ForeColor = System.Drawing.Color.White;
            this.btnStartGame.Location = new System.Drawing.Point(0, 0);
            this.btnStartGame.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(400, 80);
            this.btnStartGame.TabIndex = 6;
            this.btnStartGame.Text = "BẮT ĐẦU TRÒ CHƠI";
            this.btnStartGame.UseVisualStyleBackColor = false;
            this.btnStartGame.Click += new System.EventHandler(this.btnStartGame_Click);
            // 
            // btnLeaveRoom
            // 
            this.btnLeaveRoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.btnLeaveRoom.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnLeaveRoom.FlatAppearance.BorderSize = 0;
            this.btnLeaveRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeaveRoom.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLeaveRoom.ForeColor = System.Drawing.Color.White;
            this.btnLeaveRoom.Location = new System.Drawing.Point(800, 0);
            this.btnLeaveRoom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLeaveRoom.Name = "btnLeaveRoom";
            this.btnLeaveRoom.Size = new System.Drawing.Size(400, 80);
            this.btnLeaveRoom.TabIndex = 7;
            this.btnLeaveRoom.Text = "RỜI PHÒNG";
            this.btnLeaveRoom.UseVisualStyleBackColor = false;
            this.btnLeaveRoom.Click += new System.EventHandler(this.btnLeaveRoom_Click);
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.pnlHeader.Controls.Add(this.lblPlayerName);
            this.pnlHeader.Controls.Add(this.lblPlayerCount);
            this.pnlHeader.Controls.Add(this.lblRoomId);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(30, 20, 30, 20);
            this.pnlHeader.Size = new System.Drawing.Size(1200, 120);
            this.pnlHeader.TabIndex = 8;
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.pnlChatContainer);
            this.pnlMain.Controls.Add(this.pnlPlayerList);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 120);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1200, 630);
            this.pnlMain.TabIndex = 9;
            // 
            // pnlChatContainer
            // 
            this.pnlChatContainer.BackColor = System.Drawing.Color.White;
            this.pnlChatContainer.Controls.Add(this.txtChat);
            this.pnlChatContainer.Controls.Add(this.pnlChatInput);
            this.pnlChatContainer.Controls.Add(this.label2);
            this.pnlChatContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlChatContainer.Location = new System.Drawing.Point(400, 0);
            this.pnlChatContainer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlChatContainer.Name = "pnlChatContainer";
            this.pnlChatContainer.Padding = new System.Windows.Forms.Padding(15);
            this.pnlChatContainer.Size = new System.Drawing.Size(800, 630);
            this.pnlChatContainer.TabIndex = 1;
            // 
            // pnlChatInput
            // 
            this.pnlChatInput.BackColor = System.Drawing.Color.White;
            this.pnlChatInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlChatInput.Controls.Add(this.txtMessage);
            this.pnlChatInput.Controls.Add(this.btnSendChat);
            this.pnlChatInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlChatInput.Location = new System.Drawing.Point(15, 555);
            this.pnlChatInput.Name = "pnlChatInput";
            this.pnlChatInput.Padding = new System.Windows.Forms.Padding(5);
            this.pnlChatInput.Size = new System.Drawing.Size(770, 60);
            this.pnlChatInput.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(15, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(770, 35);
            this.label2.TabIndex = 5;
            this.label2.Text = "TRÒ CHUYỆN";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlPlayerList
            // 
            this.pnlPlayerList.BackColor = System.Drawing.Color.White;
            this.pnlPlayerList.Controls.Add(this.lstPlayers);
            this.pnlPlayerList.Controls.Add(this.label1);
            this.pnlPlayerList.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlPlayerList.Location = new System.Drawing.Point(0, 0);
            this.pnlPlayerList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlPlayerList.Name = "pnlPlayerList";
            this.pnlPlayerList.Padding = new System.Windows.Forms.Padding(15);
            this.pnlPlayerList.Size = new System.Drawing.Size(400, 630);
            this.pnlPlayerList.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(370, 35);
            this.label1.TabIndex = 3;
            this.label1.Text = "DANH SÁCH NGƯỜI CHƠI";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlFooter
            // 
            this.pnlFooter.Controls.Add(this.btnLeaveRoom);
            this.pnlFooter.Controls.Add(this.btnStartGame);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 750);
            this.pnlFooter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1200, 80);
            this.pnlFooter.TabIndex = 10;
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Font = new System.Drawing.Font("Segoe UI", 15F);
            this.lblPlayerName.ForeColor = System.Drawing.SystemColors.Control;
            this.lblPlayerName.Location = new System.Drawing.Point(1015, 32);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(64, 41);
            this.lblPlayerName.TabIndex = 2;
            this.lblPlayerName.Text = "Tên";
            // 
            // GameRoomForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1200, 830);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameRoomForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Phòng chơi Ma Sói";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.pnlChatContainer.ResumeLayout(false);
            this.pnlChatInput.ResumeLayout(false);
            this.pnlChatInput.PerformLayout();
            this.pnlPlayerList.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblRoomId;
        private System.Windows.Forms.Label lblPlayerCount;
        private System.Windows.Forms.ListBox lstPlayers;
        private System.Windows.Forms.RichTextBox txtChat;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSendChat;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.Button btnLeaveRoom;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Panel pnlChatContainer;
        private System.Windows.Forms.Panel pnlPlayerList;
        private System.Windows.Forms.Panel pnlChatInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblPlayerName;
    }
}