namespace EclipseAutomation
{
    partial class MainForm
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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblNextStep = new System.Windows.Forms.Label();
            this.lblCountdown = new System.Windows.Forms.Label();
            this.pbLiveView = new System.Windows.Forms.PictureBox();
            this.lblClock = new System.Windows.Forms.Label();
            this.testsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkCameraSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.takeAnnualStackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.takeSecondContactStackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.takeTotalityStackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.takeSinglePhotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLiveView)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.testsToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(12, 4, 0, 4);
            this.menuStrip.Size = new System.Drawing.Size(1008, 42);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(56, 34);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 34);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // lblNextStep
            // 
            this.lblNextStep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNextStep.Location = new System.Drawing.Point(12, 42);
            this.lblNextStep.Name = "lblNextStep";
            this.lblNextStep.Size = new System.Drawing.Size(752, 85);
            this.lblNextStep.TabIndex = 1;
            this.lblNextStep.Text = "C2 coming soon, prepare to remove the camera filter.\r\nSome more text to test wrap" +
    "ping behavior.\r\nLine 3g";
            this.lblNextStep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCountdown
            // 
            this.lblCountdown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCountdown.BackColor = System.Drawing.Color.Orange;
            this.lblCountdown.Font = new System.Drawing.Font("Consolas", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCountdown.Location = new System.Drawing.Point(763, 48);
            this.lblCountdown.Name = "lblCountdown";
            this.lblCountdown.Size = new System.Drawing.Size(233, 64);
            this.lblCountdown.TabIndex = 2;
            this.lblCountdown.Text = "12:12:12";
            this.lblCountdown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbLiveView
            // 
            this.pbLiveView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbLiveView.Location = new System.Drawing.Point(12, 130);
            this.pbLiveView.Name = "pbLiveView";
            this.pbLiveView.Size = new System.Drawing.Size(984, 587);
            this.pbLiveView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLiveView.TabIndex = 3;
            this.pbLiveView.TabStop = false;
            // 
            // lblClock
            // 
            this.lblClock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClock.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClock.Location = new System.Drawing.Point(754, 0);
            this.lblClock.Name = "lblClock";
            this.lblClock.Size = new System.Drawing.Size(254, 42);
            this.lblClock.TabIndex = 5;
            this.lblClock.Text = "yyyy-mm-dd hh:mm:ss";
            this.lblClock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // testsToolStripMenuItem
            // 
            this.testsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkCameraSettingsToolStripMenuItem,
            this.takeAnnualStackToolStripMenuItem,
            this.takeSecondContactStackToolStripMenuItem,
            this.takeTotalityStackToolStripMenuItem,
            this.takeSinglePhotoToolStripMenuItem});
            this.testsToolStripMenuItem.Name = "testsToolStripMenuItem";
            this.testsToolStripMenuItem.Size = new System.Drawing.Size(70, 34);
            this.testsToolStripMenuItem.Text = "&Tests";
            // 
            // checkCameraSettingsToolStripMenuItem
            // 
            this.checkCameraSettingsToolStripMenuItem.Name = "checkCameraSettingsToolStripMenuItem";
            this.checkCameraSettingsToolStripMenuItem.Size = new System.Drawing.Size(333, 34);
            this.checkCameraSettingsToolStripMenuItem.Text = "&Check Camera Settings";
            // 
            // takeAnnualStackToolStripMenuItem
            // 
            this.takeAnnualStackToolStripMenuItem.Name = "takeAnnualStackToolStripMenuItem";
            this.takeAnnualStackToolStripMenuItem.Size = new System.Drawing.Size(333, 34);
            this.takeAnnualStackToolStripMenuItem.Text = "Take &Annual Stack";
            // 
            // takeSecondContactStackToolStripMenuItem
            // 
            this.takeSecondContactStackToolStripMenuItem.Name = "takeSecondContactStackToolStripMenuItem";
            this.takeSecondContactStackToolStripMenuItem.Size = new System.Drawing.Size(333, 34);
            this.takeSecondContactStackToolStripMenuItem.Text = "Take &Second Contact Stack";
            // 
            // takeTotalityStackToolStripMenuItem
            // 
            this.takeTotalityStackToolStripMenuItem.Name = "takeTotalityStackToolStripMenuItem";
            this.takeTotalityStackToolStripMenuItem.Size = new System.Drawing.Size(333, 34);
            this.takeTotalityStackToolStripMenuItem.Text = "Take &Totality Stack";
            // 
            // takeSinglePhotoToolStripMenuItem
            // 
            this.takeSinglePhotoToolStripMenuItem.Name = "takeSinglePhotoToolStripMenuItem";
            this.takeSinglePhotoToolStripMenuItem.Size = new System.Drawing.Size(333, 34);
            this.takeSinglePhotoToolStripMenuItem.Text = "Take Single &Photo";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.lblClock);
            this.Controls.Add(this.pbLiveView);
            this.Controls.Add(this.lblCountdown);
            this.Controls.Add(this.lblNextStep);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "MainForm";
            this.Text = "Eclipse Automation";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLiveView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.Label lblNextStep;
        private System.Windows.Forms.Label lblCountdown;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.PictureBox pbLiveView;
        private System.Windows.Forms.Label lblClock;
        private System.Windows.Forms.ToolStripMenuItem testsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkCameraSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem takeAnnualStackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem takeSecondContactStackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem takeTotalityStackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem takeSinglePhotoToolStripMenuItem;
    }
}

