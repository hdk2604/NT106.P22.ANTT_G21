using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WerewolfClient.Forms
{
    public partial class InGameForm : Form
    {
       

        public InGameForm()
        {
            InitializeComponent();
            Load += Form1_Load;
            
        }

        private string placeholderText = "Nhập tin nhắn của bạn";
        private string player = "Player 1: ";
        private void Form1_Load(object sender, EventArgs e)
        {
            panelTopLeft.BackColor = Color.FromArgb(100, 0, 0, 0);
            panelRole.BackColor = Color.FromArgb(100, 0, 0, 0);
            SetPlaceholder();
            this.richTextBox2.Enter += new System.EventHandler(this.richTextBox2_Enter);
            this.richTextBox2.Leave += new System.EventHandler(this.richTextBox2_Leave);
            AddPanelsToTable();
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
                richTextBox1.AppendText(player + richTextBox2.Text + Environment.NewLine);
                richTextBox2.Clear();
                SetPlaceholder();  // Đặt lại placeholder sau khi gửi
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void AddPanelsToTable()
        {
            // Xóa các control có sẵn để tránh xung đột
            tableLayoutPanel1.Controls.Clear();
            Image originalImage = Properties.Resources.UserIcon;

            // Xác định kích thước mong muốn
            int newWidth = 70;  // thay đổi theo ý bạn
            int newHeight = 70; // thay đổi theo ý bạn

            // Tạo Bitmap mới với kích thước mong muốn
            Image resizedImage = new Bitmap(originalImage, new Size(newWidth, newHeight));
            // Nếu TableLayoutPanel được thiết kế sẵn với 16 ô,
            // ta không cần đặt lại RowCount hay ColumnCount, trừ khi cần thay đổi.
            int playerIndex = 1;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Panel p = new Panel();
                    p.Dock = DockStyle.Fill;
                    p.BackColor = Color.FromArgb(30, 0, 0, 0);
                    p.Margin = new Padding(2);
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.Name = $"panel_{row}_{col}";

                    // Tạo TableLayoutPanel nội bộ với 2 hàng
                    TableLayoutPanel innerTLP = new TableLayoutPanel();
                    innerTLP.Dock = DockStyle.Fill;
                    innerTLP.RowCount = 2;
                    innerTLP.ColumnCount = 1;
                    innerTLP.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));      // Hàng đầu tiên chứa Label, chiều cao cố định 30px (điều chỉnh nếu cần)
                    innerTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 100));       // Hàng thứ hai chứa PictureBox, chiếm phần còn lại của Panel
                    p.Controls.Add(innerTLP);

                    // Tạo Label cho Player (nằm trên cùng)
                    Label nameLabel = new Label();
                    nameLabel.Text = $"Player {playerIndex++}";
                    nameLabel.ForeColor = Color.White;
                    nameLabel.Dock = DockStyle.Fill;
                    nameLabel.TextAlign = ContentAlignment.MiddleCenter;
                    innerTLP.Controls.Add(nameLabel, 0, 0);

                    // Tạo PictureBox hiển thị hình user (nằm ở dưới, không chạm vào Label)
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Image = Properties.Resources.UserIcon;  // Đảm bảo UserImage đã được thêm vào Resources
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.Dock = DockStyle.Fill;
                    innerTLP.Controls.Add(pictureBox, 0, 1);
                    tableLayoutPanel1.Controls.Add(p, col, row);
                }
            }
        }

    }




    // Xử lý sự kiện Click của button

}

