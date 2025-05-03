using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

namespace WerewolfClient.Forms
{
    public partial class RolesForm : Form
    {
        private Dictionary<string, RoleDescription> roles;
        private Button currentSelectedBtn;
        private readonly Color defaultBtnColor = Color.FromArgb(65, 65, 100);
        private readonly Color hoverBtnColor = Color.FromArgb(80, 80, 120);
        private readonly Color selectedBtnColor = Color.FromArgb(220, 80, 80);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public RolesForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            InitializeRoles();
            SetupUI();
            ApplyModernStyling();
            this.Load += RolesForm_Load;
        }

        private void InitializeRoles()
        {
            roles = new Dictionary<string, RoleDescription>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "MA SÓI",
                    new RoleDescription(
                        "Mỗi đêm, Ma Sói thức dậy và chọn một người để giết." + Environment.NewLine + Environment.NewLine +
                        "• Hoạt động ban đêm" + Environment.NewLine +
                        "• Có thể nói chuyện với đồng bọn" + Environment.NewLine +
                        "• Chiến thắng khi số lượng bằng dân làng",
                        Properties.Resources.WereWolfIcon)
                },
                {
                    "TIÊN TRI",
                    new RoleDescription(
                        "Mỗi đêm có thể kiểm tra 1 người là Sói hay Dân làng." + Environment.NewLine + Environment.NewLine +
                        "• Nhận kết quả chính xác" + Environment.NewLine +
                        "• Thông tin quan trọng cho phe Dân" + Environment.NewLine +
                        "• Dễ bị Sói nhắm đến",
                        Properties.Resources.Seer)
                },
                {
                    "BẢO VỆ",
                    new RoleDescription(
                        "Mỗi đêm có thể bảo vệ 1 người khỏi bị Sói cắn." + Environment.NewLine + Environment.NewLine +
                        "• Không thể bảo vệ cùng người 2 đêm liên tiếp" + Environment.NewLine +
                        "• Có thể tự bảo vệ bản thân",
                        Properties.Resources.Guardian)
                },
                {
                    "THỢ SĂN",
                    new RoleDescription(
                        "Khi bị giết hoặc bị treo cổ, có thể bắn chết 1 người khác." + Environment.NewLine + Environment.NewLine +
                        "• Phải sử dụng khả năng ngay khi chết" + Environment.NewLine +
                        "• Có thể chọn không bắn ai",
                        Properties.Resources.Hunter)
                },
                {
                    "PHÙ THỦY",
                    new RoleDescription(
                        "Có 2 bình thuốc: 1 cứu người, 1 giết người." + Environment.NewLine + Environment.NewLine +
                        "• Mỗi bình chỉ dùng 1 lần" + Environment.NewLine +
                        "• Có thể cứu chính mình" + Environment.NewLine +
                        "• Không thể dùng cả 2 bình cùng đêm",
                        Properties.Resources.WitchIcon)
                },
                {
                    "DÂN LÀNG",
                    new RoleDescription(
                        "Không có khả năng đặc biệt, ban ngày bầu chọn để treo cổ nghi phạm." + Environment.NewLine + Environment.NewLine +
                        "• Chiến thắng khi loại bỏ hết Sói" + Environment.NewLine +
                        "• Cần phân tích tốt để tìm ra Sói" + Environment.NewLine +
                        "• Phải tham gia thảo luận cùng dân làng",
                        Properties.Resources.villagerIcon)
                }
            };
        }

        private void SetupUI()
        {
            flowRoles.SuspendLayout();
            flowRoles.Controls.Clear();

            flowRoles.Visible = true;
            flowRoles.Width = 250; // Đặt width đủ lớn
            flowRoles.AutoScroll = true;

            // Thứ tự các role muốn hiển thị
            var roleOrder = new List<string> { "MA SÓI", "TIÊN TRI", "BẢO VỆ", "THỢ SĂN", "PHÙ THỦY", "DÂN LÀNG", "MA SÓI" };

            foreach (var roleName in roleOrder)
            {
                if (roles.TryGetValue(roleName, out var role))
                {
                    var btn = new Button
                    {
                        Text = roleName,
                        Tag = roleName,
                        Size = new Size(230, 60), // Tăng width để chứa đủ text
                        Margin = new Padding(5),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = defaultBtnColor,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 11, FontStyle.Regular),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Image = role.Image != null ? ResizeImage(role.Image, 30, 30) : null,
                        ImageAlign = ContentAlignment.MiddleLeft,
                        TextImageRelation = TextImageRelation.ImageBeforeText,
                        FlatAppearance = {
                    BorderSize = 0,
                    MouseOverBackColor = hoverBtnColor,
                    MouseDownBackColor = selectedBtnColor
                }
                    };

                    // Tạo góc bo tròn
                    btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 15, 15));

                    btn.Click += (s, e) => {
                        SelectRoleButton(btn);
                        ShowRole(roleName);
                    };

                    flowRoles.Controls.Add(btn);
                    Debug.WriteLine($"Đã thêm nút: {roleName}");
                }
                else
                {
                    Debug.WriteLine($"Không tìm thấy role: {roleName}");
                }
            }

            // Chọn MA SÓI đầu tiên
            var firstBtn = flowRoles.Controls.OfType<Button>().FirstOrDefault();
            if (firstBtn != null)
            {
                SelectRoleButton(firstBtn);
                ShowRole(firstBtn.Tag.ToString());
            }

            flowRoles.ResumeLayout(true);
            flowRoles.PerformLayout(); 
        }

        private Button CreateRoleButton(string roleName)
        {
            if (!roles.TryGetValue(roleName, out var role))
            {
                Debug.WriteLine($"Không tìm thấy thông tin cho vai trò: {roleName}");
                return null;
            }

            var btn = new Button
            {
                Text = roleName,
                Tag = roleName,
                Size = new Size(200, 60),
                Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat,
                BackColor = defaultBtnColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11F),
                TextAlign = ContentAlignment.MiddleLeft,
                ImageAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                FlatAppearance = {
                    BorderSize = 0,
                    MouseOverBackColor = hoverBtnColor,
                    MouseDownBackColor = Color.FromArgb(100, 100, 140)
                }
            };

            // Xử lý hình ảnh
            if (role.Image != null)
            {
                btn.Image = ResizeImage(role.Image, 30, 30);
                Debug.WriteLine($"Đã thêm hình ảnh cho {roleName}");
            }
            else
            {
                Debug.WriteLine($"Không có hình ảnh cho {roleName}");
            }

            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 10, 10));
            btn.Click += RoleButton_Click;

            btn.MouseEnter += (s, e) =>
            {
                if (btn != currentSelectedBtn)
                {
                    btn.BackColor = hoverBtnColor;
                }
                btn.Cursor = Cursors.Hand;
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn != currentSelectedBtn)
                {
                    btn.BackColor = defaultBtnColor;
                }
            };

            return btn;
        }

        private void RoleButton_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            SelectRoleButton(btn);
            ShowRole(btn.Tag.ToString());
        }

        private void SelectRoleButton(Button btn)
        {
            if (currentSelectedBtn != null)
            {
                currentSelectedBtn.BackColor = defaultBtnColor;
                currentSelectedBtn.Font = new Font(currentSelectedBtn.Font, FontStyle.Regular);
            }

            btn.BackColor = selectedBtnColor;
            btn.Font = new Font(btn.Font, FontStyle.Bold);
            currentSelectedBtn = btn;
        }

        private void ShowRole(string roleName)
        {
            if (roles.TryGetValue(roleName, out var role))
            {
                lblRoleName.Text = roleName;
                txtDescription.Text = role.Description;
                picRole.Image = role.Image ?? Properties.Resources.villagerIcon;
                Debug.WriteLine($"Đang hiển thị nội dung cho: {roleName}");
            }
            else
            {
                Debug.WriteLine($"Không tìm thấy nội dung cho: {roleName}");
            }
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            if (image == null) return null;

            var destImage = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return destImage;
        }

        private void ApplyModernStyling()
        {
            // Form styling
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.Padding = new Padding(10);

            // Close button
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = Color.FromArgb(220, 80, 80);
            btnClose.ForeColor = Color.White;
            btnClose.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnClose.Width, btnClose.Height, 15, 15));
            btnClose.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // Description box
            txtDescription.Multiline = true;
            txtDescription.WordWrap = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.BorderStyle = BorderStyle.None;
            txtDescription.BackColor = Color.White;
            txtDescription.Font = new Font("Segoe UI", 11.5F);
            txtDescription.ForeColor = Color.FromArgb(80, 80, 80);
            txtDescription.Margin = new Padding(10);
            txtDescription.Height = 250;

            // Role image
            picRole.SizeMode = PictureBoxSizeMode.Zoom;
            picRole.BackColor = Color.Transparent;
            picRole.Margin = new Padding(0, 20, 0, 20);

            // Role name label
            lblRoleName.Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold);
            lblRoleName.ForeColor = Color.FromArgb(70, 70, 100);
            lblRoleName.Margin = new Padding(0, 10, 0, 20);

            // Panels
            pnlSidebar.BackColor = Color.FromArgb(50, 50, 80);
            pnlContent.BackColor = Color.White;
            pnlContent.Padding = new Padding(20);
        }

        private void RolesForm_Load(object sender, EventArgs e)
        {
            this.Opacity = 0;
            var fadeIn = new Timer { Interval = 20 };
            fadeIn.Tick += (s, args) =>
            {
                if (this.Opacity >= 1)
                    fadeIn.Stop();
                else
                    this.Opacity += 0.05;
            };
            fadeIn.Start();
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();
    }

    public class RoleDescription
    {
        public string Description { get; }
        public Image Image { get; }

        public RoleDescription(string description, Image image)
        {
            Description = description;
            Image = image;
        }
    }
}