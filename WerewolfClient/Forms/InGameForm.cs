using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using Firebase.Database;
using WerewolfClient.Models;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Linq;
using Firebase.Database.Query;
using Firebase.Database.Streaming;

namespace WerewolfClient.Forms
{
    public partial class InGameForm : Form
    {
        private List<string> playerNamesFromConstructor = new List<string>();
        private List<Player> gamePlayers = new List<Player>();
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
            { "villager", Properties.Resources.villagerIcon },
            { "dead", Properties.Resources.DeadIcon }
        };

        private enum GamePhaseClient { Night, DayDiscussion, DayVote, Unknown }
        private GamePhaseClient currentPhaseClient = GamePhaseClient.Unknown;
        private System.Windows.Forms.Timer phaseTimerUi;
        private DateTime phaseStartTimeUtc;
        private IDisposable firebasePhaseListener;
        private bool isHost = false;
        private string gameId;
        private int phaseDurationSeconds = 30;  // Thời gian chuyển phase
        private bool isFirebaseListenerSetup = false;
        private bool isGameDataLoaded = false;
        private string lastFirebasePhaseRaw = null;
        private string lastFirebaseStartTimeRaw = null;
        private IDisposable firebaseGameStatusListener;
        private bool isServerConnected = false;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private SynchronizationContext uiContext;
        private string chatRoomId;
        private StringBuilder receiveBuffer = new StringBuilder();
        private Panel selectedPlayerPanel = null;
        private Button actionButton = null;
        private string currentActionTargetPlayerName;
        private string currentActionButtonText;
        private System.Threading.CancellationTokenSource _cancellationTokenSource;
        private Task _receivingTask;
        private IDisposable firebasePlayersListener;
        private System.Windows.Forms.Timer _updatePlayersDisplayDebounceTimer;
        private const int UpdatePlayersDisplayDebounceTimeMs = 300;
        private IDisposable firebaseGameLogListener;
        private int currentDay = 1;
        private bool isGameReallyOver = false; 

