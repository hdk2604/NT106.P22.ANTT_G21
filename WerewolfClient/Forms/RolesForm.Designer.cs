namespace WerewolfClient.Forms
{
    partial class RolesForm
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
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.flowRoles = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.pnlSeparator = new System.Windows.Forms.Panel();
            this.lblRoleName = new System.Windows.Forms.Label();
            this.picRole = new System.Windows.Forms.PictureBox();
            this.pnlSidebar.SuspendLayout();
            this.pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picRole)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(70)))));
            this.pnlSidebar.Controls.Add(this.lblTitle);
            this.pnlSidebar.Controls.Add(this.flowRoles);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.pnlSidebar.Size = new System.Drawing.Size(250, 700);
            this.pnlSidebar.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(60)))));
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.lblTitle.Size = new System.Drawing.Size(245, 90);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "DANH SÁCH VAI TRÒ";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowRoles
            // 
            this.flowRoles.AutoScroll = true;
            this.flowRoles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(70)))));
            this.flowRoles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowRoles.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowRoles.Location = new System.Drawing.Point(0, 90);
            this.flowRoles.Name = "flowRoles";
            this.flowRoles.Padding = new System.Windows.Forms.Padding(15, 15, 15, 25);
            this.flowRoles.Size = new System.Drawing.Size(245, 610);
            this.flowRoles.TabIndex = 1;
            // 
            // pnlContent
            // 
            this.pnlContent.BackColor = System.Drawing.Color.White;
            this.pnlContent.Controls.Add(this.btnClose);
            this.pnlContent.Controls.Add(this.txtDescription);
            this.pnlContent.Controls.Add(this.pnlSeparator);
            this.pnlContent.Controls.Add(this.lblRoleName);
            this.pnlContent.Controls.Add(this.picRole);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(250, 0);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(40, 30, 40, 30);
            this.pnlContent.Size = new System.Drawing.Size(750, 700);
            this.pnlContent.TabIndex = 1;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(620, 630);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 45);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "ĐÓNG";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.Color.White;
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 11.5F);
            this.txtDescription.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.txtDescription.Location = new System.Drawing.Point(40, 131);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(670, 539);
            this.txtDescription.TabIndex = 3;
            // 
            // pnlSeparator
            // 
            this.pnlSeparator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.pnlSeparator.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSeparator.Location = new System.Drawing.Point(40, 130);
            this.pnlSeparator.Margin = new System.Windows.Forms.Padding(0, 10, 0, 30);
            this.pnlSeparator.Name = "pnlSeparator";
            this.pnlSeparator.Size = new System.Drawing.Size(670, 1);
            this.pnlSeparator.TabIndex = 2;
            // 
            // lblRoleName
            // 
            this.lblRoleName.AutoSize = true;
            this.lblRoleName.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRoleName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(100)))));
            this.lblRoleName.Location = new System.Drawing.Point(140, 45);
            this.lblRoleName.Name = "lblRoleName";
            this.lblRoleName.Size = new System.Drawing.Size(215, 65);
            this.lblRoleName.TabIndex = 1;
            this.lblRoleName.Text = "VAI TRÒ";
            // 
            // picRole
            // 
            this.picRole.BackColor = System.Drawing.Color.Transparent;
            this.picRole.Location = new System.Drawing.Point(40, 30);
            this.picRole.Name = "picRole";
            this.picRole.Size = new System.Drawing.Size(90, 90);
            this.picRole.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picRole.TabIndex = 0;
            this.picRole.TabStop = false;
            // 
            // RolesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlSidebar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "RolesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LUẬT CHƠI MA SÓI";
            this.pnlSidebar.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picRole)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel flowRoles;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.PictureBox picRole;
        private System.Windows.Forms.Label lblRoleName;
        private System.Windows.Forms.Panel pnlSeparator;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button btnClose;
    }
}