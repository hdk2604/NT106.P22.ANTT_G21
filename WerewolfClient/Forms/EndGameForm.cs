using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WerewolfClient.Models; // CurrentUserManager is in WerewolfClient.Models
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace WerewolfClient.Forms
{
    public partial class EndGameForm : Form
    {
        // Define a structure or use existing Player model for player data
        // Assuming Player model from WerewolfClient.Models includes Name, Role, IsAlive
        private List<Models.Player> gamePlayers;
        private string gameResultText;
        private string gameStatsText;
        private string roomCode;
        private bool isHost;
        private TcpClient client;
        private Panel loadingOverlay;
        private Label loadingLabel;
        private bool isResizing = false;
        private System.Windows.Forms.Timer resizeTimer;
        private Panel overlayPanel;
        private Label resultLabel;
        private FlowLayoutPanel playerListPanel;
        private Button exitButton;
        private InGameForm inGameFormRef;
        private string gameId;

        public EndGameForm(string resultText, List<Models.Player> players, string statsText, string gameId, string roomCode, bool isHost, TcpClient client)
        {
            InitializeComponent();
            this.gameResultText = resultText;
            this.gamePlayers = players;
            this.gameStatsText = statsText;
            this.gameId = gameId;
            this.roomCode = roomCode;
            this.isHost = isHost;
            this.client = client;
            
            // Set form to fullscreen
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            
            // Initialize loading overlay
            InitializeLoadingOverlay();
            
            // Initialize resize timer
            InitializeResizeTimer();
            
            this.Load += EndGameForm_Load;
            this.Shown += EndGameForm_Shown;
            this.ResizeBegin += EndGameForm_ResizeBegin;
            this.ResizeEnd += EndGameForm_ResizeEnd;
        }

        private void InitializeResizeTimer()
        {
            resizeTimer = new System.Windows.Forms.Timer();
            resizeTimer.Interval = 150; // 150ms delay
            resizeTimer.Tick += ResizeTimer_Tick;
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            if (isResizing)
            {
                ShowLoading();
                try
                {
                    RefreshPlayerPanels();
                }
                finally
                {
                    HideLoading();
                    isResizing = false;
                }
            }
        }

        private void EndGameForm_ResizeBegin(object sender, EventArgs e)
        {
            isResizing = true;
            resizeTimer.Stop();
        }

        private void EndGameForm_ResizeEnd(object sender, EventArgs e)
        {
            resizeTimer.Start();
        }

        private void RefreshPlayerPanels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshPlayerPanels));
                return;
            }

            // Store current scroll position
            var scrollPosition = tableLayoutPanelPlayers.VerticalScroll.Value;

            // Refresh the panels
            InitializePlayerPanels();

            // Restore scroll position
            tableLayoutPanelPlayers.VerticalScroll.Value = Math.Min(scrollPosition, tableLayoutPanelPlayers.VerticalScroll.Maximum);
        }

        private void InitializeLoadingOverlay()
        {
            loadingOverlay = new Panel();
            loadingOverlay.Dock = DockStyle.Fill;
            loadingOverlay.BackColor = Color.FromArgb(200, 0, 0, 0);
            loadingOverlay.Visible = false;

            loadingLabel = new Label();
            loadingLabel.Text = "Đang tải dữ liệu...";
            loadingLabel.Dock = DockStyle.Fill;
            loadingLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            loadingLabel.ForeColor = Color.White;
            loadingLabel.TextAlign = ContentAlignment.MiddleCenter;

            loadingOverlay.Controls.Add(loadingLabel);
            this.Controls.Add(loadingOverlay);
            loadingOverlay.BringToFront();
        }

        private void ShowLoading()
        {
            if (loadingOverlay.InvokeRequired)
            {
                loadingOverlay.Invoke(new Action(ShowLoading));
                return;
            }
            loadingOverlay.Visible = true;
            loadingOverlay.BringToFront();
        }

        private void HideLoading()
        {
            if (loadingOverlay.InvokeRequired)
            {
                loadingOverlay.Invoke(new Action(HideLoading));
                return;
            }
            loadingOverlay.Visible = false;
        }

        private async void EndGameForm_Load(object sender, EventArgs e)
        {
            try
            {
                ShowLoading();
                // Lấy dữ liệu người chơi mới nhất từ Firebase
                var firebase = new FirebaseHelper();
                var latestPlayers = await firebase.GetPlayers(gameId);
                if (latestPlayers != null)
                {
                    gamePlayers = latestPlayers;
                }
                // Hiển thị danh sách tên người chơi lấy được
                if (gamePlayers != null && gamePlayers.Count > 0)
                {
                    string names = string.Join(", ", gamePlayers.Select(p => p.Name));
                    MessageBox.Show($"Danh sách người chơi: {names}", "DEBUG: Players");
                    InitializePlayerPanels();
                }
                else
                {
                    MessageBox.Show("Không lấy được người chơi nào!", "DEBUG: Players");
                    InitializePlayerPanels();
                }

                // Make controls transparent to show background image
                labelResult.BackColor = Color.Transparent;
                panelStats.BackColor = Color.Transparent;
                labelStats.BackColor = Color.Transparent;
                panelButtons.BackColor = Color.Transparent;
                buttonBackToLobby.BackColor = Color.Transparent;
                buttonExit.BackColor = Color.Transparent;
                // Note: Making ListView transparent reliably is complex. 
                // Its background might still obscure the form background.

                // Set background image based on result
                if (gameResultText.ToLower().Contains("sói chiến thắng"))
                {
                    this.BackgroundImage = Properties.Resources.WereWolfWin;
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else if (gameResultText.ToLower().Contains("dân làng chiến thắng"))
                {
                    this.BackgroundImage = Properties.Resources.Day;
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else
                {
                    MessageBox.Show($"Result: {gameResultText}, No matching background.");
                }

                // Set the result text
                labelResult.Text = gameResultText;

                // Populate the player panels
                await Task.Run(() => InitializePlayerPanels());

                // Set the stats text
                labelStats.Text = "Thống kê: " + gameStatsText;

                // Wire up button click events
                buttonBackToLobby.Click += ButtonBackToLobby_Click;
                buttonExit.Click += ButtonExit_Click;
            }
            finally
            {
                HideLoading();
            }
        }

        private void InitializePlayerPanels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(InitializePlayerPanels));
                return;
            }

            tableLayoutPanelPlayers.SuspendLayout();
            try
            {
                tableLayoutPanelPlayers.Controls.Clear();

                // Add header panel
                Panel headerPanel = new Panel();
                headerPanel.Dock = DockStyle.Fill;
                headerPanel.BackColor = Color.FromArgb(50, 0, 0, 0);
                headerPanel.BorderStyle = BorderStyle.FixedSingle;

                TableLayoutPanel headerTLP = new TableLayoutPanel();
                headerTLP.Dock = DockStyle.Fill;
                headerTLP.ColumnCount = 3;
                headerTLP.RowCount = 1;
                headerTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
                headerTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
                headerTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                headerPanel.Controls.Add(headerTLP);

                // Header Labels
                Label nameHeader = new Label();
                nameHeader.Text = "Tên người chơi";
                nameHeader.Dock = DockStyle.Fill;
                nameHeader.TextAlign = ContentAlignment.MiddleLeft;
                nameHeader.Padding = new Padding(10, 0, 0, 0);
                nameHeader.ForeColor = Color.Gold;
                nameHeader.Font = new Font(nameHeader.Font, FontStyle.Bold);
                headerTLP.Controls.Add(nameHeader, 0, 0);

                Label roleHeader = new Label();
                roleHeader.Text = "Vai trò";
                roleHeader.Dock = DockStyle.Fill;
                roleHeader.TextAlign = ContentAlignment.MiddleLeft;
                roleHeader.ForeColor = Color.Gold;
                roleHeader.Font = new Font(roleHeader.Font, FontStyle.Bold);
                headerTLP.Controls.Add(roleHeader, 1, 0);

                Label statusHeader = new Label();
                statusHeader.Text = "Trạng thái";
                statusHeader.Dock = DockStyle.Fill;
                statusHeader.TextAlign = ContentAlignment.MiddleLeft;
                statusHeader.ForeColor = Color.Gold;
                statusHeader.Font = new Font(statusHeader.Font, FontStyle.Bold);
                headerTLP.Controls.Add(statusHeader, 2, 0);

                tableLayoutPanelPlayers.Controls.Add(headerPanel, 0, 0);

                // Add player panels
                if (gamePlayers != null)
                {
                    for (int i = 0; i < gamePlayers.Count; i++)
                    {
                        var player = gamePlayers[i];
                        Panel p = new Panel();
                        p.Dock = DockStyle.Fill;
                        p.BackColor = Color.FromArgb(30, 0, 0, 0);
                        p.Margin = new Padding(2);
                        p.BorderStyle = BorderStyle.FixedSingle;
                        p.Name = $"panel_{i}";

                        TableLayoutPanel innerTLP = new TableLayoutPanel();
                        innerTLP.Dock = DockStyle.Fill;
                        innerTLP.ColumnCount = 3;
                        innerTLP.RowCount = 1;
                        innerTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
                        innerTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
                        innerTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                        p.Controls.Add(innerTLP);

                        // Name Label
                        Label nameLabel = new Label();
                        nameLabel.Text = player.Name;
                        nameLabel.Dock = DockStyle.Fill;
                        nameLabel.TextAlign = ContentAlignment.MiddleLeft;
                        nameLabel.Padding = new Padding(10, 0, 0, 0);
                        nameLabel.ForeColor = Color.White;
                        nameLabel.Font = new Font("Segoe UI", 18, FontStyle.Bold);
                        innerTLP.Controls.Add(nameLabel, 0, 0);

                        // Role Label
                        Label roleLabel = new Label();
                        roleLabel.Text = player.Role;
                        roleLabel.Dock = DockStyle.Fill;
                        roleLabel.TextAlign = ContentAlignment.MiddleLeft;
                        roleLabel.ForeColor = Color.White;
                        roleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                        innerTLP.Controls.Add(roleLabel, 1, 0);

                        // Status Label
                        Label statusLabel = new Label();
                        statusLabel.Text = player.IsAlive ? "Sống" : "Chết";
                        statusLabel.Dock = DockStyle.Fill;
                        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
                        statusLabel.ForeColor = player.IsAlive ? Color.LightGreen : Color.Red;
                        statusLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                        innerTLP.Controls.Add(statusLabel, 2, 0);

                        tableLayoutPanelPlayers.Controls.Add(p, 0, i + 1);
                    }
                }
            }
            finally
            {
                tableLayoutPanelPlayers.ResumeLayout();
            }
        }

        private void EndGameForm_Shown(object sender, EventArgs e)
        {
            // Additional initialization code if needed
            // Fade in effect
            this.Opacity = 0;
            var timer = new Timer();
            timer.Interval = 20;
            timer.Tick += (s, ev) =>
            {
                if (this.Opacity < 1)
                    this.Opacity += 0.05;
                else
                    timer.Stop();
            };
            timer.Start();
        }

        private void ButtonBackToLobby_Click(object sender, EventArgs e)
        {
            // Gọi cleanup mạng trước khi về lobby
            if (inGameFormRef != null)
            {
                inGameFormRef.CleanupNetworkResources();
            }
            if (client != null)
            {
                client.Close();
            }
            this.Hide();
            LobbyForm lobbyForm = new LobbyForm(CurrentUserManager.CurrentUser?.Email);
            lobbyForm.FormClosed += (sender2, args) => this.Close();
            lobbyForm.Show();
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            // Xóa handler này
        }

        private void buttonBackToLobby_MouseEnter(object sender, EventArgs e)
        {
            buttonBackToLobby.BackColor = Color.FromArgb(0, 102, 184); // Darker blue
        }

        private void buttonBackToLobby_MouseLeave(object sender, EventArgs e)
        {
            buttonBackToLobby.BackColor = Color.FromArgb(0, 122, 204); // Original blue
        }

        private void buttonExit_MouseEnter(object sender, EventArgs e)
        {
            buttonExit.BackColor = Color.FromArgb(172, 0, 0); // Darker red
        }

        private void buttonExit_MouseLeave(object sender, EventArgs e)
        {
            buttonExit.BackColor = Color.FromArgb(192, 0, 0); // Original red
        }

        // Helper for rounded panel
        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        }

        // Helper to get role icon
        private Image GetRoleIcon(string role)
        {
            switch (role?.ToLower())
            {
                case "werewolf": return Properties.Resources.WereWolfIcon;
                case "seer": return Properties.Resources.Seer;
                case "bodyguard": return Properties.Resources.Guardian;
                case "hunter": return Properties.Resources.Hunter;
                case "witch": return Properties.Resources.WitchIcon;
                case "villager": return Properties.Resources.villagerIcon;
                default: return Properties.Resources.villagerIcon;
            }
        }

        public void SetInGameFormRef(InGameForm form)
        {
            inGameFormRef = form;
        }
    }
}
