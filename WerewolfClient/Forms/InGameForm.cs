using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Database.Query;
using WerewolfClient.Models;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.Linq;

namespace WerewolfClient.Forms
{
    public partial class InGameForm : Form
    {
        private List<string> playerNames = new List<string>();
        private List<string> players = new List<string>();
        public string CurrentUserName { get; set; } = null;
        private string currentUserRole = null;
        private string currentUserId = null;
        private System.Windows.Forms.Timer pollTimer;
        private string currentGameId = null;
        private Dictionary<string, Image> roleIcons = new Dictionary<string, Image>
        {
            { "werewolf", Properties.Resources.WereWolfIcon },
            { "seer", Properties.Resources.Seer },
            { "bodyguard", Properties.Resources.Guardian },
            { "hunter", Properties.Resources.Hunter },
            { "witch", Properties.Resources.WitchIcon },
            { "villager", Properties.Resources.villagerIcon }
        };

        private enum GamePhase { Night, DayDiscussion, DayVote }
        private GamePhase currentPhase = GamePhase.Night;
        private System.Windows.Forms.Timer phaseTimer;
        private DateTime phaseStartTime;
        private IDisposable phaseListener;
        private IDisposable timeListener;
        private bool isHost = false;
        private string gameId = null;
        private int phaseDuration = 30;
        private bool isListenerSetup = false;
        private bool isDataLoaded = false;
        private string lastPhaseRaw = null;
        private string lastStartTimeRaw = null;
        private IDisposable gameStatusListener;
        private bool isConnected = false;
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private SynchronizationContext uiContext;
        private string roomId;
        private StringBuilder receiveBuffer = new StringBuilder();
        private Panel selectedPlayerPanel = null;
        private Button actionButton = null;

        public InGameForm(List<string> playerNames, TcpClient existingClient = null)
        {
            InitializeComponent();
            this.playerNames = playerNames;
            this.players = new List<string>(playerNames);
            uiContext = SynchronizationContext.Current;
            Load += Form1_Load;
            ConnectToServer(existingClient);
            tableLayoutPanel1.Click += TableLayoutPanel1_Click;
        }

        public InGameForm() : this(new List<string>()) { }

