namespace WerewolfClient.Forms
{
    partial class EndGameForm
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
            this.labelResult = new System.Windows.Forms.Label();
            this.tableLayoutPanelPlayers = new System.Windows.Forms.TableLayoutPanel();
            this.panelStats = new System.Windows.Forms.Panel();
            this.labelStats = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonBackToLobby = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.panelLoadingOverlay = new System.Windows.Forms.Panel();
            this.labelLoading = new System.Windows.Forms.Label();
            this.panelStats.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.panelLoadingOverlay.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelResult
            // 
            this.labelResult.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelResult.Font = new System.Drawing.Font("Segoe UI", 28.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult.ForeColor = System.Drawing.Color.Gold;
            this.labelResult.Location = new System.Drawing.Point(0, 0);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(1124, 80);
            this.labelResult.TabIndex = 0;
            this.labelResult.Text = "Kết quả trận đấu";
            this.labelResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanelPlayers
            // 
            this.tableLayoutPanelPlayers.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelPlayers.ColumnCount = 1;
            this.tableLayoutPanelPlayers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelPlayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelPlayers.Location = new System.Drawing.Point(0, 80);
            this.tableLayoutPanelPlayers.Name = "tableLayoutPanelPlayers";
            this.tableLayoutPanelPlayers.RowCount = 9;
            this.tableLayoutPanelPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanelPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanelPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanelPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanelPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanelPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanelPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanelPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanelPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanelPlayers.Size = new System.Drawing.Size(1124, 373);
            this.tableLayoutPanelPlayers.TabIndex = 1;
            // 
            // panelStats
            // 
            this.panelStats.Controls.Add(this.labelStats);
            this.panelStats.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStats.Location = new System.Drawing.Point(0, 453);
            this.panelStats.Name = "panelStats";
            this.panelStats.Size = new System.Drawing.Size(1124, 80);
            this.panelStats.TabIndex = 2;
            // 
            // labelStats
            // 
            this.labelStats.AutoSize = true;
            this.labelStats.ForeColor = System.Drawing.Color.White;
            this.labelStats.Location = new System.Drawing.Point(12, 10);
            this.labelStats.Name = "labelStats";
            this.labelStats.Size = new System.Drawing.Size(70, 16);
            this.labelStats.TabIndex = 0;
            this.labelStats.Text = "Thống kê: ";
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.buttonBackToLobby);
            this.panelButtons.Controls.Add(this.buttonExit);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 533);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(1124, 67);
            this.panelButtons.TabIndex = 3;
            // 
            // buttonBackToLobby
            // 
            this.buttonBackToLobby.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.buttonBackToLobby.FlatAppearance.BorderSize = 0;
            this.buttonBackToLobby.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBackToLobby.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.buttonBackToLobby.ForeColor = System.Drawing.Color.White;
            this.buttonBackToLobby.Location = new System.Drawing.Point(412, 13);
            this.buttonBackToLobby.Name = "buttonBackToLobby";
            this.buttonBackToLobby.Size = new System.Drawing.Size(150, 40);
            this.buttonBackToLobby.TabIndex = 0;
            this.buttonBackToLobby.Text = "Về phòng chờ";
            this.buttonBackToLobby.UseVisualStyleBackColor = false;
            this.buttonBackToLobby.MouseEnter += new System.EventHandler(this.buttonBackToLobby_MouseEnter);
            this.buttonBackToLobby.MouseLeave += new System.EventHandler(this.buttonBackToLobby_MouseLeave);
            // 
            // buttonExit
            // 
            this.buttonExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.buttonExit.FlatAppearance.BorderSize = 0;
            this.buttonExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExit.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.buttonExit.ForeColor = System.Drawing.Color.White;
            this.buttonExit.Location = new System.Drawing.Point(562, 13);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(150, 40);
            this.buttonExit.TabIndex = 1;
            this.buttonExit.Text = "Về Lobby";
            this.buttonExit.UseVisualStyleBackColor = false;
            this.buttonExit.MouseEnter += new System.EventHandler(this.buttonExit_MouseEnter);
            this.buttonExit.MouseLeave += new System.EventHandler(this.buttonExit_MouseLeave);
            // 
            // panelLoadingOverlay
            // 
            this.panelLoadingOverlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(200)))));
            this.panelLoadingOverlay.Controls.Add(this.labelLoading);
            this.panelLoadingOverlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLoadingOverlay.Location = new System.Drawing.Point(0, 0);
            this.panelLoadingOverlay.Name = "panelLoadingOverlay";
            this.panelLoadingOverlay.Size = new System.Drawing.Size(1124, 600);
            this.panelLoadingOverlay.TabIndex = 4;
            // 
            // labelLoading
            // 
            this.labelLoading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelLoading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.labelLoading.ForeColor = System.Drawing.Color.White;
            this.labelLoading.Location = new System.Drawing.Point(0, 0);
            this.labelLoading.Name = "labelLoading";
            this.labelLoading.Size = new System.Drawing.Size(1124, 600);
            this.labelLoading.TabIndex = 0;
            this.labelLoading.Text = "Đang tải dữ liệu...";
            this.labelLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EndGameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1124, 600);
            this.Controls.Add(this.tableLayoutPanelPlayers);
            this.Controls.Add(this.panelStats);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.labelResult);
            this.Controls.Add(this.panelLoadingOverlay);
            this.Name = "EndGameForm";
            this.Text = "Kết thúc trận đấu";
            this.panelStats.ResumeLayout(false);
            this.panelStats.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.panelLoadingOverlay.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label labelResult;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelPlayers;
        private System.Windows.Forms.Panel panelStats;
        private System.Windows.Forms.Label labelStats;
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.Button buttonBackToLobby;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Panel panelLoadingOverlay;
        private System.Windows.Forms.Label labelLoading;
    }
}