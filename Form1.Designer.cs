namespace Shoot_Scoot
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.status1 = new System.Windows.Forms.Label();
            this.status2 = new System.Windows.Forms.Label();
            this.shortcut = new System.Windows.Forms.Label();
            this.vishok = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Shoot_Scoot.Properties.Resources.ss;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(300, 300);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // status1
            // 
            this.status1.AutoSize = true;
            this.status1.BackColor = System.Drawing.Color.White;
            this.status1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.status1.Location = new System.Drawing.Point(29, 249);
            this.status1.Name = "status1";
            this.status1.Size = new System.Drawing.Size(154, 20);
            this.status1.TabIndex = 1;
            this.status1.Text = "Screenshot mode is:";
            // 
            // status2
            // 
            this.status2.AutoSize = true;
            this.status2.BackColor = System.Drawing.Color.White;
            this.status2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.status2.ForeColor = System.Drawing.Color.Green;
            this.status2.Location = new System.Drawing.Point(186, 249);
            this.status2.Name = "status2";
            this.status2.Size = new System.Drawing.Size(73, 20);
            this.status2.TabIndex = 2;
            this.status2.Text = "ACTIVE";
            // 
            // shortcut
            // 
            this.shortcut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.shortcut.AutoSize = true;
            this.shortcut.BackColor = System.Drawing.Color.White;
            this.shortcut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.shortcut.Location = new System.Drawing.Point(3, 10);
            this.shortcut.Name = "shortcut";
            this.shortcut.Size = new System.Drawing.Size(294, 20);
            this.shortcut.TabIndex = 4;
            this.shortcut.Text = "Press ALT + INSERT to take screenshot";
            // 
            // vishok
            // 
            this.vishok.AutoSize = true;
            this.vishok.BackColor = System.Drawing.Color.White;
            this.vishok.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vishok.Location = new System.Drawing.Point(46, 282);
            this.vishok.Name = "vishok";
            this.vishok.Size = new System.Drawing.Size(209, 16);
            this.vishok.TabIndex = 6;
            this.vishok.Text = "Developed by Vishok Manikantan";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 300);
            this.Controls.Add(this.vishok);
            this.Controls.Add(this.shortcut);
            this.Controls.Add(this.status2);
            this.Controls.Add(this.status1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_LoadAsync);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label status1;
        private System.Windows.Forms.Label status2;
        private System.Windows.Forms.Label shortcut;
        private System.Windows.Forms.Label vishok;
    }
}

