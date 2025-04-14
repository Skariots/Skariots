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
            this.startTestButton = new System.Windows.Forms.Button();
            this.stopTestButton = new System.Windows.Forms.Button();
            this.timerCS = new System.Windows.Forms.Timer(this.components);
            this.directionBox = new System.Windows.Forms.TextBox();
            this.angularSpeedBox = new System.Windows.Forms.TextBox();
            this.directionLabel = new System.Windows.Forms.Label();
            this.angularSpeedLabel = new System.Windows.Forms.Label();
            this.impRigTextBox = new System.Windows.Forms.TextBox();
            this.impForwTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // serialPort
            // 
            this.serialPort.BaudRate = 115200;
            this.serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort_DataReceived);
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
            this.commandTimer.Interval = 50;
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
            this.resetButton.Text = "CALIBRATE";
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
            this.gyroTimer.Interval = 50;
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
            // startTestButton
            // 
            this.startTestButton.Location = new System.Drawing.Point(306, 281);
            this.startTestButton.Name = "startTestButton";
            this.startTestButton.Size = new System.Drawing.Size(152, 23);
            this.startTestButton.TabIndex = 15;
            this.startTestButton.Text = "START TEST";
            this.startTestButton.UseVisualStyleBackColor = true;
            this.startTestButton.Click += new System.EventHandler(this.startTestButton_Click);
            // 
            // stopTestButton
            // 
            this.stopTestButton.Location = new System.Drawing.Point(306, 322);
            this.stopTestButton.Name = "stopTestButton";
            this.stopTestButton.Size = new System.Drawing.Size(152, 23);
            this.stopTestButton.TabIndex = 16;
            this.stopTestButton.Text = "STOP TEST";
            this.stopTestButton.UseVisualStyleBackColor = true;
            this.stopTestButton.Click += new System.EventHandler(this.stopTestButton_Click);
            // 
            // timerCS
            // 
            this.timerCS.Enabled = true;
            this.timerCS.Interval = 50;
            this.timerCS.Tick += new System.EventHandler(this.timerCS_Tick);
            // 
            // directionBox
            // 
            this.directionBox.Location = new System.Drawing.Point(306, 376);
            this.directionBox.Name = "directionBox";
            this.directionBox.Size = new System.Drawing.Size(100, 22);
            this.directionBox.TabIndex = 17;
            this.directionBox.Text = "0";
            this.directionBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.directionBox.TextChanged += new System.EventHandler(this.directionBox_TextChanged);
            // 
            // angularSpeedBox
            // 
            this.angularSpeedBox.Location = new System.Drawing.Point(457, 375);
            this.angularSpeedBox.Name = "angularSpeedBox";
            this.angularSpeedBox.Size = new System.Drawing.Size(100, 22);
            this.angularSpeedBox.TabIndex = 18;
            this.angularSpeedBox.Text = "0.0";
            this.angularSpeedBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.angularSpeedBox.TextChanged += new System.EventHandler(this.angularSpeedBox_TextChanged);
            // 
            // directionLabel
            // 
            this.directionLabel.AutoSize = true;
            this.directionLabel.Location = new System.Drawing.Point(303, 400);
            this.directionLabel.Name = "directionLabel";
            this.directionLabel.Size = new System.Drawing.Size(107, 16);
            this.directionLabel.TabIndex = 19;
            this.directionLabel.Text = "degrees (0 - 360)";
            // 
            // angularSpeedLabel
            // 
            this.angularSpeedLabel.AutoSize = true;
            this.angularSpeedLabel.Location = new System.Drawing.Point(519, 400);
            this.angularSpeedLabel.Name = "angularSpeedLabel";
            this.angularSpeedLabel.Size = new System.Drawing.Size(38, 16);
            this.angularSpeedLabel.TabIndex = 20;
            this.angularSpeedLabel.Text = "rad/s";
            // 
            // impRigTextBox
            // 
            this.impRigTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.impRigTextBox.Location = new System.Drawing.Point(852, 224);
            this.impRigTextBox.Name = "impRigTextBox";
            this.impRigTextBox.Size = new System.Drawing.Size(100, 22);
            this.impRigTextBox.TabIndex = 21;
            this.impRigTextBox.Text = "0";
            this.impRigTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // impForwTextBox
            // 
            this.impForwTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.impForwTextBox.Location = new System.Drawing.Point(852, 174);
            this.impForwTextBox.Name = "impForwTextBox";
            this.impForwTextBox.Size = new System.Drawing.Size(100, 22);
            this.impForwTextBox.TabIndex = 22;
            this.impForwTextBox.Text = "0";
            this.impForwTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(790, 180);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 23;
            this.label1.Text = "Forward";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(814, 227);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 16);
            this.label2.TabIndex = 24;
            this.label2.Text = "right";
            // 
            // ROVER
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 570);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.impForwTextBox);
            this.Controls.Add(this.impRigTextBox);
            this.Controls.Add(this.angularSpeedLabel);
            this.Controls.Add(this.directionLabel);
            this.Controls.Add(this.angularSpeedBox);
            this.Controls.Add(this.directionBox);
            this.Controls.Add(this.stopTestButton);
            this.Controls.Add(this.startTestButton);
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
        private System.Windows.Forms.Button startTestButton;
        private System.Windows.Forms.Button stopTestButton;
        private System.Windows.Forms.Timer timerCS;
        private System.Windows.Forms.TextBox directionBox;
        private System.Windows.Forms.TextBox angularSpeedBox;
        private System.Windows.Forms.Label directionLabel;
        private System.Windows.Forms.Label angularSpeedLabel;
        private System.Windows.Forms.TextBox impRigTextBox;
        private System.Windows.Forms.TextBox impForwTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

