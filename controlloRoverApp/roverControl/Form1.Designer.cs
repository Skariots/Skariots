namespace roverControl
{
    partial class ROVER
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.portBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.commandTimer = new System.Windows.Forms.Timer(this.components);
            this.forwardButton = new System.Windows.Forms.Button();
            this.rightButton = new System.Windows.Forms.Button();
            this.backwardsButton = new System.Windows.Forms.Button();
            this.leftButton = new System.Windows.Forms.Button();
            this.comLabel = new System.Windows.Forms.Label();
            this.speedLabel = new System.Windows.Forms.Label();
            this.speedBox = new System.Windows.Forms.TextBox();
            this.goBackButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.gyroPort = new System.IO.Ports.SerialPort(this.components);
            this.orientationBox = new System.Windows.Forms.TextBox();
            this.orientationLabel = new System.Windows.Forms.Label();
            this.gyroTimer = new System.Windows.Forms.Timer(this.components);
            this.rotateRightButton = new System.Windows.Forms.Button();
            this.rotateLeftButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // serialPort
            // 
            this.serialPort.BaudRate = 115200;
            // 
            // portBox
            // 
            this.portBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.portBox.Location = new System.Drawing.Point(852, 67);
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(100, 22);
            this.portBox.TabIndex = 0;
            this.portBox.Text = "0";
            this.portBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // connectButton
            // 
            this.connectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connectButton.Location = new System.Drawing.Point(790, 12);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(162, 48);
            this.connectButton.TabIndex = 1;
            this.connectButton.Text = "CONNECT";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // commandTimer
            // 
            this.commandTimer.Interval = 30;
            this.commandTimer.Tick += new System.EventHandler(this.commandTimer_Tick);
            // 
            // forwardButton
            // 
            this.forwardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.forwardButton.BackColor = System.Drawing.SystemColors.Control;
            this.forwardButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.forwardButton.Location = new System.Drawing.Point(127, 241);
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(50, 75);
            this.forwardButton.TabIndex = 2;
            this.forwardButton.Text = "↑";
            this.forwardButton.UseVisualStyleBackColor = false;
            // 
            // rightButton
            // 
            this.rightButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rightButton.BackColor = System.Drawing.SystemColors.Control;
            this.rightButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rightButton.Location = new System.Drawing.Point(177, 322);
            this.rightButton.Name = "rightButton";
            this.rightButton.Size = new System.Drawing.Size(75, 50);
            this.rightButton.TabIndex = 3;
            this.rightButton.Text = "→";
            this.rightButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.rightButton.UseVisualStyleBackColor = false;
            // 
            // backwardsButton
            // 
            this.backwardsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.backwardsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backwardsButton.Location = new System.Drawing.Point(127, 376);
            this.backwardsButton.Name = "backwardsButton";
            this.backwardsButton.Size = new System.Drawing.Size(50, 75);
            this.backwardsButton.TabIndex = 4;
            this.backwardsButton.Text = "↓";
            this.backwardsButton.UseVisualStyleBackColor = true;
            // 
            // leftButton
            // 
            this.leftButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.leftButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.leftButton.Location = new System.Drawing.Point(51, 322);
            this.leftButton.Name = "leftButton";
            this.leftButton.Size = new System.Drawing.Size(75, 50);
            this.leftButton.TabIndex = 5;
            this.leftButton.Text = "←";
            this.leftButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.leftButton.UseVisualStyleBackColor = true;
            // 
            // comLabel
            // 
            this.comLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comLabel.AutoSize = true;
            this.comLabel.Location = new System.Drawing.Point(860, 70);
            this.comLabel.Name = "comLabel";
            this.comLabel.Size = new System.Drawing.Size(37, 16);
            this.comLabel.TabIndex = 6;
            this.comLabel.Text = "COM";
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Location = new System.Drawing.Point(12, 123);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(48, 16);
            this.speedLabel.TabIndex = 7;
            this.speedLabel.Text = "Speed";
            // 
            // speedBox
            // 
            this.speedBox.Location = new System.Drawing.Point(66, 123);
            this.speedBox.Name = "speedBox";
            this.speedBox.ReadOnly = true;
            this.speedBox.Size = new System.Drawing.Size(49, 22);
            this.speedBox.TabIndex = 8;
            this.speedBox.Text = "1";
            this.speedBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // goBackButton
            // 
            this.goBackButton.Location = new System.Drawing.Point(200, 190);
            this.goBackButton.Name = "goBackButton";
            this.goBackButton.Size = new System.Drawing.Size(100, 34);
            this.goBackButton.TabIndex = 9;
            this.goBackButton.Text = "GO BACK";
            this.goBackButton.UseVisualStyleBackColor = true;
            this.goBackButton.Click += new System.EventHandler(this.goBackButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(306, 190);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(100, 33);
            this.resetButton.TabIndex = 10;
            this.resetButton.Text = "RESET POS";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // gyroPort
            // 
            this.gyroPort.BaudRate = 115200;
            this.gyroPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.gyroPort_DataReceived);
            // 
            // orientationBox
            // 
            this.orientationBox.Location = new System.Drawing.Point(89, 79);
            this.orientationBox.Name = "orientationBox";
            this.orientationBox.ReadOnly = true;
            this.orientationBox.Size = new System.Drawing.Size(90, 22);
            this.orientationBox.TabIndex = 11;
            this.orientationBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // orientationLabel
            // 
            this.orientationLabel.AutoSize = true;
            this.orientationLabel.Location = new System.Drawing.Point(12, 85);
            this.orientationLabel.Name = "orientationLabel";
            this.orientationLabel.Size = new System.Drawing.Size(71, 16);
            this.orientationLabel.TabIndex = 12;
            this.orientationLabel.Text = "Orientation";
            // 
            // gyroTimer
            // 
            this.gyroTimer.Interval = 500;
            this.gyroTimer.Tick += new System.EventHandler(this.gyroTimer_Tick);
            // 
            // rotateRightButton
            // 
            this.rotateRightButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rotateRightButton.Location = new System.Drawing.Point(183, 260);
            this.rotateRightButton.Name = "rotateRightButton";
            this.rotateRightButton.Size = new System.Drawing.Size(50, 50);
            this.rotateRightButton.TabIndex = 13;
            this.rotateRightButton.Text = "↷";
            this.rotateRightButton.UseVisualStyleBackColor = true;
            // 
            // rotateLeftButton
            // 
            this.rotateLeftButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rotateLeftButton.Location = new System.Drawing.Point(71, 260);
            this.rotateLeftButton.Name = "rotateLeftButton";
            this.rotateLeftButton.Size = new System.Drawing.Size(50, 50);
            this.rotateLeftButton.TabIndex = 14;
            this.rotateLeftButton.Text = "↶";
            this.rotateLeftButton.UseVisualStyleBackColor = true;
            // 
            // ROVER
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 570);
            this.Controls.Add(this.rotateLeftButton);
            this.Controls.Add(this.rotateRightButton);
            this.Controls.Add(this.orientationLabel);
            this.Controls.Add(this.orientationBox);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.goBackButton);
            this.Controls.Add(this.speedBox);
            this.Controls.Add(this.speedLabel);
            this.Controls.Add(this.comLabel);
            this.Controls.Add(this.leftButton);
            this.Controls.Add(this.backwardsButton);
            this.Controls.Add(this.rightButton);
            this.Controls.Add(this.forwardButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.portBox);
            this.KeyPreview = true;
            this.Name = "ROVER";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ROVER";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ROVER_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ROVER_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort serialPort;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Timer commandTimer;
        private System.Windows.Forms.Button forwardButton;
        private System.Windows.Forms.Button rightButton;
        private System.Windows.Forms.Button backwardsButton;
        private System.Windows.Forms.Button leftButton;
        private System.Windows.Forms.Label comLabel;
        private System.Windows.Forms.Label speedLabel;
        private System.Windows.Forms.TextBox speedBox;
        private System.Windows.Forms.Button goBackButton;
        private System.Windows.Forms.Button resetButton;
        private System.IO.Ports.SerialPort gyroPort;
        private System.Windows.Forms.TextBox orientationBox;
        private System.Windows.Forms.Label orientationLabel;
        private System.Windows.Forms.Timer gyroTimer;
        private System.Windows.Forms.Button rotateRightButton;
        private System.Windows.Forms.Button rotateLeftButton;
    }
}