        private string placeholderText = "Nhập tin nhắn của bạn";
        private async void Form1_Load(object sender, EventArgs e)
        {   
            // Setup UI trước
            panelTopLeft.BackColor = Color.FromArgb(100, 0, 0, 0);
            panelRole.BackColor = Color.FromArgb(100, 0, 0, 0);
            labelTimer.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            labelTimer.ForeColor = Color.Orange;
            labelTimer.BackColor = Color.Transparent;
            labelTimer.Dock = DockStyle.Fill;
            labelTimer.TextAlign = ContentAlignment.MiddleCenter;
            labelTimer.BorderStyle = BorderStyle.None;
            labelTimer.BringToFront();
            labelTimer.Text = "Đang chờ dữ liệu...";
            SetPlaceholder();
            this.richTextBox2.Enter += new System.EventHandler(this.richTextBox2_Enter);
            this.richTextBox2.Leave += new System.EventHandler(this.richTextBox2_Leave);
            
            AddPanelsToTable();

            // Load data sau
            if (!isDataLoaded && !string.IsNullOrEmpty(currentUserId) && !string.IsNullOrEmpty(gameId))
            {
                isDataLoaded = true;
                try
                {
                    await LoadCurrentUserRole();
                    ListenPhaseFromFirebase();
                    await LoadCurrentPhaseFromFirebase();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading game data: {ex.Message}");
                    isDataLoaded = false;
                }
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {

        }

        private void pictureWerewolf_Click(object sender, EventArgs e)
        {

        }
        private void SetPlaceholder()
        {
            // Kiểm tra nếu richTextBox2 rỗng, hiển thị placeholder
            if (string.IsNullOrWhiteSpace(richTextBox2.Text))
            {
                richTextBox2.ForeColor = Color.Gray;  // Màu chữ placeholder
                richTextBox2.Text = placeholderText;
            }
        }

        private void richTextBox2_Enter(object sender, EventArgs e)
        {
            // Khi người dùng click vào richTextBox2, nếu có placeholder, xóa nó
            if (richTextBox2.Text == placeholderText)
            {
                richTextBox2.Text = "";
                richTextBox2.ForeColor = Color.White;  // Màu chữ khi nhập
            }
        }

        private void richTextBox2_Leave(object sender, EventArgs e)
        {
            // Khi người dùng rời khỏi richTextBox2 và nó rỗng, hiển thị lại placeholder
            if (string.IsNullOrWhiteSpace(richTextBox2.Text))
            {
                SetPlaceholder();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Thêm nội dung từ richTextBox2 vào richTextBox1 và làm mới richTextBox2
            if (richTextBox2.Text != placeholderText && !string.IsNullOrWhiteSpace(richTextBox2.Text))
            {
                string message = richTextBox2.Text;
                SendMessage($"CHAT_MESSAGE:{roomId}:{CurrentUserName}:{message}");
                richTextBox2.Clear();
                SetPlaceholder();  // Đặt lại placeholder sau khi gửi
            }
        }

        private void ConnectToServer(TcpClient existingClient = null)
        {
            try
            {
                if (existingClient != null)
                {
                    client = existingClient;
                    stream = client.GetStream();
                    isConnected = true;
                }
                else
                {
                    client = new TcpClient("localhost", 8888);
                    stream = client.GetStream();
                    isConnected = true;
                    SendMessage($"JOIN_ROOM:{roomId}:{CurrentUserName}");
                }

                // Start receive thread
                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                isConnected = false;
                if (client != null)
                {
                    try
                    {
                        client.Close();
                    }
                    catch { }
                }
                MessageBox.Show($"Không thể kết nối đến server: {ex.Message}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void SendMessage(string message)
        {
            if (!isConnected || client == null || stream == null) return;

            try
            {
                if (!client.Connected)
                {
                    isConnected = false;
                    return;
                }

                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                isConnected = false;
                if (!this.IsDisposed)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"Lỗi khi gửi tin nhắn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            StringBuilder messageBuffer = new StringBuilder();

            try
            {
                while (isConnected)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        string content = messageBuffer.ToString();
                        
                        // Xử lý tất cả tin nhắn hoàn chỉnh trong buffer
                        int lastNewLine = content.LastIndexOf('\n');
                        if (lastNewLine >= 0)
                        {
                            string[] messages = content.Substring(0, lastNewLine + 1).Split('\n');
                            foreach (string message in messages)
                            {
                                if (!string.IsNullOrEmpty(message.Trim()))
                                {
                                    string trimmedMessage = message.Trim();
                                    uiContext.Post(_ => ProcessServerMessage(trimmedMessage), null);
                                }
                            }
                            // Giữ lại phần chưa hoàn chỉnh
                            messageBuffer.Clear();
                            messageBuffer.Append(content.Substring(lastNewLine + 1));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (isConnected)
                {
                    uiContext.Post(_ =>
                    {
                        MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                    }, null);
                }
            }
        }

        private void ProcessServerMessage(string message)
        {
            try
            {
                string[] parts = message.Split(':');
                if (parts.Length < 2) return;

                string command = parts[0];
                switch (command)
                {
                    case "CHAT_MESSAGE":
                        if (parts.Length >= 3)
                        {
                            string sender = parts[1];
                            string msg = string.Join(":", parts.Skip(2));
                            AddChatMessage(sender, msg);
                        }
                        break;
                    case "PLAYER_JOINED":
                        if (parts.Length >= 2)
                        {
                            string newPlayer = parts[1];
                            OnPlayerJoined(newPlayer);
                        }
                        break;
                    case "PLAYER_LEFT":
                        if (parts.Length >= 2)
                        {
                            string leftPlayer = parts[1];
                            OnPlayerLeft(leftPlayer);
                        }
                        break;
                    case "PLAYER_LIST":
                        if (parts.Length >= 2)
                        {
                            string[] playerList = parts[1].Split(',');
                            players = playerList.ToList();
                            UpdatePlayerList();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error processing message: {ex.Message}\n");
            }
        }

        private void AddChatMessage(string sender, string message)
        {
            try
            {
                if (richTextBox1.InvokeRequired)
                {
                    richTextBox1.Invoke(new Action(() => AddChatMessage(sender, message)));
                    return;
                }

                string formattedMessage = $"[{DateTime.Now:HH:mm}] {sender}: {message}";
                richTextBox1.AppendText(formattedMessage + Environment.NewLine);
                richTextBox1.ScrollToCaret();
            }
            catch (Exception ex)
            {
                // Log error but don't show to user to avoid spam
                System.Diagnostics.Debug.WriteLine($"Error adding chat message: {ex.Message}");
            }
        }

        private void OnPlayerJoined(string playerName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnPlayerJoined(playerName)));
                return;
            }
            if (!players.Contains(playerName))
            {
                players.Add(playerName);
                UpdatePlayerList();
                richTextBox1.AppendText($"[{DateTime.Now:HH:mm}] Hệ thống: {playerName} đã tham gia phòng\n");
            }
        }

        private void OnPlayerLeft(string playerName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnPlayerLeft(playerName)));
                return;
            }
            if (players.Contains(playerName))
            {
                players.Remove(playerName);
                UpdatePlayerList();
                richTextBox1.AppendText($"[{DateTime.Now:HH:mm}] Hệ thống: {playerName} đã rời khỏi phòng\n");
            }
        }

        private void UpdatePlayerList()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdatePlayerList));
                return;
            }
            playerNames = new List<string>(players);
            AddPanelsToTable();
        }

        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void AddPanelsToTable()
        {
            try 
            {
                // Load role trước nếu chưa có
                if (string.IsNullOrEmpty(currentUserRole))
                {
                    await LoadCurrentUserRole();
                }

                tableLayoutPanel1.Controls.Clear();
                Image originalImage = Properties.Resources.UserIcon;
                int newWidth = 70;
                int newHeight = 70;
                Image resizedImage = new Bitmap(originalImage, new Size(newWidth, newHeight));
                int playerIndex = 0;

                // Thay đổi số hàng từ 4 xuống 2
                for (int row = 0; row < 2; row++)
                {
                    for (int col = 0; col < 4; col++)
                    {
                        Panel p = new Panel();
                        p.Dock = DockStyle.Fill;
                        p.BackColor = Color.FromArgb(30, 0, 0, 0);
                        p.Margin = new Padding(2);
                        p.BorderStyle = BorderStyle.FixedSingle;
                        p.Name = $"panel_{row}_{col}";

                        TableLayoutPanel innerTLP = new TableLayoutPanel();
                        innerTLP.Dock = DockStyle.Fill;
                        innerTLP.RowCount = 2;
                        innerTLP.ColumnCount = 1;
                        innerTLP.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                        innerTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                        p.Controls.Add(innerTLP);

                        if (playerIndex < playerNames.Count)
                        {   
                            string thisPlayerName = playerNames[playerIndex];
                            Label nameLabel = new Label();
                            nameLabel.Text = playerNames[playerIndex];
                            nameLabel.Dock = DockStyle.Fill;
                            nameLabel.Click += (s, e) => OnPlayerPanelClick(thisPlayerName, p);
                            nameLabel.TextAlign = ContentAlignment.MiddleCenter;
                            if (!string.IsNullOrEmpty(CurrentUserName) && playerNames[playerIndex] == CurrentUserName)
                                nameLabel.ForeColor = Color.Gold;
                            else
                                nameLabel.ForeColor = Color.White;
                            innerTLP.Controls.Add(nameLabel, 0, 0);

                            PictureBox pictureBox = new PictureBox();
                            if (!string.IsNullOrEmpty(CurrentUserName) && playerNames[playerIndex] == CurrentUserName && !string.IsNullOrEmpty(currentUserRole) && roleIcons.ContainsKey(currentUserRole))
                            {
                                pictureBox.Image = roleIcons[currentUserRole];
                            }
                            else
                            {
                                pictureBox.Image = Properties.Resources.UserIcon;
                            }
                            pictureBox.Click += (s, e) => OnPlayerPanelClick(thisPlayerName, p);
                            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                            pictureBox.Dock = DockStyle.Fill;
                            innerTLP.Controls.Add(pictureBox, 0, 1);
                        }
                        else
                        {
                            Label nameLabel = new Label();
                            nameLabel.Text = "";
                            nameLabel.Dock = DockStyle.Fill;
                            innerTLP.Controls.Add(nameLabel, 0, 0);
                        }
                        tableLayoutPanel1.Controls.Add(p, col, row);
                        playerIndex++;

                        if (playerIndex <= playerNames.Count && playerIndex > 0)
                        {
                            string thisPlayerName = playerNames[playerIndex - 1];
                            p.Click += (s, e) => OnPlayerPanelClick(thisPlayerName, p);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error in AddPanelsToTable: {ex.Message}\n");
            }
        }

        public void SetFirebaseInfo(string userId, string gameId, bool isHost = false, string roomId = null)
        {
            currentUserId = userId;
            this.gameId = gameId;
            currentGameId = gameId;
            this.isHost = isHost;
            this.roomId = roomId;
        }

        private async Task LoadCurrentUserRole()
        {
            var firebase = new FirebaseClient(
                "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(CurrentUserManager.CurrentUser?.IdToken)
                });
            var player = await firebase
                .Child("games")
                .Child(currentGameId)
                .Child("players")
                .Child(currentUserId)
                .OnceSingleAsync<Player>();
            currentUserRole = player?.Role?.ToLower();
        }

        private async Task LoadCurrentPhaseFromFirebase()
        {
            var firebase = new FirebaseClient(
                "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(CurrentUserManager.CurrentUser?.IdToken) });
            var game = await firebase
                .Child("games")
                .Child(gameId)
                .OnceSingleAsync<Game>();
            if (game != null && !string.IsNullOrEmpty(game.CurrentPhase) && !string.IsNullOrEmpty(game.PhaseStartTime))
            {
                UpdatePhaseFromFirebase(game.CurrentPhase, game.PhaseStartTime);
            }
        }

        private void ListenPhaseFromFirebase()
        {
            try
            {
                if (isListenerSetup)
                {
                    return;
                }

                if (phaseListener != null)
                {
                    phaseListener.Dispose();
                    phaseListener = null;
                }
                if (timeListener != null)
                {
                    timeListener.Dispose();
                    timeListener = null;
                }
                if (gameStatusListener != null)
                {
                    gameStatusListener.Dispose();
                    gameStatusListener = null;
                }


                var firebase = new FirebaseClient(
                    "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(CurrentUserManager.CurrentUser?.IdToken) });

                // Lắng nghe currentPhase
                phaseListener = firebase
                    .Child($"games/{gameId}/currentPhase")
                    .AsObservable<string>()
                    .Subscribe(
                        onNext: phaseEvt =>
                        {
                            richTextBox1.AppendText($"Phase event received: {phaseEvt.Object}\n");

                            if (!string.IsNullOrEmpty(phaseEvt.Object))
                            {
                                firebase
                                    .Child($"games/{gameId}/phaseStartTime")
                                    .OnceSingleAsync<string>()
                                    .ContinueWith(t =>
                                    {
                                        if (t.IsCompleted && !t.IsFaulted && t.Result != null)
                                        {
                                            this.Invoke((MethodInvoker)delegate
                                            {
                                                UpdatePhaseFromFirebase(phaseEvt.Object, t.Result);
                                            });
                                        }
                                    });
                            }
                        },
                        onError: ex =>
                        {
                            richTextBox1.AppendText($"Error in phase listener: {ex.Message}\n");
                        });

                // Lắng nghe trạng thái game
                gameStatusListener = firebase
                    .Child($"games/{gameId}/status")
                    .AsObservable<string>()
                    .Subscribe(
                        onNext: statusEvt =>
                        {
                            richTextBox1.AppendText($"Status event received: {statusEvt.Object}\n");
                            if (!string.IsNullOrEmpty(statusEvt.Object))
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    switch (statusEvt.Object)
                                    {
                                        case "villagers_win":
                                            MessageBox.Show("Phe Dân làng đã thắng!", "Kết thúc game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            break;
                                        case "werewolves_win":
                                            MessageBox.Show("Phe Sói đã thắng!", "Kết thúc game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            break;
                                    }
                                });
                            }
                        },
                        onError: ex =>
                        {
                            richTextBox1.AppendText($"Error in game status listener: {ex.Message}\n");
                        });

                isListenerSetup = true;

                // ✅ Tạo Poll Timer fallback nếu listener chết
                if (pollTimer == null)
                {
                    pollTimer = new System.Windows.Forms.Timer();
                    pollTimer.Interval = 2000; // mỗi 2 giây
                    pollTimer.Tick += async (s, e) =>
                    {
                        try
                        {
                            string latestPhase = await firebase.Child($"games/{gameId}/currentPhase").OnceSingleAsync<string>();
                            string latestStartTime = await firebase.Child($"games/{gameId}/phaseStartTime").OnceSingleAsync<string>();

                            if (!string.IsNullOrEmpty(latestPhase) && !string.IsNullOrEmpty(latestStartTime))
                            {
                                UpdatePhaseFromFirebase(latestPhase, latestStartTime);
                            }
                        }
                        catch (Exception ex)
                        {
                            richTextBox1.AppendText($"[PollTimer] Error: {ex.Message}\n");
                        }
                    };
                    pollTimer.Start();
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error setting up Firebase listener: {ex.Message}\n");
            }
        }

        private async void UpdatePhaseFromFirebase(string phase, string startTimeStr)
        {
            try
            {
                // Nếu không đổi thì bỏ qua
                if (phase == lastPhaseRaw && startTimeStr == lastStartTimeRaw)
                {
                    return;
                }

                lastPhaseRaw = phase;
                lastStartTimeRaw = startTimeStr;

                // Cập nhật enum phase
                switch (phase)
                {
                    case "night":
                        currentPhase = GamePhase.Night;
                        break;
                    case "day_discussion":
                        currentPhase = GamePhase.DayDiscussion;
                        break;
                    case "day_vote":
                        currentPhase = GamePhase.DayVote;
                        break;
                    default:
                        currentPhase = GamePhase.Night;
                        break;
                }

                // Parse thời gian
                if (!string.IsNullOrEmpty(startTimeStr))
                {
                    try
                    {
                        phaseStartTime = DateTime.Parse(startTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        if (phaseStartTime.Kind != DateTimeKind.Utc)
                            phaseStartTime = DateTime.SpecifyKind(phaseStartTime, DateTimeKind.Utc);
                    }
                    catch (Exception ex)
                    {
                        richTextBox1.AppendText($"Invalid startTime format, using UtcNow. Parse error: {ex.Message}\n");
                        phaseStartTime = DateTime.UtcNow;
                    }
                }
                else
                {
                    phaseStartTime = DateTime.UtcNow;
                }

                // Khởi tạo hoặc reset timer
                if (phaseTimer == null)
                {
                    phaseTimer = new System.Windows.Forms.Timer();
                    phaseTimer.Interval = 1000;
                    phaseTimer.Tick += PhaseTimer_Tick;
                }
                else
                {
                    phaseTimer.Stop();
                }

                UpdateTimerLabel();
                phaseTimer.Start();

                // Kiểm tra status game cho tất cả người chơi
                var firebaseHelper = new FirebaseHelper();
                var game = await firebaseHelper.GetGame(gameId);
                if (!string.IsNullOrEmpty(game.Status))
                {
                    switch (game.Status)
                    {
                        case "villagers_win":
                            MessageBox.Show("Phe Dân làng đã thắng!", "Kết thúc game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case "werewolves_win":
                            MessageBox.Show("Phe Sói đã thắng!", "Kết thúc game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                    }
                }

                // Log
                richTextBox1.AppendText($"Phase updated to {phase}, startTime={phaseStartTime.ToString("o")}, timer started\n");
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error in UpdatePhaseFromFirebase: {ex.Message}\n");
            }
        }

        private async void PhaseTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                int secondsPassed = (int)(DateTime.UtcNow - phaseStartTime).TotalSeconds;
                int timeLeft = Math.Max(0, phaseDuration - secondsPassed);
                UpdateTimerLabel(timeLeft);
                
                if (timeLeft <= 0)
                {
                    phaseTimer.Stop();
                    if (isHost)
                    {
                        await NextPhase_Host();
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error in PhaseTimer_Tick: {ex.Message}{Environment.NewLine}");
            }
        }

        private void UpdateTimerLabel(int? customTimeLeft = null)
        {
            if (labelTimer.InvokeRequired)
            {
                labelTimer.Invoke(new Action(() => UpdateTimerLabel(customTimeLeft)));
                return;
            }
            string phaseText = "";
            switch (currentPhase)
            {
                case GamePhase.Night:
                    phaseText = "Đêm: ";
                    break;
                case GamePhase.DayDiscussion:
                    phaseText = "Thảo luận: ";
                    break;
                case GamePhase.DayVote:
                    phaseText = "Bầu chọn: ";
                    break;
                default:
                    phaseText = "";
                    break;
            }
            int timeLeft = customTimeLeft ?? phaseDuration;
            labelTimer.Text = phaseText + timeLeft.ToString() + "s";
            labelTimer.TextAlign = ContentAlignment.MiddleCenter;
            if (timeLeft <= 10)
            {
                labelTimer.ForeColor = Color.Red;
            }
            else
            {
                labelTimer.ForeColor = Color.Orange;
            }
        }

        private async Task NextPhase_Host()
        {
            try
            {
                var firebaseHelper = new FirebaseHelper();
                string currentPhaseStr = currentPhase.ToString().ToLower();
                await firebaseHelper.NextPhase(gameId, currentPhaseStr);
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error in NextPhase_Host: {ex.Message}\n");
            }
        }

        private async Task CheckWerewolfWin()
        {
            try
            {
                var firebaseHelper = new FirebaseHelper();
                await firebaseHelper.CheckGameEndCondition(gameId);
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error checking game end: {ex.Message}\n");
            }
        }

        private void OnPlayerPanelClick(string targetPlayerName, Panel panel)
        {
           
            // Xóa nút cũ nếu có
            if (actionButton != null)
            {
                tableLayoutPanel1.Controls.Remove(actionButton);
                actionButton.Dispose();
                actionButton = null;
            }

            // Kiểm tra phase và role để quyết định có hiện nút không
            bool showButton = false;
            string buttonText = "";

            if (currentPhase == GamePhase.Night)
            {
                if (currentUserRole == "werewolf" && targetPlayerName != CurrentUserName)
                {
                    showButton = true;
                    buttonText = "Giết";
                }
                else if (currentUserRole == "bodyguard")
                {
                    showButton = true;
                    buttonText = "Bảo vệ";
                }
                else if (currentUserRole == "seer" && targetPlayerName != CurrentUserName)
                {
                    showButton = true;
                    buttonText = "Soi";
                }
                else if (currentUserRole == "witch")
                {
                    showButton = true;
                    buttonText = (targetPlayerName == CurrentUserName) ? "Cứu" : "Giết";
                }
            }


            // TEST: luôn tạo nút để kiểm tra vị trí
            if (true)
            {
                actionButton = new Button();
                actionButton.Text = string.IsNullOrEmpty(buttonText) ? "Test" : buttonText;
                actionButton.Size = new Size(80, 35);
                actionButton.Location = new Point(100, 100); // Đặt rõ ràng trong tableLayoutPanel1
                actionButton.BringToFront();
                tableLayoutPanel1.Controls.Add(actionButton);

                actionButton.Click += (s, e) =>
                {
                    MessageBox.Show($"Bạn đã chọn {buttonText} {targetPlayerName}");
                };
            }
        }

        private void TableLayoutPanel1_Click(object sender, EventArgs e)
        {
            if (actionButton != null)
            {
                tableLayoutPanel1.Controls.Remove(actionButton);
                actionButton.Dispose();
                actionButton = null;
            }
        }

        private void pictureSheriff_Click(object sender, EventArgs e)
        {

        }
    }
}

