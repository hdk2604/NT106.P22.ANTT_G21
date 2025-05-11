using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WerewolfClient.Forms
{
    partial class InGameForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InGameForm));
            this.panelLeft = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.panelRole = new System.Windows.Forms.Panel();
            this.pictureWitch = new System.Windows.Forms.PictureBox();
            this.pictureProphet = new System.Windows.Forms.PictureBox();
            this.pictureSheriff = new System.Windows.Forms.PictureBox();
            this.pictureVillager = new System.Windows.Forms.PictureBox();
            this.pictureWerewolf = new System.Windows.Forms.PictureBox();
            this.panelTopLeft = new System.Windows.Forms.Panel();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelTimer = new System.Windows.Forms.Label();
            this.panelLeft.SuspendLayout();
            this.panelRole.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureWitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureProphet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureSheriff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureVillager)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureWerewolf)).BeginInit();
            this.panelTopLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.BackColor = System.Drawing.Color.Transparent;
            this.panelLeft.Controls.Add(this.button1);
            this.panelLeft.Controls.Add(this.richTextBox2);
            this.panelLeft.Controls.Add(this.richTextBox1);
            this.panelLeft.Controls.Add(this.panelRole);
            this.panelLeft.Controls.Add(this.panelTopLeft);
            this.panelLeft.ForeColor = System.Drawing.Color.Black;
            this.panelLeft.Location = new System.Drawing.Point(-1, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(500, 980);
            this.panelLeft.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Black;
            this.button1.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(448, 748);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(52, 51);
            this.button1.TabIndex = 4;
            this.button1.Text = "Gửi";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox2
            // 
            this.richTextBox2.BackColor = System.Drawing.Color.Black;
            this.richTextBox2.Font = new System.Drawing.Font("Palatino Linotype", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.richTextBox2.ForeColor = System.Drawing.Color.White;
            this.richTextBox2.Location = new System.Drawing.Point(3, 748);
            this.richTextBox2.Multiline = false;
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox2.Size = new System.Drawing.Size(446, 51);
            this.richTextBox2.TabIndex = 3;
            this.richTextBox2.Text = "";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.Black;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Font = new System.Drawing.Font("Palatino Linotype", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.richTextBox1.ForeColor = System.Drawing.Color.Red;
            this.richTextBox1.Location = new System.Drawing.Point(3, 200);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(497, 547);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // panelRole
            // 
            this.panelRole.BackColor = System.Drawing.Color.White;
            this.panelRole.Controls.Add(this.pictureWitch);
            this.panelRole.Controls.Add(this.pictureProphet);
            this.panelRole.Controls.Add(this.pictureSheriff);
            this.panelRole.Controls.Add(this.pictureVillager);
            this.panelRole.Controls.Add(this.pictureWerewolf);
            this.panelRole.Location = new System.Drawing.Point(3, 76);
            this.panelRole.Name = "panelRole";
            this.panelRole.Size = new System.Drawing.Size(500, 100);
            this.panelRole.TabIndex = 1;
            // 
            // pictureWitch
            // 
            this.pictureWitch.BackColor = System.Drawing.Color.Transparent;
            this.pictureWitch.Image = global::WerewolfClient.Properties.Resources.WitchIcon;
            this.pictureWitch.Location = new System.Drawing.Point(342, 20);
            this.pictureWitch.Name = "pictureWitch";
            this.pictureWitch.Size = new System.Drawing.Size(60, 60);
            this.pictureWitch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureWitch.TabIndex = 1;
            this.pictureWitch.TabStop = false;
            // 
            // pictureProphet
            // 
            this.pictureProphet.BackColor = System.Drawing.Color.Transparent;
            this.pictureProphet.Image = global::WerewolfClient.Properties.Resources.ProphetIcon;
            this.pictureProphet.Location = new System.Drawing.Point(258, 20);
            this.pictureProphet.Name = "pictureProphet";
            this.pictureProphet.Size = new System.Drawing.Size(60, 60);
            this.pictureProphet.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureProphet.TabIndex = 3;
            this.pictureProphet.TabStop = false;
            // 
            // pictureSheriff
            // 
            this.pictureSheriff.BackColor = System.Drawing.Color.Transparent;
            this.pictureSheriff.Image = global::WerewolfClient.Properties.Resources.SheriffIcon;
            this.pictureSheriff.Location = new System.Drawing.Point(173, 20);
            this.pictureSheriff.Name = "pictureSheriff";
            this.pictureSheriff.Size = new System.Drawing.Size(60, 60);
            this.pictureSheriff.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureSheriff.TabIndex = 2;
            this.pictureSheriff.TabStop = false;
            // 
            // pictureVillager
            // 
            this.pictureVillager.BackColor = System.Drawing.Color.Transparent;
            this.pictureVillager.Image = global::WerewolfClient.Properties.Resources.villagerIcon;
            this.pictureVillager.Location = new System.Drawing.Point(90, 20);
            this.pictureVillager.Name = "pictureVillager";
            this.pictureVillager.Size = new System.Drawing.Size(60, 60);
            this.pictureVillager.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureVillager.TabIndex = 1;
            this.pictureVillager.TabStop = false;
            // 
            // pictureWerewolf
            // 
            this.pictureWerewolf.BackColor = System.Drawing.Color.Transparent;
            this.pictureWerewolf.Image = global::WerewolfClient.Properties.Resources.WereWolfIcon;
            this.pictureWerewolf.Location = new System.Drawing.Point(10, 20);
            this.pictureWerewolf.Name = "pictureWerewolf";
            this.pictureWerewolf.Size = new System.Drawing.Size(60, 60);
            this.pictureWerewolf.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureWerewolf.TabIndex = 0;
            this.pictureWerewolf.TabStop = false;
            this.pictureWerewolf.Click += new System.EventHandler(this.pictureWerewolf_Click);
            // 
            // panelTopLeft
            // 
            this.panelTopLeft.BackColor = System.Drawing.Color.White;
            this.panelTopLeft.Controls.Add(this.labelTimer);
            this.panelTopLeft.Controls.Add(this.buttonHelp);
            this.panelTopLeft.Controls.Add(this.buttonExit);
            this.panelTopLeft.Location = new System.Drawing.Point(3, 3);
            this.panelTopLeft.Name = "panelTopLeft";
            this.panelTopLeft.Size = new System.Drawing.Size(500, 55);
            this.panelTopLeft.TabIndex = 0;
            // 
            // buttonHelp
            // 
            this.buttonHelp.Location = new System.Drawing.Point(439, 9);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(36, 38);
            this.buttonHelp.TabIndex = 1;
            this.buttonHelp.Text = "?";
            this.buttonHelp.UseVisualStyleBackColor = true;
            // 
            // buttonExit
            // 
            this.buttonExit.BackColor = System.Drawing.Color.White;
            this.buttonExit.Location = new System.Drawing.Point(10, 9);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(35, 33);
            this.buttonExit.TabIndex = 0;
            this.buttonExit.Text = "X";
            this.buttonExit.UseVisualStyleBackColor = false;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(508, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(998, 787);
            this.tableLayoutPanel1.TabIndex = 1;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // labelTimer
            // 
            this.labelTimer.AutoSize = true;
            this.labelTimer.Location = new System.Drawing.Point(220, 17);
            this.labelTimer.Name = "labelTimer";
            this.labelTimer.Size = new System.Drawing.Size(44, 16);
            this.labelTimer.TabIndex = 2;
            this.labelTimer.Text = "label1";
            // 
            // InGameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1518, 933);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panelLeft);
            this.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.Name = "InGameForm";
            this.Text = "Ma Sói - Wolvesville Style UI";
            this.panelLeft.ResumeLayout(false);
            this.panelRole.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureWitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureProphet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureSheriff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureVillager)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureWerewolf)).EndInit();
            this.panelTopLeft.ResumeLayout(false);
            this.panelTopLeft.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion
        private System.Windows.Forms.Panel panelTopLeft;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Panel panelRole;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.PictureBox pictureWerewolf;
        private PictureBox pictureVillager;
        private PictureBox pictureSheriff;
        private PictureBox pictureProphet;
        private PictureBox pictureWitch;
        private System.Windows.Forms.Panel panelLeft;
        private RichTextBox richTextBox1;
        private RichTextBox richTextBox2;
        private System.Windows.Forms.Button button1;
        private TableLayoutPanel tableLayoutPanel1;
        private Label labelTimer;
    }
}

