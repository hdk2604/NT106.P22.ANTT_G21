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
        private string currentActionTargetPlayerName; 
        private string currentActionButtonText;
        private System.Threading.CancellationTokenSource _cancellationTokenSource;
        private Task _receivingTask;

        public InGameForm(List<string> playerNames, TcpClient existingClient = null)
        {
            InitializeComponent();
            this.playerNames = playerNames;
            this.players = new List<string>(playerNames);
            uiContext = SynchronizationContext.Current;
            Load += Form1_Load;
            ConnectToServer(existingClient);
            tableLayoutPanel1.Click += TableLayoutPanel1_Click;
            this.FormClosing += InGameForm_FormClosing;
        }

        public InGameForm() : this(new List<string>()) { }

        private string placeholderText = "Nhập tin nhắn của bạn";
        private async void Form1_Load(object sender, EventArgs e)
        {
            // Khởi tạo actionButton một lần
            this.actionButton = new System.Windows.Forms.Button();
            this.actionButton.Size = new System.Drawing.Size(120, 35); // Kích thước tùy chỉnh
            this.actionButton.Visible = false; // Ẩn ban đầu
            this.actionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.actionButton.FlatAppearance.BorderSize = 1;
            this.actionButton.FlatAppearance.BorderColor = System.Drawing.Color.Aqua; // Màu viền nổi bật
            this.actionButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48))))); // Màu nền tối
            this.actionButton.ForeColor = System.Drawing.Color.White; // Màu chữ trắng
            this.actionButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.actionButton.Name = "playerActionButton"; // Đặt tên để dễ nhận biết
            this.actionButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Controls.Add(this.actionButton); // Thêm vào Form chính
            this.actionButton.BringToFront();     // Đảm bảo nó hiện trên các control khác

            // panelLoadingOverlay đã được Visible = true từ Designer.
            panelLoadingOverlay.Location = tableLayoutPanel1.Location; 
            panelLoadingOverlay.Size = tableLayoutPanel1.Size;         
            panelLoadingOverlay.BringToFront();                        

            // Setup UI
            panelTopLeft.BackColor = Color.FromArgb(100, 0, 0, 0); 
            panelRole.BackColor = Color.FromArgb(100, 0, 0, 0);    
            labelTimer.Font = new Font("Segoe UI", 28F, FontStyle.Bold); 
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

            // Load data sau
            if (!isDataLoaded && !string.IsNullOrEmpty(currentUserId) && !string.IsNullOrEmpty(gameId)) //
            {
                isDataLoaded = true; //
                try
                {
                    if (IsHandleCreated) labelLoading.Text = "Đang chuẩn bị giao diện người chơi..."; //
                    await AddPanelsToTable(); // Gọi và chờ AddPanelsToTable hoàn thành //

                    if (IsHandleCreated) labelLoading.Text = "Đang đồng bộ trạng thái trò chơi..."; //
                    ListenPhaseFromFirebase(); // Giả sử hàm này không block lâu hoặc chạy nền //
                    await LoadCurrentPhaseFromFirebase(); //

                    if (IsHandleCreated) panelLoadingOverlay.Visible = false; // Ẩn panel loading khi tất cả đã xong //
                }
                catch (Exception ex)
                {
                    isDataLoaded = false; 
                    if (IsHandleCreated)
                    {
                        labelLoading.Text = $"Lỗi tải dữ liệu: {ex.Message}"; //
                        MessageBox.Show($"Error loading game data: {ex.Message}"); //
                    }
                }
            }
            else if (isDataLoaded) // Nếu form load lại nhưng dữ liệu đã có //
            {
                await AddPanelsToTable();
                if (IsHandleCreated) panelLoadingOverlay.Visible = false; //
            }
            else // Không có currentUserId hoặc gameId //
            {
                if (IsHandleCreated)
                {
                    labelLoading.Text = "Lỗi: Thông tin game không hợp lệ."; //
                                                                            
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
                    if (!string.IsNullOrEmpty(message))
                    {
                        SendMessage($"CHAT_MESSAGE:{roomId}:{CurrentUserName}:{message}");
                        richTextBox2.Clear();
                        SetPlaceholder();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi tin nhắn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectToServer(TcpClient existingClient = null)
        {
            try
            {
                // Initialize CancellationTokenSource
                _cancellationTokenSource = new System.Threading.CancellationTokenSource();

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

                // Start receiving messages asynchronously on a background thread
                _receivingTask = Task.Run(() => StartReceivingAsync(_cancellationTokenSource.Token));
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

        private async Task StartReceivingAsync(System.Threading.CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096];
            StringBuilder messageBuffer = new StringBuilder();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = 0;
                    // Use ReadAsync with cancellation token
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead > 0)
                    {
                        messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                        string content = messageBuffer.ToString();

                        // Process complete messages
                        int lastNewLine = content.LastIndexOf('\n');
                        if (lastNewLine >= 0)
                        {
                            string[] messages = content.Substring(0, lastNewLine + 1).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string message in messages)
                            {
                                if (!string.IsNullOrEmpty(message.Trim()))
                                {
                                    string trimmedMessage = message.Trim();
                                    uiContext.Post(_ => ProcessServerMessage(trimmedMessage), null);
                                }
                            }
                            // Keep the remaining partial message
                            messageBuffer.Clear();
                            messageBuffer.Append(content.Substring(lastNewLine + 1));
                        }
                    }
                    else // Connection was closed by the remote host gracefully
                    {
                        // This can happen if the server closes the connection
                        isConnected = false; // Signal to exit the loop
                        break; // Exit the while loop
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // This exception is expected when the cancellation token is signaled
                isConnected = false;
            }
            catch (Exception ex)
            {
                // Handle other unexpected exceptions in the receive loop
                isConnected = false;
                uiContext.Post(_ =>
                {
                    // Show the error but don't automatically close the form here
                    // The FormClosing event handler will handle cleanup and closing
                    MessageBox.Show($"Lỗi trong luồng nhận dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }, null);
            }
            finally
            {
                // Ensure resources are cleaned up if the receive task completes
                // Closing client/stream here is generally safe after the receive loop exits
                // However, we primarily rely on the UI thread's cleanup for consistency
                // No specific cleanup needed directly in this finally block if UI thread handles it
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
                        if (parts.Length >= 4)
                        {
                            string roomId = parts[1];
                            string sender = parts[2];
                            string msg = string.Join(":", parts.Skip(3));
                            if (roomId == this.roomId)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    AddChatMessage(sender, msg);
                                });
                            }
                        }
                        break;
                    case "PLAYER_JOINED":
                        if (parts.Length >= 2)
                        {
                            string newPlayer = parts[1];
                            this.Invoke((MethodInvoker)delegate
                            {
                                OnPlayerJoined(newPlayer);
                                AddChatMessage("Hệ thống", $"{newPlayer} đã tham gia phòng");
                            });
                        }
                        break;
                    case "PLAYER_LEFT":
                        if (parts.Length >= 2)
                        {
                            string leftPlayer = parts[1];
                            this.Invoke((MethodInvoker)delegate
                            {
                                OnPlayerLeft(leftPlayer);
                                AddChatMessage("Hệ thống", $"{leftPlayer} đã rời khỏi phòng");
                            });
                        }
                        break;
                    case "PLAYER_LIST":
                        if (parts.Length >= 2)
                        {
                            string[] playerList = parts[1].Split(',');
                            this.Invoke((MethodInvoker)delegate
                            {
                                players = playerList.ToList();
                                UpdatePlayerList();
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xử lý tin nhắn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                string timestamp = DateTime.Now.ToString("HH:mm");
                string formattedMessage = $"[{timestamp}] {sender}: {message}\n";
                
                richTextBox1.AppendText(formattedMessage);
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
            catch (Exception ex)
            {
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

        private async void UpdatePlayerList() 
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdatePlayerList));
                return;
            }
            playerNames = new List<string>(players);
            await AddPanelsToTable();
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

        private async Task AddPanelsToTable() 
        {

            if (tableLayoutPanel1.InvokeRequired)
            {
                await (Task)tableLayoutPanel1.Invoke(new Func<Task>(AddPanelsToTable));
                return;
            }

            try
            {
                tableLayoutPanel1.SuspendLayout(); 
                if (string.IsNullOrEmpty(currentUserRole) && !string.IsNullOrEmpty(currentUserId) && !string.IsNullOrEmpty(gameId))
                {
                    if (panelLoadingOverlay.Visible && IsHandleCreated) 
                    {
                        labelLoading.Text = "Đang tải thông tin vai trò...";
                    }
                    await LoadCurrentUserRole();
                }
                var firebase = new FirebaseHelper();
                var playerObjects = await firebase.GetPlayers(gameId);
                var alivePlayers = playerObjects.Where(p => p.IsAlive).ToList();
                playerNames = alivePlayers.Select(p => p.Name).ToList();

                tableLayoutPanel1.Controls.Clear();
                Image originalImage = Properties.Resources.UserIcon;
                int newWidth = 70;
                int newHeight = 70;
                Image resizedImage = new Bitmap(originalImage, new Size(newWidth, newHeight));
                int playerIndex = 0;

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
                                pictureBox.Image = Properties.Resources.UserIcon; // Sử dụng UserIcon thay vì resizedImage nếu bạn muốn icon mặc định
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
                // Ghi log lỗi, ví dụ: vào richTextBox1 nếu nó đã sẵn sàng
                if (IsHandleCreated && richTextBox1 != null) // Kiểm tra IsHandleCreated
                {
                    richTextBox1.AppendText($"Error in AddPanelsToTable: {ex.Message}\n");
                }
            }
            finally
            {
                if (IsHandleCreated && tableLayoutPanel1 != null) // Kiểm tra IsHandleCreated
                {
                    tableLayoutPanel1.ResumeLayout(true); // Cho phép vẽ lại layout
                }
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
                        if (IsHandleCreated)
                        {
                            richTextBox1.AppendText($"Invalid startTime format, using UtcNow. Parse error: {ex.Message}\n");
                        }
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
                    if (IsHandleCreated)
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
                }

                // Log
                if (IsHandleCreated)
                {
                    richTextBox1.AppendText($"Phase updated to {phase}, startTime={phaseStartTime.ToString("o")}, timer started\n");
                }
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
                if (IsHandleCreated)
                {
                    UpdateTimerLabel(timeLeft);
                }
                
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

        private async void OnPlayerPanelClick(string targetPlayerName, Panel panelClicked) // panelClicked là panel của người chơi được nhấp vào
        {
            var firebase = new FirebaseHelper();
            var players = await firebase.GetPlayers(gameId);
            var targetPlayer = players.FirstOrDefault(p => p.Name == targetPlayerName);
            if (targetPlayer == null || !targetPlayer.IsAlive) return;

            // Luôn gỡ bỏ event handler cũ trước khi quyết định có gán lại hay không
            if (this.actionButton != null)
            {
                this.actionButton.Click -= CurrentActionButton_Click; // Sử dụng this.actionButton để rõ ràng
                this.actionButton.Visible = false; // Ẩn nút đi trước khi kiểm tra logic mới
            }

            bool showButton = false;
            string buttonText = "";

            if (currentPhase == GamePhase.Night) //
            {
                if (currentUserRole == "werewolf" && targetPlayerName != CurrentUserName) //
                {
                    showButton = true; //
                    buttonText = "Giết"; //
                }
                else if (currentUserRole == "bodyguard") // Bodyguard có thể tự bảo vệ hoặc bảo vệ người khác //
                {
                    showButton = true; //
                    buttonText = "Bảo vệ"; //
                }
                else if (currentUserRole == "seer" && targetPlayerName != CurrentUserName) //
                {
                    showButton = true; //
                    buttonText = "Soi"; //
                }
                else if (currentUserRole == "witch") // Logic cho Witch cần cụ thể hơn về việc dùng thuốc nào //
                {
                    // Ví dụ đơn giản: Nút chung "Dùng Kỹ năng", logic chi tiết hơn khi click
                    showButton = true; //
                    buttonText = "Dùng Thuốc";
                }
            }
            else if (currentPhase == GamePhase.DayVote) //
            {
                // Chỉ cho phép bỏ phiếu cho người khác và người chơi hiện tại chưa vote
                if (targetPlayerName != CurrentUserName /* && !currentPlayerHasVoted (cần thêm biến trạng thái này) */)
                {
                    showButton = true;
                    buttonText = "Bỏ phiếu";
                }
            }
            // ... Các logic khác cho các vai trò và phase khác ...

            if (showButton && this.actionButton != null)
            {
                this.actionButton.Text = buttonText;

                // Tính toán vị trí cho actionButton (là con của Form) để nó xuất hiện gần panelClicked
                Point panelScreenLocation = panelClicked.PointToScreen(Point.Empty); // Tọa độ của panelClicked trên màn hình
                Point panelFormLocation = this.PointToClient(panelScreenLocation);    // Chuyển sang tọa độ của Form
                int buttonX = panelFormLocation.X + (panelClicked.Width - this.actionButton.Width) / 2; // Căn giữa theo chiều ngang của panelClicked
                int buttonY = panelFormLocation.Y + panelClicked.Height + 5; // Cách panelClicked 5px về phía dưới

                // Nếu nút bị tràn ra khỏi mép dưới của Form, đặt nó bên trên panelClicked
                if (buttonY + this.actionButton.Height > this.ClientSize.Height - 5) // Cách mép form 5px
                {
                    buttonY = panelFormLocation.Y - this.actionButton.Height - 5; // Cách panelClicked 5px về phía trên
                }

                // Đảm bảo nút không bị lệch ra ngoài Form theo chiều ngang
                if (buttonX < 5) buttonX = 5; // Cách mép trái 5px
                if (buttonX + this.actionButton.Width > this.ClientSize.Width - 5) // Cách mép phải 5px
                {
                    buttonX = this.ClientSize.Width - this.actionButton.Width - 5;
                }

                this.actionButton.Location = new Point(buttonX, buttonY);
                this.actionButton.Visible = true;
                this.actionButton.BringToFront(); // Đảm bảo nút luôn nằm trên

                // Lưu thông tin cho event handler và gán lại event
                currentActionTargetPlayerName = targetPlayerName;
                currentActionButtonText = buttonText;
                this.actionButton.Click += CurrentActionButton_Click;
            }
        }

        private async void CurrentActionButton_Click(object sender, EventArgs e)
        {
            var firebase = new FirebaseHelper();

            var players = await firebase.GetPlayers(gameId);
            var targetPlayer = players.FirstOrDefault(p => p.Name == currentActionTargetPlayerName);

            if (targetPlayer == null || !targetPlayer.IsAlive)
            {
                AddChatMessage("Lỗi", "Không thể chọn người chơi đã chết hoặc không tồn tại.");
                return;
            }

            AddChatMessage("Hành động", $"Bạn đã chọn {currentActionButtonText} người chơi {currentActionTargetPlayerName}.");

        

            if (currentActionButtonText == "Giết" && currentUserRole == "werewolf")
            {
                await firebase.WereWolfAction(gameId, currentUserId, targetPlayer.Id);
                AddChatMessage("Hệ thống", $"Bạn đã chọn giết {currentActionTargetPlayerName}.");
                AddChatMessage("DEBUG", $"werewolfId = {currentUserId}");
                AddChatMessage("DEBUG", $"targetPlayerId = {targetPlayer.Id}");
                AddChatMessage("DEBUG", $"gameId = {gameId}");
            }
           
        }

        private async Task NextPhase_Host()
        {
            try
            {
                var firebaseHelper = new FirebaseHelper();

                // Nếu là phase "night", xử lý kết quả đêm trước khi chuyển phase
                if (currentPhase == GamePhase.Night)
                {
                    await firebaseHelper.ProcessNightResults(gameId);
                }

                string currentPhaseStr;
                switch (currentPhase)
                {
                    case GamePhase.Night:
                        currentPhaseStr = "night";
                        break;
                    case GamePhase.DayDiscussion:
                        currentPhaseStr = "day_discussion";
                        break;
                    case GamePhase.DayVote:
                        currentPhaseStr = "day_vote";
                        break;
                    default:
                        currentPhaseStr = "night";
                        break;
                }

                await firebaseHelper.NextPhase(gameId, currentPhaseStr);

                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error in NextPhase_Host: {ex.Message}\n");
            }
        }
        private void TableLayoutPanel1_Click(object sender, EventArgs e)
        {
            if (this.actionButton != null && this.actionButton.Visible)
            {
                Point cursorPosOnForm = this.PointToClient(Cursor.Position);
                if (!this.actionButton.Bounds.Contains(cursorPosOnForm))
                {
                    this.actionButton.Visible = false;
                    this.actionButton.Click -= CurrentActionButton_Click;
                }
            }
        }


        private void btnRole_Click(object sender, EventArgs e)
        {
            RolesForm rolesForm = new RolesForm();
            rolesForm.ShowDialog();
        }


        private async void BtnQuit_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Bạn có chắc chắn muốn thoát về lobby?",
                    "Xác nhận thoát",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Send quit message to server
                    if (isConnected && client != null && stream != null)
                    {
                        SendMessage($"QUIT_ROOM:{roomId}:{CurrentUserName}");
                    }

                    // Clean up resources (timers, listeners)
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
                    if (pollTimer != null)
                    {
                        pollTimer.Stop();
                        pollTimer.Dispose();
                    }
                    if (phaseTimer != null)
                    {
                        phaseTimer.Stop();
                        phaseTimer.Dispose();
                    }

                    // Signal cancellation to the receiving task
                    if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Cancel();
                    }

                    // No need to await here, rely on FormClosing to await the task

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thoát game: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void InGameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Prevent the form from closing immediately to allow async cleanup
            e.Cancel = true;

            // Add cleanup logic here
            // Clean up resources (timers, listeners)
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
            if (pollTimer != null)
            {
                pollTimer.Stop();
                pollTimer.Dispose();
            }
            if (phaseTimer != null)
            {
                phaseTimer.Stop();
                phaseTimer.Dispose();
            }

            // Signal cancellation to the receiving task
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            // Wait for the receiving task to complete after cancellation is signaled
            if (_receivingTask != null && !_receivingTask.IsCompleted)
            {
                try
                {
                    // Wait for the task to finish. It should exit after cancellation.
                    // Add a timeout to prevent indefinite waiting if something goes wrong.
                    await Task.WhenAny(_receivingTask, Task.Delay(100)); // Wait up to 500 ms
                }
                catch { /* Ignore exceptions during await */ }
            }

            // Clean up network resources after the receiving task has stopped
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }

            // Dispose CancellationTokenSource
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            // Now that cleanup is done, allow the form to close
            e.Cancel = false;
            // Manually close the form again now that cancellation is not pending
            // Need to use Invoke because this might be called from a background thread due to async void
            if (IsHandleCreated) // Check before invoking
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    this.Close();
                });
            }
        }
    }
}

