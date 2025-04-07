namespace WerewolfClient.Forms
{
    partial class RoomLobbyForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnFind = new System.Windows.Forms.Button();
            this.btnReturn = new System.Windows.Forms.Button();
            this.gameName = new System.Windows.Forms.Label();
            this.btnCreate = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(324, 164);
            this.panel1.TabIndex = 8;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::WerewolfClient.Properties.Resources._1329565;
            this.pictureBox2.Location = new System.Drawing.Point(156, 34);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(143, 110);
            this.pictureBox2.TabIndex = 3;
            this.pictureBox2.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Georgia", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(68, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(198, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Player Information";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.button1.Font = new System.Drawing.Font("Georgia", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(14, 74);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Friends";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Georgia", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(11, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "Player Name";
            // 
            // btnFind
            // 
            this.btnFind.BackColor = System.Drawing.Color.Transparent;
            this.btnFind.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFind.FlatAppearance.BorderColor = System.Drawing.Color.LightCoral;
            this.btnFind.FlatAppearance.BorderSize = 2;
            this.btnFind.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
            this.btnFind.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnFind.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFind.Font = new System.Drawing.Font("Georgia", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFind.ForeColor = System.Drawing.Color.DarkRed;
            this.btnFind.Location = new System.Drawing.Point(1171, 405);
            this.btnFind.Margin = new System.Windows.Forms.Padding(2);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(240, 61);
            this.btnFind.TabIndex = 12;
            this.btnFind.Text = "FIND";
            this.btnFind.UseVisualStyleBackColor = false;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // btnReturn
            // 
            this.btnReturn.BackColor = System.Drawing.Color.Transparent;
            this.btnReturn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReturn.FlatAppearance.BorderColor = System.Drawing.Color.LightCoral;
            this.btnReturn.FlatAppearance.BorderSize = 2;
            this.btnReturn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
            this.btnReturn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnReturn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReturn.Font = new System.Drawing.Font("Georgia", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReturn.ForeColor = System.Drawing.Color.DarkRed;
            this.btnReturn.Location = new System.Drawing.Point(1171, 506);
            this.btnReturn.Margin = new System.Windows.Forms.Padding(2);
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.Size = new System.Drawing.Size(240, 61);
            this.btnReturn.TabIndex = 11;
            this.btnReturn.Text = "RETURN";
            this.btnReturn.UseVisualStyleBackColor = false;
            this.btnReturn.Click += new System.EventHandler(this.btnReturn_Click);
            // 
            // gameName
            // 
            this.gameName.AutoSize = true;
            this.gameName.BackColor = System.Drawing.Color.Transparent;
            this.gameName.Font = new System.Drawing.Font("Palatino Linotype", 42F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameName.ForeColor = System.Drawing.Color.Maroon;
            this.gameName.Location = new System.Drawing.Point(1113, 182);
            this.gameName.Name = "gameName";
            this.gameName.Size = new System.Drawing.Size(342, 76);
            this.gameName.TabIndex = 10;
            this.gameName.Text = "Werewolves";
            // 
            // btnCreate
            // 
            this.btnCreate.BackColor = System.Drawing.Color.Transparent;
            this.btnCreate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreate.FlatAppearance.BorderColor = System.Drawing.Color.LightCoral;
            this.btnCreate.FlatAppearance.BorderSize = 2;
            this.btnCreate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
            this.btnCreate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreate.Font = new System.Drawing.Font("Georgia", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCreate.ForeColor = System.Drawing.Color.DarkRed;
            this.btnCreate.Location = new System.Drawing.Point(1171, 308);
            this.btnCreate.Margin = new System.Windows.Forms.Padding(2);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(240, 61);
            this.btnCreate.TabIndex = 9;
            this.btnCreate.Text = "CREATE";
            this.btnCreate.UseVisualStyleBackColor = false;
            // 
            // RoomLobbyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::WerewolfClient.Properties.Resources.z6474793253367_db44b1af33961683c4d8fbe139cb3aac;
            this.ClientSize = new System.Drawing.Size(1520, 941);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.btnReturn);
            this.Controls.Add(this.gameName);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.panel1);
            this.Name = "RoomLobbyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RoomLobbyForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.Button btnReturn;
        private System.Windows.Forms.Label gameName;
        private System.Windows.Forms.Button btnCreate;
    }
}