        public InGameForm(List<string> playerNamesParam, TcpClient existingClient = null)
        {
            InitializeComponent();
            this.playerNamesFromConstructor = playerNamesParam;
            uiContext = SynchronizationContext.Current;
            Load += new System.EventHandler(this.Form1_Load);
            tableLayoutPanel1.Click += new System.EventHandler(this.TableLayoutPanel1_Click);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InGameForm_FormClosing);
            ConnectToServer(existingClient);
        }

        public InGameForm() : this(new List<string>()) { }

        private string placeholderText = "Nhập tin nhắn của bạn";
        private async void Form1_Load(object sender, EventArgs e)
        {
            this.actionButton = new System.Windows.Forms.Button();
            this.actionButton.Size = new System.Drawing.Size(130, 40);
            this.actionButton.Visible = false;
            this.actionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.actionButton.FlatAppearance.BorderSize = 1;
            this.actionButton.FlatAppearance.BorderColor = System.Drawing.Color.Aqua;
            this.actionButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.actionButton.ForeColor = System.Drawing.Color.White;
            this.actionButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.actionButton.Name = "playerActionButton";
            this.actionButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Controls.Add(this.actionButton);
            this.actionButton.BringToFront();

            panelLoadingOverlay.Location = tableLayoutPanel1.Location;
            panelLoadingOverlay.Size = tableLayoutPanel1.Size;
            panelLoadingOverlay.BringToFront();
            panelLoadingOverlay.Visible = true;

            panelTopLeft.BackColor = Color.FromArgb(100, 0, 0, 0);
            panelRole.BackColor = Color.FromArgb(100, 0, 0, 0);
            labelTimer.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            labelTimer.ForeColor = Color.Orange;
            labelTimer.BackColor = Color.Transparent;
            labelTimer.Dock = DockStyle.Fill;
            labelTimer.TextAlign = ContentAlignment.MiddleCenter;
            labelTimer.BorderStyle = BorderStyle.None;
            labelTimer.BringToFront();
            labelTimer.Text = "Đang chờ...";
            SetPlaceholder();
            this.richTextBox2.Enter += new System.EventHandler(this.richTextBox2_Enter);
            this.richTextBox2.Leave += new System.EventHandler(this.richTextBox2_Leave);
            if (this.button1 != null)
            {
                this.button1.Click += new System.EventHandler(this.button1_Click);
            }
            if (this.richTextBox2 != null)
            {
                this.richTextBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox2_KeyDown);
            }
            if (this.btnRole != null)
            {
                this.btnRole.Click += new System.EventHandler(this.btnRole_Click);
            }
            if (this.btnQuit != null)
            {
                this.btnQuit.Click += new System.EventHandler(this.BtnQuit_Click);
            }

            _updatePlayersDisplayDebounceTimer = new System.Windows.Forms.Timer();
            _updatePlayersDisplayDebounceTimer.Interval = UpdatePlayersDisplayDebounceTimeMs;
            _updatePlayersDisplayDebounceTimer.Tick += async (s, ev) =>
            {
                _updatePlayersDisplayDebounceTimer.Stop();
                if (this.IsHandleCreated && !this.IsDisposed && isGameDataLoaded)
                {
                    await UpdatePlayerDisplayAsync();
                }
            };

            if (!isGameDataLoaded && !string.IsNullOrEmpty(currentUserId) && !string.IsNullOrEmpty(gameId))
            {
                isGameDataLoaded = true;
                try
                {
                    if (IsHandleCreated) labelLoading.Text = "Đang tải vai trò...";
                    await LoadCurrentUserRole();

                    if (IsHandleCreated) labelLoading.Text = "Đang chuẩn bị giao diện người chơi...";
                    await UpdatePlayerDisplayAsync();

                    if (IsHandleCreated) labelLoading.Text = "Đang đồng bộ trạng thái trò chơi...";
                    SetupFirebaseListeners();
                    await LoadCurrentPhaseFromFirebase(); 

                    // Chỉ ẩn panel loading nếu game CHƯA kết thúc và form còn tồn tại
                    if (!isGameReallyOver && IsHandleCreated && panelLoadingOverlay != null)
                    {
                        panelLoadingOverlay.Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    isGameDataLoaded = false;
                    if (IsHandleCreated)
                    {
                        labelLoading.Text = $"Lỗi tải dữ liệu: {ex.Message}";
                        MessageBox.Show($"Error loading game data: {ex.Message}", "Lỗi Tải Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (panelLoadingOverlay != null) panelLoadingOverlay.Visible = false; // Vẫn ẩn nếu lỗi
                    }
                }
            }
            else if (isGameDataLoaded) 
            {
                await UpdatePlayerDisplayAsync();
                // Chỉ ẩn panel loading nếu game CHƯA kết thúc và form còn tồn tại
                if (!isGameReallyOver && IsHandleCreated && panelLoadingOverlay != null)
                {
                    panelLoadingOverlay.Visible = false;
                }
            }
            else
            {
                if (IsHandleCreated)
                {
                    labelLoading.Text = "Lỗi: Thông tin game/người dùng không hợp lệ.";
                    MessageBox.Show("Thông tin game hoặc người dùng không hợp lệ để tải dữ liệu.", "Lỗi Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (panelLoadingOverlay != null) panelLoadingOverlay.Visible = false; // Vẫn ẩn nếu lỗi
                }
            }
        }
        private void SetPlaceholder()
        {
            if (string.IsNullOrWhiteSpace(richTextBox2.Text))
            {
                richTextBox2.ForeColor = Color.Gray;
                richTextBox2.Text = placeholderText;
            }
        }

        private void richTextBox2_Enter(object sender, EventArgs e)
        {
            if (richTextBox2.Text == placeholderText)
            {
                richTextBox2.Text = "";
                richTextBox2.ForeColor = Color.FromArgb(200, 200, 200);
            }
        }

        private void richTextBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBox2.Text))
            {
                SetPlaceholder();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (richTextBox2.Text != placeholderText && !string.IsNullOrWhiteSpace(richTextBox2.Text))
                {
                    string message = richTextBox2.Text.Trim();
                    if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(chatRoomId) && CurrentUserName != null)
                    {
                        SendMessage($"CHAT_MESSAGE:{chatRoomId}:{CurrentUserName}:{message}");
                        richTextBox2.Clear();
                        SetPlaceholder();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi tin nhắn: {ex.Message}", "Lỗi Gửi Tin Nhắn", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectToServer(TcpClient existingClient = null)
        {
            try
            {
                _cancellationTokenSource = new System.Threading.CancellationTokenSource();

                if (existingClient != null && existingClient.Connected)
                {
                    tcpClient = existingClient;
                    networkStream = tcpClient.GetStream();
                    isServerConnected = true;
                }
                else
                {
                    tcpClient = new TcpClient("localhost", 8888);
                    networkStream = tcpClient.GetStream();
                    isServerConnected = true;
                    if (!string.IsNullOrEmpty(chatRoomId) && !string.IsNullOrEmpty(CurrentUserName))
                    {
                        SendMessage($"JOIN_ROOM:{chatRoomId}:{CurrentUserName}");
                    }
                }
                _receivingTask = Task.Run(() => StartReceivingAsync(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                isServerConnected = false;
                tcpClient?.Close();
                MessageBox.Show($"Không thể kết nối đến server chat: {ex.Message}", "Lỗi Kết Nối Chat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SendMessage(string message)
        {
            if (!isServerConnected || tcpClient == null || !tcpClient.Connected || networkStream == null)
            {
                isServerConnected = false; return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                networkStream.Write(data, 0, data.Length);
                networkStream.Flush();
            }
            catch (Exception ex)
            {
                isServerConnected = false;
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    Invoke((MethodInvoker)delegate {
                        MessageBox.Show($"Lỗi khi gửi tin nhắn: {ex.Message}", "Lỗi Gửi Tin Nhắn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            }
        }

        private async Task StartReceivingAsync(System.Threading.CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096];
            StringBuilder messageProcessingBuffer = new StringBuilder();


            try
            {
                while (!cancellationToken.IsCancellationRequested && tcpClient.Connected)
                {
                    int bytesRead = 0;
                    if (networkStream.CanRead)
                    {
                        var readTask = networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        bytesRead = await readTask;
                    }

                    if (bytesRead > 0)
                    {
                        messageProcessingBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        string content = messageProcessingBuffer.ToString();
                        int newlineIndex;
                        while ((newlineIndex = content.IndexOf('\n')) >= 0)
                        {
                            string completeMessage = content.Substring(0, newlineIndex).Trim();
                            content = content.Substring(newlineIndex + 1);
                            if (!string.IsNullOrEmpty(completeMessage))
                            {
                                uiContext.Post(new SendOrPostCallback(ProcessServerMessage), completeMessage);
                            }
                        }
                        messageProcessingBuffer.Clear().Append(content);
                    }
                    else
                    {
                        isServerConnected = false;
                        break;
                    }
                }
            }
            catch (OperationCanceledException) { isServerConnected = false; }
            catch (System.IO.IOException) { isServerConnected = false; }
            catch (Exception ex)
            {
                isServerConnected = false;
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    uiContext.Post(_ => {
                        MessageBox.Show($"Lỗi trong luồng nhận dữ liệu: {ex.Message}", "Lỗi Nhận Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }, null);
                }
            }
        }

        private void ProcessServerMessage(object messageObj)
        {
            string message = messageObj as string;
            if (message == null) return;
            try
            {
                string[] parts = message.Split(':');
                if (parts.Length < 2 && !(parts.Length >= 1 && (parts[0] == "PLAYER_JOINED" || parts[0] == "PLAYER_LEFT")))
                {
                    if (parts.Length >= 1 && (parts[0] == "PLAYER_LIST")) { } else return;
                }


                string command = parts[0];
                switch (command)
                {
                    case "CHAT_MESSAGE":
                        if (parts.Length >= 4)
                        {
                            string msgRoomId = parts[1];
                            string sender = parts[2];
                            string msg = string.Join(":", parts.Skip(3));
                            if (msgRoomId == this.chatRoomId)
                            {
                                AddChatMessage(sender, msg);
                            }
                        }
                        break;
                    case "PLAYER_JOINED":
                        if (parts.Length >= 2)
                        {
                            string newPlayer = parts[1];
                            OnPlayerJoined(newPlayer);
                            DisplayGameLog(new GameLog { Timestamp = DateTime.Now, Message = $"{newPlayer} đã tham gia phòng chat.", Phase = "chat_event" });
                        }
                        break;
                    case "PLAYER_LEFT":
                        if (parts.Length >= 2)
                        {
                            string leftPlayer = parts[1];
                            OnPlayerLeft(leftPlayer);
                            DisplayGameLog(new GameLog { Timestamp = DateTime.Now, Message = $"{leftPlayer} đã rời phòng chat.", Phase = "chat_event" });
                        }
                        break;
                    case "PLAYER_LIST":
                        
                        break;
                }
            }
            catch (Exception ex)
            {
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    MessageBox.Show($"Lỗi xử lý tin nhắn từ server: {ex.Message}", "Lỗi Xử Lý Tin Nhắn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddChatMessage(string sender, string message)
        {
            if (isGameReallyOver) return;

            if (this.IsDisposed || !this.IsHandleCreated || richTextBox1 == null || richTextBox1.IsDisposed) return;
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() => AddChatMessage(sender, message)));
                return;
            }
            try
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectionLength = 0;
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.SelectionColor = Color.WhiteSmoke;
                richTextBox1.AppendText($"[{timestamp}] {sender}: ");

                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectionLength = 0;
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.SelectionColor = Color.Gainsboro;
                richTextBox1.AppendText($"{message}\n");

                richTextBox1.SelectionFont = richTextBox1.Font;
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
                richTextBox1.ScrollToCaret();
            }
            catch (Exception ex) { Console.WriteLine("Error adding chat: " + ex.Message); }
        }

        private void OnPlayerJoined(string playerName)
        {
            
        }

        private void OnPlayerLeft(string playerName)
        {
            
        }

        private void DisplayGameLog(GameLog logEntry)
        {
            if (isGameReallyOver) 
            {
                Console.WriteLine($"DIAGNOSTIC InGameForm: DisplayGameLog - Game is over. Suppressing log: '{logEntry.Message}'");
                return;
            }

            if (this.IsDisposed || !this.IsHandleCreated || richTextBox1 == null || richTextBox1.IsDisposed) return;

            List<string> phasesToIgnoreInChat = new List<string>
            {
                "night_action_detail",
                "seer_action_detail",
                "witch_action_detail",
                "bodyguard_action_detail",
                "game_end" 
            };

            if (logEntry.Phase != null && phasesToIgnoreInChat.Contains(logEntry.Phase.ToLower()))
            {
                Console.WriteLine($"DIAGNOSTIC InGameForm: DisplayGameLog - Filtered out (ignored) log: '{logEntry.Message}' (Phase: {logEntry.Phase})");
                return;
            }

            string formattedMessage = $"[{logEntry.Timestamp:HH:mm}] {logEntry.Message}\n";
            Font logFont = new Font(richTextBox1.Font, FontStyle.Italic);
            Color logColor = Color.LightSteelBlue;

            if (logEntry.Phase == "night_summary" || logEntry.Phase == "day_vote_result" || logEntry.Phase == "hunter_shot")
            {
                logColor = Color.Gold;
                logFont = new Font(richTextBox1.Font, FontStyle.Bold | FontStyle.Italic);
            }
            else if (logEntry.Phase == "SystemError")
            {
                logColor = Color.Tomato;
            }
            else if (logEntry.Phase == "system_phase_change")
            {
                logColor = Color.MediumPurple;
                logFont = new Font(richTextBox1.Font, FontStyle.Bold);
            }
            else if (logEntry.Phase == "system")
            {
                logColor = Color.SpringGreen;
            }
            else if (logEntry.Phase == "chat_event")
            {
                logColor = Color.DarkGray;
            }
            else if (logEntry.Phase == "night_result")
            {
                logColor = Color.SkyBlue;
            }

            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() => {
                    richTextBox1.SelectionStart = richTextBox1.TextLength;
                    richTextBox1.SelectionLength = 0;
                    richTextBox1.SelectionFont = logFont;
                    richTextBox1.SelectionColor = logColor;
                    richTextBox1.AppendText(formattedMessage);
                    richTextBox1.SelectionFont = richTextBox1.Font;
                    richTextBox1.SelectionColor = richTextBox1.ForeColor;
                    richTextBox1.ScrollToCaret();
                }));
            }
            else
            {
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectionLength = 0;
                richTextBox1.SelectionFont = logFont;
                richTextBox1.SelectionColor = logColor;
                richTextBox1.AppendText(formattedMessage);
                richTextBox1.SelectionFont = richTextBox1.Font;
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
                richTextBox1.ScrollToCaret();
            }
        }


        private async Task UpdatePlayerDisplayAsync()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (tableLayoutPanel1.InvokeRequired)
            {
                await (Task)tableLayoutPanel1.Invoke(new Func<Task>(UpdatePlayerDisplayAsync));
                return;
            }

            try
            {
                tableLayoutPanel1.SuspendLayout();
                if (string.IsNullOrEmpty(currentUserRole) && !string.IsNullOrEmpty(currentUserId) && !string.IsNullOrEmpty(gameId))
                {
                    if (panelLoadingOverlay.Visible && !isGameReallyOver && IsHandleCreated) labelLoading.Text = "Đang tải lại vai trò...";
                    await LoadCurrentUserRole();
                }

                var firebase = new FirebaseHelper();
                List<Player> latestGamePlayers = await firebase.GetPlayers(gameId);
                if (latestGamePlayers == null) latestGamePlayers = new List<Player>();

                gamePlayers = latestGamePlayers;


                tableLayoutPanel1.Controls.Clear();
                int playerDisplayIndex = 0;
                var orderedPlayers = gamePlayers.OrderBy(p => p.Name == CurrentUserName ? 0 : 1)
                                               .ThenBy(p => p.Name)
                                               .ToList();

                for (int row = 0; row < tableLayoutPanel1.RowCount; row++)
                {
                    for (int col = 0; col < tableLayoutPanel1.ColumnCount; col++)
                    {
                        if (playerDisplayIndex >= orderedPlayers.Count)
                        {
                            Panel emptyP = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 0, 0, 0), Margin = new Padding(2), BorderStyle = BorderStyle.None };
                            tableLayoutPanel1.Controls.Add(emptyP, col, row);
                            continue;
                        }

                        Player currentPlayerDoc = orderedPlayers[playerDisplayIndex];
                        string playerName = currentPlayerDoc.Name;

                        Panel p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(3), BorderStyle = BorderStyle.FixedSingle };
                        p.Name = $"playerPanel_{playerName.Replace(" ", "_")}";

                        TableLayoutPanel innerTLP = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
                        innerTLP.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                        innerTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                        p.Controls.Add(innerTLP);

                        Label nameLabel = new Label { Text = playerName, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                        PictureBox pictureBox = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Fill, Margin = new Padding(5) };

                        if (!currentPlayerDoc.IsAlive)
                        {
                            pictureBox.Image = roleIcons.ContainsKey("dead") ? roleIcons["dead"] : Properties.Resources.UserIcon;
                            nameLabel.ForeColor = Color.DarkGray;
                            p.BackColor = Color.FromArgb(150, 50, 50, 50);
                        }
                        else
                        {
                            p.BackColor = Color.FromArgb(150, 30, 30, 30);
                            if (playerName == CurrentUserName) p.BackColor = Color.FromArgb(150, 0, 50, 50);

                            nameLabel.ForeColor = (playerName == CurrentUserName) ? Color.Gold : Color.WhiteSmoke;

                            if (playerName == CurrentUserName && !string.IsNullOrEmpty(currentUserRole) && roleIcons.ContainsKey(currentUserRole))
                            {
                                pictureBox.Image = roleIcons[currentUserRole];
                            }
                            else
                            {
                                pictureBox.Image = Properties.Resources.UserIcon;
                            }
                        }

                        var panelClickHandler = new System.EventHandler((s, eArgs) => OnPlayerPanelClick(currentPlayerDoc, p));
                        p.Click += panelClickHandler;
                        nameLabel.Click += panelClickHandler;
                        pictureBox.Click += panelClickHandler;
                        innerTLP.Click += panelClickHandler;

                        innerTLP.Controls.Add(nameLabel, 0, 0);
                        innerTLP.Controls.Add(pictureBox, 0, 1);
                        tableLayoutPanel1.Controls.Add(p, col, row);
                        playerDisplayIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsHandleCreated)
                {
                    MessageBox.Show($"Lỗi cập nhật giao diện người chơi: {ex.Message}", "Lỗi Giao Diện", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Console.WriteLine($"Error in UpdatePlayerDisplayAsync: {ex.Message} \n {ex.StackTrace}");
            }
            finally
            {
                if (IsHandleCreated && tableLayoutPanel1 != null)
                {
                    tableLayoutPanel1.ResumeLayout(true);
                }
            }
        }

        public void SetFirebaseInfo(string userId, string gameIdParam, bool isHostParam = false, string roomIdParam = null)
        {
            this.currentUserId = userId;
            this.gameId = gameIdParam;
            this.currentGameId = gameIdParam;
            this.isHost = isHostParam;
            this.chatRoomId = roomIdParam;

            Console.WriteLine($"DIAGNOSTIC InGameForm: SetFirebaseInfo called. UserID: {userId}, GameID: {gameIdParam}, IsHost: {isHostParam}, RoomID: {roomIdParam}");

            if (isServerConnected && tcpClient != null && tcpClient.Connected && !string.IsNullOrEmpty(chatRoomId) && !string.IsNullOrEmpty(CurrentUserName))
            {
                // SendMessage($"JOIN_ROOM:{chatRoomId}:{CurrentUserName}"); // Đã gửi ở ConnectToServer
            }
        }

        private async Task LoadCurrentUserRole()
        {
            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(currentGameId)) return;
            try
            {
                var firebase = new FirebaseClient(
                    "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(CurrentUserManager.CurrentUser?.IdToken) });

                var player = await firebase
                    .Child("games")
                    .Child(currentGameId)
                    .Child("players")
                    .Child(currentUserId)
                    .OnceSingleAsync<Player>();

                if (player != null)
                {
                    currentUserRole = player.Role?.ToLower();
                    CurrentUserName = player.Name;
                }
                else
                {
                    currentUserRole = "spectator"; CurrentUserName = "Khán giả";
                }
                Console.WriteLine($"DIAGNOSTIC InGameForm: LoadCurrentUserRole - User: {CurrentUserName}, Role: {currentUserRole}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading current user role: {ex.Message}");
                if (IsHandleCreated)
                {
                    MessageBox.Show($"Lỗi tải vai trò: {ex.Message}", "Lỗi Vai Trò", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task LoadCurrentPhaseFromFirebase()
        {
            if (string.IsNullOrEmpty(gameId)) return;
            try
            {
                var firebase = new FirebaseClient(
                    "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(CurrentUserManager.CurrentUser?.IdToken) });
                var game = await firebase.Child("games").Child(gameId).OnceSingleAsync<Game>();

                if (game != null && !string.IsNullOrEmpty(game.CurrentPhase) && !string.IsNullOrEmpty(game.PhaseStartTime))
                {
                    if (game.PhaseDuration.HasValue) phaseDurationSeconds = game.PhaseDuration.Value;
                    Console.WriteLine($"DIAGNOSTIC InGameForm: LoadCurrentPhaseFromFirebase - Loaded Phase: {game.CurrentPhase}, StartTime: {game.PhaseStartTime}, Duration: {phaseDurationSeconds}s");
                    UpdatePhaseAndTimerFromFirebase(game.CurrentPhase, game.PhaseStartTime);
                }
                else
                {
                    Console.WriteLine($"DIAGNOSTIC InGameForm: LoadCurrentPhaseFromFirebase - Game data or phase info is null/empty. Game: {game?.Id}, Phase: {game?.CurrentPhase}, StartTime: {game?.PhaseStartTime}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading current phase: {ex.Message}");
                if (IsHandleCreated) MessageBox.Show($"Lỗi tải phase hiện tại: {ex.Message}", "Lỗi Phase", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupFirebaseListeners()
        {
            if (isFirebaseListenerSetup || string.IsNullOrEmpty(gameId)) return;
            try
            {
                var firebase = new FirebaseClient(
                    "https://werewolf-d83dd-default-rtdb.asia-southeast1.firebasedatabase.app/",
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(CurrentUserManager.CurrentUser?.IdToken) });

                if (firebasePhaseListener == null)
                {
                    firebasePhaseListener = firebase.Child("games").Child(gameId)
                        .AsObservable<Game>()
                        .Subscribe(
                            gameEvent => {
                                if (gameEvent.Object != null)
                                {
                                    if (gameEvent.Object.PhaseDuration.HasValue) phaseDurationSeconds = gameEvent.Object.PhaseDuration.Value;
                                    Console.WriteLine($"DIAGNOSTIC InGameForm: firebasePhaseListener received update. Phase: {gameEvent.Object.CurrentPhase}, StartTime: {gameEvent.Object.PhaseStartTime}, Duration: {phaseDurationSeconds}s");
                                    uiContext.Post(new SendOrPostCallback(o => UpdatePhaseAndTimerFromFirebase(((Game)o).CurrentPhase, ((Game)o).PhaseStartTime)), gameEvent.Object);
                                }
                                else { Console.WriteLine("DIAGNOSTIC InGameForm: firebasePhaseListener received null gameEvent.Object"); }
                            },
                            ex => HandleFirebaseError("Phase Listener", ex)
                        );
                    Console.WriteLine($"DIAGNOSTIC InGameForm: firebasePhaseListener for gameId '{gameId}' setup.");
                }

                if (firebaseGameStatusListener == null)
                {
                    firebaseGameStatusListener = firebase.Child("games").Child(gameId).Child("status")
                        .AsObservable<string>()
                        .Subscribe(
                            statusEvent => {
                                Console.WriteLine($"DIAGNOSTIC InGameForm: firebaseGameStatusListener received update. Status: {statusEvent.Object}");
                                uiContext.Post(new SendOrPostCallback(o => ProcessGameStatusUpdate((string)o)), statusEvent.Object);
                            },
                            ex => HandleFirebaseError("Game Status Listener", ex)
                        );
                    Console.WriteLine($"DIAGNOSTIC InGameForm: firebaseGameStatusListener for gameId '{gameId}' setup.");
                }

                if (firebasePlayersListener == null)
                {
                    firebasePlayersListener = firebase.Child("games").Child(gameId).Child("players")
                        .AsObservable<Player>() 
                        .Subscribe(
                            playerEventOrSnapshot =>
                            {
                                if (this.IsHandleCreated && !this.IsDisposed)
                                {
                                    Console.WriteLine($"DIAGNOSTIC InGameForm: firebasePlayersListener received update for player key: {playerEventOrSnapshot.Key}. Triggering debounce timer.");
                                    _updatePlayersDisplayDebounceTimer.Stop();
                                    _updatePlayersDisplayDebounceTimer.Start();
                                }
                            },
                            ex => HandleFirebaseError("Players Listener", ex)
                        );
                    Console.WriteLine($"DIAGNOSTIC InGameForm: firebasePlayersListener for gameId '{gameId}' setup.");
                }

                if (firebaseGameLogListener == null)
                {
                    firebaseGameLogListener = firebase
                        .Child("gameLogs")
                        .Child(gameId)
                        .AsObservable<GameLog>() 
                        .Subscribe(
                            logEvent =>
                            {
                                if (logEvent.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate && logEvent.Object != null) // Check for new logs
                                {
                                    GameLog newLog = logEvent.Object;
                                    Console.WriteLine($"DIAGNOSTIC InGameForm: firebaseGameLogListener received new log (Key: {logEvent.Key}): '{newLog.Message}' for phase '{newLog.Phase}'");
                                    uiContext.Post(new SendOrPostCallback((state) =>
                                    {
                                        DisplayGameLog(newLog);
                                    }), null);
                                }
                            },
                            ex => HandleFirebaseError("GameLog Listener", ex)
                        );
                    Console.WriteLine($"DIAGNOSTIC InGameForm: firebaseGameLogListener for gameId '{gameId}' setup.");
                }


                isFirebaseListenerSetup = true;
                Console.WriteLine("DIAGNOSTIC InGameForm: Firebase Listeners Setup COMPLETE.");
            }
            catch (Exception ex) { HandleFirebaseError("SetupListeners", ex); }
        }


        private void HandleFirebaseError(string listenerName, Exception ex)
        {
            Console.WriteLine($"DIAGNOSTIC InGameForm: Firebase Error ({listenerName}): {ex.ToString()}");
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                MessageBox.Show($"Lỗi Firebase Listener ({listenerName}): {ex.Message}", "Lỗi Firebase", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessGameStatusUpdate(object statusObj)
        {
            string status = statusObj as string;
            if (status == null || this.IsDisposed || !this.IsHandleCreated) return;
            Console.WriteLine($"DIAGNOSTIC InGameForm: ProcessGameStatusUpdate. Status: {status}");
            string message = ""; bool endGameSignal = false;
            switch (status.ToLower())
            {
                case "villagers_win": message = "Phe Dân làng đã thắng!"; endGameSignal = true; break;
                case "werewolves_win": message = "Phe Sói đã thắng!"; endGameSignal = true; break;
                case "ended": message = "Trò chơi đã kết thúc."; endGameSignal = true; break;
            }

            if (endGameSignal && !isGameReallyOver)
            {
                isGameReallyOver = true;
                if (IsHandleCreated)
                    MessageBox.Show(message, "Kết Thúc Game", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (phaseTimerUi != null) phaseTimerUi.Stop();
                currentPhaseClient = GamePhaseClient.Unknown;

                if (IsHandleCreated && labelTimer != null && !labelTimer.IsDisposed)
                {
                    if (labelTimer.InvokeRequired)
                    {
                        labelTimer.Invoke(new Action(() => labelTimer.Text = "GAME KẾT THÚC"));
                    }
                    else
                    {
                        labelTimer.Text = "GAME KẾT THÚC";
                    }
                }

                if (IsHandleCreated && panelLoadingOverlay != null)
                {
                    string overlayMessage = $"GAME KẾT THÚC\n{message}";
                    if (labelLoading.InvokeRequired) { labelLoading.Invoke(new Action(() => labelLoading.Text = overlayMessage)); }
                    else { labelLoading.Text = overlayMessage; }

                    if (panelLoadingOverlay.InvokeRequired) { panelLoadingOverlay.Invoke(new Action(() => { panelLoadingOverlay.Visible = true; panelLoadingOverlay.BringToFront(); })); }
                    else { panelLoadingOverlay.Visible = true; panelLoadingOverlay.BringToFront(); }
                }

                if (actionButton != null && actionButton.Visible)
                {
                    if (actionButton.InvokeRequired) { actionButton.Invoke(new Action(() => actionButton.Visible = false)); }
                    else { actionButton.Visible = false; }
                }
            }
        }

        private async void UpdatePhaseAndTimerFromFirebase(string phase, string startTimeStr)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            Game currentGameData = null;
            if (!string.IsNullOrEmpty(gameId))
            {
                var fbHelper = new FirebaseHelper();
                currentGameData = await fbHelper.GetGame(gameId);
            }

            if (currentGameData != null && currentGameData.PhaseDuration.HasValue)
            {
                phaseDurationSeconds = currentGameData.PhaseDuration.Value;
            }

            if (currentGameData != null)
            {
                this.currentDay = currentGameData.RoundNumber > 0 ? currentGameData.RoundNumber : 1;
            }
            else
            {
                this.currentDay = 1;
            }

            if (currentGameData != null &&
                (currentGameData.Status == "villagers_win" || currentGameData.Status == "werewolves_win" || currentGameData.Status == "ended"))
            {
                Console.WriteLine($"DIAGNOSTIC InGameForm: UpdatePhaseAndTimerFromFirebase - Game already ended (Status: {currentGameData.Status}). Suppressing phase change UI updates.");
                if (!isGameReallyOver)
                {
                    isGameReallyOver = true;
                    currentPhaseClient = GamePhaseClient.Unknown;
                    if (phaseTimerUi != null) phaseTimerUi.Stop();
                    if (labelTimer != null && !labelTimer.IsDisposed)
                    {
                        if (labelTimer.InvokeRequired) { labelTimer.Invoke(new Action(() => labelTimer.Text = "GAME KẾT THÚC")); }
                        else { labelTimer.Text = "GAME KẾT THÚC"; }
                    }

                    if (IsHandleCreated && panelLoadingOverlay != null)
                    {
                        string endMessage = "";
                        if (currentGameData.Status == "villagers_win") endMessage = "Phe Dân làng đã thắng!";
                        else if (currentGameData.Status == "werewolves_win") endMessage = "Phe Sói đã thắng!";
                        else endMessage = "Trò chơi đã kết thúc.";
                        string overlayMessage = $"GAME KẾT THÚC\n{endMessage}";

                        if (labelLoading.InvokeRequired) { labelLoading.Invoke(new Action(() => labelLoading.Text = overlayMessage)); }
                        else { labelLoading.Text = overlayMessage; }

                        if (panelLoadingOverlay.InvokeRequired) { panelLoadingOverlay.Invoke(new Action(() => { panelLoadingOverlay.Visible = true; panelLoadingOverlay.BringToFront(); })); }
                        else { panelLoadingOverlay.Visible = true; panelLoadingOverlay.BringToFront(); }
                    }

                    if (actionButton != null && actionButton.Visible)
                    {
                        if (actionButton.InvokeRequired) { actionButton.Invoke(new Action(() => actionButton.Visible = false)); }
                        else { actionButton.Visible = false; }
                    }
                }
                return;
            }

            GamePhaseClient oldPhaseClient = currentPhaseClient;

            if (phase == lastFirebasePhaseRaw && startTimeStr == lastFirebaseStartTimeRaw && currentPhaseClient != GamePhaseClient.Unknown && !isGameReallyOver)
            {
                Console.WriteLine($"DIAGNOSTIC InGameForm: UpdatePhaseAndTimerFromFirebase - No change detected from last Firebase data ({phase}, {startTimeStr}). Current client phase: {currentPhaseClient}. Skipping update.");
                return;
            }

            lastFirebasePhaseRaw = phase;
            lastFirebaseStartTimeRaw = startTimeStr;
            Console.WriteLine($"DIAGNOSTIC InGameForm: UpdatePhaseAndTimerFromFirebase - Processing. New Firebase Phase: '{phase}', StartTime: '{startTimeStr}', Round for display logic: {this.currentDay}");

            GamePhaseClient newPhaseClientOnClient = GamePhaseClient.Unknown;
            string newPhaseDisplayName = "Không xác định";

            switch (phase?.ToLower())
            {
                case "night":
                    newPhaseClientOnClient = GamePhaseClient.Night;
                    newPhaseDisplayName = (this.currentDay == 1) ? "Đêm Đầu Tiên" : $"Đêm thứ {this.currentDay}";
                    break;
                case "day_discussion":
                    newPhaseClientOnClient = GamePhaseClient.DayDiscussion;
                    newPhaseDisplayName = $"Thảo Luận Sáng thứ {this.currentDay + 1}";
                    break;
                case "day_vote":
                    newPhaseClientOnClient = GamePhaseClient.DayVote;
                    newPhaseDisplayName = $"Bỏ Phiếu Sáng thứ {this.currentDay + 1}";
                    break;
                default:
                    newPhaseClientOnClient = GamePhaseClient.Unknown;
                    break;
            }

            bool actualPhaseChangeOnClient = (currentPhaseClient != newPhaseClientOnClient);

            if (actualPhaseChangeOnClient || currentPhaseClient == GamePhaseClient.Unknown)
            {
                currentPhaseClient = newPhaseClientOnClient;
                Console.WriteLine($"DIAGNOSTIC InGameForm: Client phase set to: {currentPhaseClient} (Actual Game Round: {this.currentDay})");

                if (actualPhaseChangeOnClient && currentPhaseClient != GamePhaseClient.Unknown && !isGameReallyOver)
                {
                    GameLog clientPhaseChangeLog = new GameLog
                    {
                        Timestamp = DateTime.UtcNow,
                        Message = $"Đã chuyển sang Giai đoạn {newPhaseDisplayName}.",
                        Phase = "system_phase_change"
                    };
                    DisplayGameLog(clientPhaseChangeLog);
                }
            }

            if (!string.IsNullOrEmpty(startTimeStr))
            {
                try
                {
                    phaseStartTimeUtc = DateTime.Parse(startTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();
                    Console.WriteLine($"DIAGNOSTIC InGameForm: Parsed phaseStartTimeUtc: {phaseStartTimeUtc:o}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DIAGNOSTIC InGameForm: Error parsing startTimeStr '{startTimeStr}'. Error: {ex.Message}. Using UtcNow.");
                    if (IsHandleCreated)
                    {
                        MessageBox.Show($"Lỗi định dạng thời gian bắt đầu phase, sử dụng UtcNow. Lỗi: {ex.Message}", "Lỗi Thời Gian Phase", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    phaseStartTimeUtc = DateTime.UtcNow;
                }
            }
            else
            {
                phaseStartTimeUtc = DateTime.UtcNow;
                Console.WriteLine($"DIAGNOSTIC InGameForm: startTimeStr is null/empty. Using UtcNow for phaseStartTimeUtc: {phaseStartTimeUtc:o}");
            }

            if (phaseTimerUi == null)
            {
                phaseTimerUi = new System.Windows.Forms.Timer { Interval = 1000 };
                phaseTimerUi.Tick += new System.EventHandler(this.PhaseTimerUi_Tick);
            }

            if (!isGameReallyOver && (currentGameData == null || (currentGameData.Status != "villagers_win" && currentGameData.Status != "werewolves_win" && currentGameData.Status != "ended")))
            {
                phaseTimerUi.Stop();
                PhaseTimerUi_Tick(null, EventArgs.Empty);
                phaseTimerUi.Start();
                Console.WriteLine($"DIAGNOSTIC InGameForm: Phase timer restarted for {currentPhaseClient} (Actual Game Round: {this.currentDay}). Duration: {phaseDurationSeconds}s");
            }
            else
            {
                if (phaseTimerUi != null) phaseTimerUi.Stop();
                Console.WriteLine($"DIAGNOSTIC InGameForm: Game ended or no data. Phase timer NOT restarted for {currentPhaseClient}. isGameReallyOver: {isGameReallyOver}");
            }
        }

        private async void PhaseTimerUi_Tick(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated || isGameReallyOver)
            {
                if (isGameReallyOver && phaseTimerUi != null && phaseTimerUi.Enabled)
                {
                    phaseTimerUi.Stop();
                }
                return;
            }

            TimeSpan elapsed = DateTime.UtcNow - phaseStartTimeUtc;
            int timeLeft = Math.Max(0, phaseDurationSeconds - (int)elapsed.TotalSeconds);
            UpdateTimerLabelUi(timeLeft);

            if (timeLeft <= 0 && phaseTimerUi.Enabled)
            {
                phaseTimerUi.Stop();
                Console.WriteLine($"DIAGNOSTIC InGameForm: Timer expired. User: {CurrentUserName}, isHost: {isHost}, Current Client Phase: {currentPhaseClient}");

                if (isHost && !isGameReallyOver) 
                {
                    var fb = new FirebaseHelper();
                    var game = await fb.GetGame(gameId);

                    if (game != null)
                    {
                        Console.WriteLine($"DIAGNOSTIC InGameForm: Host ({CurrentUserName}) sees current Firebase phase as: {game.CurrentPhase}, Game Status: {game.Status}");
                        if (game.Status != "villagers_win" && game.Status != "werewolves_win" && game.Status != "ended")
                        {
                            Console.WriteLine($"DIAGNOSTIC InGameForm: Host ({CurrentUserName}) is calling NextPhase_Host(). Current Firebase Phase from game object: {game.CurrentPhase}");
                            await NextPhase_Host();
                        }
                        else
                        {
                            Console.WriteLine($"DIAGNOSTIC InGameForm: Host ({CurrentUserName}) - Game already ended with status: {game.Status}. No phase change initiated from timer.");
                            ProcessGameStatusUpdate(game.Status); 
                        }
                    }
                    else
                    {
                        Console.WriteLine($"DIAGNOSTIC InGameForm: Host ({CurrentUserName}) - Could not get game data from Firebase. No phase change initiated from timer.");
                        if (IsHandleCreated) MessageBox.Show("Không thể lấy dữ liệu game từ Firebase để chuyển phase (Host).", "Lỗi Dữ Liệu Game", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (!isHost)
                {
                    Console.WriteLine($"DIAGNOSTIC InGameForm: Non-Host client ({CurrentUserName}) - Timer expired for phase {currentPhaseClient}. Waiting for Host to update Firebase.");
                }
            }
        }
        private void UpdateTimerLabelUi(int? timeToDisplayOverride = null)
        {
            if (this.IsDisposed || !this.IsHandleCreated || labelTimer == null || !labelTimer.IsHandleCreated) return;

            if (labelTimer.InvokeRequired)
            {
                labelTimer.Invoke(new Action(() => UpdateTimerLabelUi(timeToDisplayOverride)));
                return;
            }

            if (isGameReallyOver) 
            {
                if (labelTimer.Text != "GAME KẾT THÚC") labelTimer.Text = "GAME KẾT THÚC";
                return;
            }

            string phaseText = "";
            int timeValue;

            if (timeToDisplayOverride.HasValue)
            {
                timeValue = timeToDisplayOverride.Value;
            }
            else
            {
                TimeSpan elapsed = DateTime.UtcNow - phaseStartTimeUtc;
                timeValue = Math.Max(0, phaseDurationSeconds - (int)elapsed.TotalSeconds);
            }

            switch (currentPhaseClient)
            {
                case GamePhaseClient.Night:
                    phaseText = (this.currentDay == 1) ? "Đêm Đầu Tiên: " : $"Đêm {this.currentDay}: ";
                    break;
                case GamePhaseClient.DayDiscussion:
                    phaseText = $"Thảo luận Sáng {this.currentDay + 1}: ";
                    break;
                case GamePhaseClient.DayVote:
                    phaseText = $"Bỏ phiếu Sáng {this.currentDay + 1}: ";
                    break;
                case GamePhaseClient.Unknown: 
                    if (!isGameDataLoaded || string.IsNullOrEmpty(gameId) || lastFirebasePhaseRaw == null)
                    {
                        if (labelTimer.Text != "Đang tải...") labelTimer.Text = "Đang tải...";
                    }
                    else
                    {
                        if (labelTimer.Text != "Đang chờ...") labelTimer.Text = "Đang chờ...";
                    }
                    labelTimer.ForeColor = Color.Orange;
                    return;
                default:
                    phaseText = "Lỗi Phase: ";
                    break;
            }

            if (timeValue <= 0)
            {
                labelTimer.Text = phaseText + "Hết giờ!";
            }
            else
            {
                labelTimer.Text = phaseText + timeValue.ToString() + "s";
            }
            labelTimer.ForeColor = (timeValue <= 10 && timeValue > 0) ? Color.Red : Color.Orange;
        }

        private async void OnPlayerPanelClick(Player targetPlayer, Panel panelClicked)
        {
            if (isGameReallyOver)
            {
                if (actionButton != null && actionButton.Visible)
                {
                    if (actionButton.InvokeRequired) { actionButton.Invoke(new Action(() => actionButton.Visible = false)); }
                    else { actionButton.Visible = false; }
                }
                return;
            }

            if (targetPlayer == null || actionButton == null || this.IsDisposed || !this.IsHandleCreated) return;

            if (actionButton.Visible && selectedPlayerPanel != panelClicked)
            { actionButton.Visible = false; if (actionButton.IsHandleCreated) actionButton.Click -= CurrentActionButton_Click; }
            selectedPlayerPanel = panelClicked;

            var selfPlayerDoc = gamePlayers.FirstOrDefault(p => p.Id == currentUserId);
            if (selfPlayerDoc == null)
            {
                MessageBox.Show("Không tìm thấy thông tin người chơi hiện tại của bạn.", "Lỗi Người Chơi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool showButton = false; string newButtonText = "";

            if (!targetPlayer.IsAlive && !(currentUserRole == "hunter" && selfPlayerDoc.IsAlive == false && selfPlayerDoc.CanShoot && targetPlayer.IsAlive))
            { actionButton.Visible = false; return; }

            if (!selfPlayerDoc.IsAlive && !(currentUserRole == "hunter" && selfPlayerDoc.CanShoot))
            { actionButton.Visible = false; return; }


            if (currentPhaseClient == GamePhaseClient.Night)
            {
                if (currentUserRole == "werewolf" && targetPlayer.Name != CurrentUserName && targetPlayer.IsAlive) { showButton = true; newButtonText = "Giết"; }
                else if (currentUserRole == "seer" && targetPlayer.Name != CurrentUserName && targetPlayer.IsAlive) { showButton = true; newButtonText = "Soi"; }
                else if (currentUserRole == "bodyguard" && targetPlayer.IsAlive) { showButton = true; newButtonText = "Bảo vệ"; }
                else if (currentUserRole == "witch" && targetPlayer.IsAlive) { showButton = true; newButtonText = $"Dùng Thuốc lên {targetPlayer.Name}"; }
            }
            else if (currentPhaseClient == GamePhaseClient.DayVote)
            {
                if (targetPlayer.Name != CurrentUserName && targetPlayer.IsAlive && selfPlayerDoc.IsAlive && !selfPlayerDoc.HasVotedToday)
                { showButton = true; newButtonText = "Bỏ phiếu Treo cổ"; }
            }

            if (currentUserRole == "hunter" && selfPlayerDoc.IsAlive == false && selfPlayerDoc.CanShoot && targetPlayer.Name != CurrentUserName && targetPlayer.IsAlive)
            { showButton = true; newButtonText = "Bắn!"; }

            if (showButton)
            {
                actionButton.Text = newButtonText;
                Point panelScreenLocation = panelClicked.PointToScreen(Point.Empty);
                Point panelFormLocation = this.PointToClient(panelScreenLocation);
                int buttonX = panelFormLocation.X + (panelClicked.Width - actionButton.Width) / 2;
                int buttonY = panelFormLocation.Y + panelClicked.Height + 5;
                if (buttonY + actionButton.Height > this.ClientSize.Height - 5) { buttonY = panelFormLocation.Y - actionButton.Height - 5; }
                buttonX = Math.Max(5, Math.Min(buttonX, this.ClientSize.Width - actionButton.Width - 5));
                actionButton.Location = new Point(buttonX, buttonY);
                actionButton.Visible = true; actionButton.BringToFront();
                currentActionTargetPlayerName = targetPlayer.Name; currentActionButtonText = newButtonText;
                if (actionButton.IsHandleCreated) actionButton.Click -= CurrentActionButton_Click; // Gỡ bỏ handler cũ trước khi thêm mới
                if (actionButton.IsHandleCreated) actionButton.Click += new System.EventHandler(this.CurrentActionButton_Click);
            }
            else { if (actionButton.IsHandleCreated) actionButton.Visible = false; if (actionButton.IsHandleCreated) actionButton.Click -= CurrentActionButton_Click; }
        }

        private async void CurrentActionButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentActionTargetPlayerName) || string.IsNullOrEmpty(currentActionButtonText) || isGameReallyOver) return;
            Button clickedButton = (Button)sender;
            if (clickedButton.IsHandleCreated) clickedButton.Visible = false;

            var firebase = new FirebaseHelper();
            var currentPlayersInGame = await firebase.GetPlayers(gameId);
            var targetPlayerDoc = currentPlayersInGame.FirstOrDefault(p => p.Name == currentActionTargetPlayerName);
            var selfPlayerDoc = currentPlayersInGame.FirstOrDefault(p => p.Id == currentUserId);

            if (selfPlayerDoc == null || (!selfPlayerDoc.IsAlive && !(currentUserRole == "hunter" && selfPlayerDoc.CanShoot)))
            {
                MessageBox.Show("Bạn đã chết hoặc không thể thực hiện hành động này.", "Thông Báo Hành Động", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (targetPlayerDoc == null ||
                (!targetPlayerDoc.IsAlive && !(currentActionButtonText == "Bắn!" && currentUserRole == "hunter" && targetPlayerDoc != null && targetPlayerDoc.IsAlive)))
            {
                MessageBox.Show("Mục tiêu không hợp lệ hoặc đã chết.", "Thông Báo Hành Động", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (IsHandleCreated) MessageBox.Show($"Bạn đã chọn: {currentActionButtonText} -> {currentActionTargetPlayerName}", "Xác Nhận Hành Động", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (currentActionButtonText == "Giết" && currentUserRole == "werewolf")
                {
                    await firebase.WereWolfAction(gameId, currentUserId, targetPlayerDoc.Id);
                }
                else if (currentActionButtonText == "Soi" && currentUserRole == "seer")
                {
                    var r = await firebase.SeerCheckPlayer(gameId, currentUserId, targetPlayerDoc.Id);
                    if (IsHandleCreated) MessageBox.Show($"Bạn soi {targetPlayerDoc.Name}.\nKết quả: {(r.IsWerewolf ? "Phe Sói" : "Không phải Phe Sói")}.\nVai trò: {r.Role}", "Kết Quả Soi (Riêng)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (currentActionButtonText == "Bảo vệ" && currentUserRole == "bodyguard")
                {
                    await firebase.BodyguardProtectPlayer(gameId, currentUserId, targetPlayerDoc.Id);
                }
                else if (currentActionButtonText.StartsWith("Dùng Thuốc lên") && currentUserRole == "witch")
                {
                    DialogResult choice = DialogResult.Cancel;
                    if (IsHandleCreated) choice = MessageBox.Show($"Bạn muốn dùng thuốc gì lên {targetPlayerDoc.Name}?\n[Yes] = Thuốc Cứu (Còn: {!selfPlayerDoc.HasUsedHealPotion})\n[No] = Thuốc Độc (Còn: {!selfPlayerDoc.HasUsedKillPotion})\n[Cancel] = Hủy", "Phù Thủy Dùng Thuốc", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (choice == DialogResult.Yes)
                    {
                        if (!selfPlayerDoc.HasUsedHealPotion) await firebase.WitchUseHealPotion(gameId, currentUserId, targetPlayerDoc.Id);
                        else if (IsHandleCreated) MessageBox.Show("Bạn đã hết thuốc cứu.", "Thông Báo Phù Thủy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (choice == DialogResult.No)
                    {
                        if (targetPlayerDoc.Id == currentUserId && IsHandleCreated) MessageBox.Show("Không thể tự dùng thuốc độc.", "Thông Báo Phù Thủy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        else if (!selfPlayerDoc.HasUsedKillPotion) await firebase.WitchUseKillPotion(gameId, currentUserId, targetPlayerDoc.Id);
                        else if (IsHandleCreated) MessageBox.Show("Bạn đã hết thuốc độc.", "Thông Báo Phù Thủy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (currentActionButtonText == "Bỏ phiếu Treo cổ" && currentPhaseClient == GamePhaseClient.DayVote)
                {
                    if (!selfPlayerDoc.HasVotedToday) await firebase.RecordDayVote(gameId, currentUserId, targetPlayerDoc.Id);
                    else if (IsHandleCreated) MessageBox.Show("Bạn đã bỏ phiếu trong ngày hôm nay rồi.", "Thông Báo Bỏ Phiếu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (currentActionButtonText == "Bắn!" && currentUserRole == "hunter" && selfPlayerDoc.CanShoot)
                {
                    if (targetPlayerDoc.Id == currentUserId && IsHandleCreated) MessageBox.Show("Không thể tự bắn mình.", "Thông Báo Thợ Săn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else await firebase.HunterShootPlayer(gameId, currentUserId, targetPlayerDoc.Id);
                }
            }
            catch (InvalidOperationException opEx)
            {
                if (IsHandleCreated) MessageBox.Show(opEx.Message, "Lỗi Hành Động", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                if (IsHandleCreated) MessageBox.Show($"Lỗi không xác định khi thực hiện hành động: {ex.Message}", "Lỗi Thực Hiện Hành Động", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Action Error: {ex.ToString()}");
            }
        }

        private async Task NextPhase_Host()
        {
            if (this.IsDisposed || !this.IsHandleCreated || isGameReallyOver) return; // Không làm gì nếu game đã kết thúc
            Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() called by Host ({CurrentUserName}).");
            try
            {
                var firebaseHelper = new FirebaseHelper();
                var game = await firebaseHelper.GetGame(gameId);
                if (game == null)
                {
                    Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - Failed to get game data for gameId: {gameId}");
                    if (IsHandleCreated) MessageBox.Show("Không thể lấy dữ liệu game trong NextPhase_Host.", "Lỗi (Host)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (game.Status == "villagers_win" || game.Status == "werewolves_win" || game.Status == "ended")
                {
                    Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - Game already ended with status: {game.Status}.");
                    ProcessGameStatusUpdate(game.Status);
                    return;
                }

                string currentPhaseStrFromFirebase = game.CurrentPhase;
                Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - Current phase from Firebase is '{currentPhaseStrFromFirebase}'.");

                if (currentPhaseStrFromFirebase.ToLower() == "night")
                {
                    if (IsHandleCreated && panelLoadingOverlay != null && !isGameReallyOver) { labelLoading.Text = "Đang xử lý kết quả đêm..."; panelLoadingOverlay.Visible = true; }
                    Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - Processing night results...");
                    await firebaseHelper.ProcessNightResults(gameId);
                    Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - Night results processed.");
                }
                else if (currentPhaseStrFromFirebase.ToLower() == "day_vote")
                {
                    if (IsHandleCreated && panelLoadingOverlay != null && !isGameReallyOver) { labelLoading.Text = "Đang xử lý kết quả bỏ phiếu..."; panelLoadingOverlay.Visible = true; }
                    Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - Processing day vote results...");
                    await firebaseHelper.ProcessDayVoteResults(gameId);
                    Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - Day vote results processed.");
                }

                if (isGameReallyOver) 
                {
                    if (IsHandleCreated && panelLoadingOverlay != null) panelLoadingOverlay.Visible = true; 
                    return;
                }


                Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - Calling firebaseHelper.NextPhase with current Firebase phase: {currentPhaseStrFromFirebase}.");
                string gameEndStatus = await firebaseHelper.NextPhase(gameId, currentPhaseStrFromFirebase);
                Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - firebaseHelper.NextPhase call completed. Game end status: '{gameEndStatus ?? "null"}'");

                if (IsHandleCreated && panelLoadingOverlay != null && !isGameReallyOver) panelLoadingOverlay.Visible = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DIAGNOSTIC InGameForm: NextPhase_Host() - Exception: {ex.ToString()}");
                if (IsHandleCreated)
                {
                    MessageBox.Show($"Lỗi khi chuyển phase (Host): {ex.Message}", "Lỗi Chuyển Phase", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (panelLoadingOverlay != null && !isGameReallyOver) panelLoadingOverlay.Visible = false;
                }
            }
        }

        private void TableLayoutPanel1_Click(object sender, EventArgs e)
        {
            if (actionButton != null && actionButton.Visible && actionButton.IsHandleCreated)
            {
                Point cursorPosOnForm = this.PointToClient(Cursor.Position);
                if (!actionButton.Bounds.Contains(cursorPosOnForm))
                { actionButton.Visible = false; actionButton.Click -= CurrentActionButton_Click; selectedPlayerPanel = null; }
            }
        }

        private void btnRole_Click(object sender, EventArgs e) { RolesForm rolesForm = new RolesForm(); rolesForm.ShowDialog(); }
        private void richTextBox2_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter && button1 != null) { button1.PerformClick(); e.SuppressKeyPress = true; } }

        private async void BtnQuit_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Bạn có chắc chắn muốn thoát về lobby?",
                    "Xác Nhận Thoát",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (isServerConnected && tcpClient != null && networkStream != null && !string.IsNullOrEmpty(chatRoomId) && !string.IsNullOrEmpty(CurrentUserName))
                    {
                        SendMessage($"QUIT_ROOM:{chatRoomId}:{CurrentUserName}"); // Sử dụng SendMessage đồng bộ gốc
                    }

                    // Dọn dẹp listeners và timers
                    if (firebasePhaseListener != null) { firebasePhaseListener.Dispose(); firebasePhaseListener = null; }
                    if (firebaseGameStatusListener != null) { firebaseGameStatusListener.Dispose(); firebaseGameStatusListener = null; }
                    if (firebasePlayersListener != null) { firebasePlayersListener.Dispose(); firebasePlayersListener = null; }
                    if (firebaseGameLogListener != null) { firebaseGameLogListener.Dispose(); firebaseGameLogListener = null; }

                    if (phaseTimerUi != null) { phaseTimerUi.Stop(); phaseTimerUi.Dispose(); phaseTimerUi = null; }
                    if (_updatePlayersDisplayDebounceTimer != null) { _updatePlayersDisplayDebounceTimer.Stop(); _updatePlayersDisplayDebounceTimer.Dispose(); _updatePlayersDisplayDebounceTimer = null; }


                    if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Cancel();
                    }
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                if (IsHandleCreated && !IsDisposed) // Kiểm tra trước khi hiển thị MessageBox
                {
                    MessageBox.Show($"Lỗi khi thoát game: {ex.Message}", "Lỗi Thoát Game", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Console.WriteLine($"Error in BtnQuit_Click (form disposed): {ex.Message}");
                }
            }
        }

        private async void InGameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ngăn form đóng ngay để xử lý bất đồng bộ, logic này từ file gốc của bạn
            e.Cancel = true;

            // Hủy các listeners (có thể đã được hủy trong BtnQuit_Click nhưng đảm bảo ở đây)
            firebasePhaseListener?.Dispose(); firebasePhaseListener = null;
            firebaseGameStatusListener?.Dispose(); firebaseGameStatusListener = null;
            firebasePlayersListener?.Dispose(); firebasePlayersListener = null;
            firebaseGameLogListener?.Dispose(); firebaseGameLogListener = null;

            phaseTimerUi?.Stop(); phaseTimerUi?.Dispose(); phaseTimerUi = null;
            _updatePlayersDisplayDebounceTimer?.Stop(); _updatePlayersDisplayDebounceTimer?.Dispose(); _updatePlayersDisplayDebounceTimer = null;

            // Hủy CancellationTokenSource cho luồng nhận TCP
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            // Chờ luồng nhận TCP kết thúc (với timeout ngắn)
            if (_receivingTask != null && !_receivingTask.IsCompleted)
            {
                try
                {
                    await Task.WhenAny(_receivingTask, Task.Delay(200));
                }
                catch (ObjectDisposedException) {}
                catch (Exception) {}
            }
            _cancellationTokenSource?.Dispose(); _cancellationTokenSource = null;


            // Đóng kết nối TCP
            if (networkStream != null)
            {
                try { networkStream.Close(); networkStream.Dispose(); } catch { /* Ignore */ }
                networkStream = null;
            }
            if (tcpClient != null)
            {
                try { tcpClient.Close(); tcpClient.Dispose(); } catch { /* Ignore */ }
                tcpClient = null;
            }
            isServerConnected = false;

            e.Cancel = false;
            if (IsHandleCreated && !IsDisposed)
            {
                this.BeginInvoke((MethodInvoker)delegate {
                    if (!IsDisposed)
                    {
                        base.Close(); 
                    }
                });
            }
            else if (!IsDisposed)
            {
                base.Close(); 
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}