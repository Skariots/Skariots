using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace roverControl
{
    public partial class ROVER: Form
    {
        private int numPort;
        private String comOutBuf;
        private string comInBufGyro;
        private Byte[] tmpGyro;
        private int roverSpeed;
        private HashSet<Keys> pressedKeys;
        private float startTargetAngle;
        private float []anglesReceived;
        private float currentAngle;
        private float lastAngle;
        private Boolean testRunning;
        private Boolean adjusting;
        private Boolean simulationStartFlag;
        private int directionRov;
        private float angularSpeedRov;
        private char[] dirBuf;
        private char[] angSpeedBuf;
        int countCs;
        public ROVER()
        {
            InitializeComponent();
            
            this.numPort = 0;
            this.comOutBuf = null;
            this.comInBufGyro = null;
            this.tmpGyro = new byte[1];
            this.roverSpeed = 1;
            this.pressedKeys = new HashSet<Keys>();
            this.anglesReceived = new float[5];
            this.currentAngle = 0;
            this.lastAngle = 0;
            this.startTargetAngle = 0;
            this.testRunning = false;
            this.adjusting = false;
            this.simulationStartFlag = false;
            this.countCs = 0;
            this.directionRov = 0;
            this.angularSpeedRov = 0;
            this.dirBuf = new char[2];
            this.angSpeedBuf = new char[2];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Close();
                    connectButton.Text = "CONNECT";
                    commandTimer.Enabled = false;   
                }
                catch { };
            }
            else
            {
                int.TryParse(portBox.Text, out this.numPort);
                if (numPort == 0)
                {
                    for (i = 1; i < 10; i++)
                    {
                        try
                        {
                            serialPort.PortName = "COM" + i;
                            serialPort.Open();
                            connectButton.Text = "DISCONNECT";
                        }
                        catch { }
                        ;
                        if (serialPort.IsOpen)
                        {
                            commandTimer.Enabled = true;
                            portBox.Text = i.ToString();
                            break;
                        }
                    }
                }
                else
                {
                    try
                    {
                        serialPort.PortName = "COM" + portBox.Text;
                        serialPort.Open();
                        connectButton.Text = "DISCONNECT";
                    }
                    catch { };
                    if (serialPort.IsOpen)
                    {
                        commandTimer.Enabled = true;
                    }
                }      
            }

            if (!gyroPort.IsOpen)
            {
                for(int j = i;j < 10; j++)
                {
                    try
                    {
                        gyroPort.PortName = "COM" + j;
                        gyroPort.Open();
                        gyroTimer.Enabled = true;
                        orientationBox.Text = "0";
                        break;
                    }
                    catch { };
                }
            }
            else
            {
                gyroPort.Close();
                gyroTimer.Enabled= false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ActiveControl = connectButton;
        }

        private void commandTimer_Tick(object sender, EventArgs e)
        {
            if(comOutBuf != null && serialPort.IsOpen)
                serialPort.Write(comOutBuf);
            comOutBuf = null;
        }
        private void ROVER_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && this.pressedKeys.Count == 0)
            {
                comOutBuf = "F" + this.roverSpeed;
                this.forwardButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.A && this.pressedKeys.Count == 0)
            {
                comOutBuf = "L" + this.roverSpeed;
                this.leftButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.S && this.pressedKeys.Count == 0)
            {
                comOutBuf = "B" + this.roverSpeed;
                this.backwardsButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.D && this.pressedKeys.Count == 0)
            {
                comOutBuf = "R" + this.roverSpeed;
                this.rightButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.Q && this.pressedKeys.Count == 0)
            {
                comOutBuf = "Z" + this.roverSpeed;
                this.rotateLeftButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.E && this.pressedKeys.Count == 0)
            {
                comOutBuf = "C" + this.roverSpeed;
                this.rotateRightButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }

            if (e.KeyCode == Keys.F && this.pressedKeys.Count == 0)
            {
                comOutBuf = "Q" + this.roverSpeed + new String(this.dirBuf) + new String(this.angSpeedBuf);
                //this.portBox.Text = comOutBuf;
                this.pressedKeys.Add(e.KeyCode);
            }

            if (e.KeyCode == Keys.R)
            {
                if(this.roverSpeed < 9)
                {
                    this.roverSpeed += 1;
                    this.speedBox.Text = this.roverSpeed.ToString();
                }
            }

            if (e.KeyCode == Keys.T)
            {
                if (this.roverSpeed > 1)
                {
                    this.roverSpeed -= 1;
                    this.speedBox.Text = this.roverSpeed.ToString();
                }
            }
        }
        private void ROVER_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.A || e.KeyCode == Keys.S || e.KeyCode == Keys.D)
            {
                comOutBuf = "X\n";
                this.forwardButton.BackColor = Control.DefaultBackColor;
                this.rightButton.BackColor = Control.DefaultBackColor;
                this.backwardsButton.BackColor = Control.DefaultBackColor;
                this.leftButton.BackColor = Control.DefaultBackColor;
                this.pressedKeys.Remove(e.KeyCode);
            }

            ////la funzione go back si bugga quando chiamata dopo una rotazione///////////////////////
            if (e.KeyCode == Keys.Q)
            {
                comOutBuf = "X\n";
                this.rotateLeftButton.BackColor = Control.DefaultBackColor;
                this.pressedKeys.Remove(e.KeyCode);
            }

            if (e.KeyCode == Keys.E)
            {
                comOutBuf = "X\n";
                this.rotateRightButton.BackColor = Control.DefaultBackColor;
                this.pressedKeys.Remove(e.KeyCode);
            }

            if (e.KeyCode == Keys.F)
            {
                comOutBuf = "X\n";
                this.pressedKeys.Remove(e.KeyCode);
            }

            if (e.KeyCode == Keys.G)
            {
                comOutBuf = "G\n";
            }

            if (e.KeyCode == Keys.P)
            {
                this.startTargetAngle = this.currentAngle;
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            comOutBuf = "XR";
        }

        private void goBackButton_Click(object sender, EventArgs e)
        {
            comOutBuf = "G\n";
        }

        private void gyroPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try{
                comInBufGyro = gyroPort.ReadLine();
            }
            catch{ };
            //controllo se i dati rappresentano un numero valido
            /*
            if (tmpGyro[0] > 44 && tmpGyro[0] < 58 && tmpGyro[0] != 47)
            {
                comInBufGyro.Append((char)tmpGyro[0]);
                numBytesReceived++;
            }
            */   
        }

        private void gyroTimer_Tick(object sender, EventArgs e)
        {
            /*
            int currentIndexAngle = this.anglesReceived.Length;
            if(currentIndexAngle >= 5)
            {
                for(int i = 0; i < 4; i++)
                {
                    this.anglesReceived[i] = this.anglesReceived[i + 1];
                }
                currentIndexAngle = 4;
            }
            
            this.mediumAngle = anglesReceived.Average();
            */
            if (float.TryParse(comInBufGyro, NumberStyles.Float, CultureInfo.InvariantCulture, out float angle))
            {
                this.currentAngle = angle;
            }
            this.comInBufGyro = null;
            this.gyroPort.DiscardInBuffer();

            while(this.currentAngle - this.lastAngle < -180)
            {
                this.currentAngle += 360;
            }
            while(this.currentAngle - this.lastAngle > 180)
            {
                this.currentAngle -= 360;
            }
            
            this.lastAngle = this.currentAngle;
            this.orientationBox.Text = this.currentAngle.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void startTestButton_Click(object sender, EventArgs e)
        {
            this.testRunning = true;
            this.timerCS.Enabled = true;
            this.countCs = 0;
            this.adjusting = false;
            this.simulationStartFlag = true;
            //this.startTargetAngle = 0; 
        }

        private void stopTestButton_Click(object sender, EventArgs e)
        {       
            this.testRunning = false;
            this.timerCS.Enabled = false;
            this.countCs = 0;
            this.comOutBuf = "X\n";
        }

        private void timerCS_Tick(object sender, EventArgs e)
        {
            if (testRunning)
            {
                if(adjusting)
                {
                    this.countCs++;
                }
                if(this.countCs >= 50 || this.countCs == 0)
                {
                    if (this.currentAngle - startTargetAngle > 10.0)
                    {
                        this.adjusting = true;
                        this.countCs = 0;
                        comOutBuf = "N" + this.roverSpeed.ToString();
                    }
                    else if (this.currentAngle - startTargetAngle < -10.0)
                    {
                        this.adjusting = true;
                        this.countCs = 0;
                        comOutBuf = "M" + this.roverSpeed.ToString();
                    }
                    else if (this.currentAngle - startTargetAngle < 10.0 && this.currentAngle - startTargetAngle > -10.0 && (adjusting || this.simulationStartFlag)) //flag adjusting per non intasare il buffer
                    {
                        comOutBuf = "F" + this.roverSpeed.ToString();
                        //this.testRunning = false;
                        //this.timerCS.Enabled = false;
                        this.countCs = 0;
                        this.adjusting = false;
                        //this.comOutBuf = "X\n";
                    }
                }
                this.simulationStartFlag = false;
            }
        }

        private void directionBox_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(this.directionBox.Text.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out this.directionRov);
            this.directionRov /= 30;

            this.dirBuf[0] = (char)('0' + this.directionRov / 10);
            this.dirBuf[1] = (char)('0' + this.directionRov % 10);
        }

        private void angularSpeedBox_TextChanged(object sender, EventArgs e)
        {
            int tmp;
            float.TryParse(this.angularSpeedBox.Text.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out this.angularSpeedRov);

            this.angularSpeedRov *= 10;
            tmp = (int)this.angularSpeedRov;
            this.angSpeedBuf[0] = (char)('0' + tmp / 10);
            this.angSpeedBuf[1] = (char)('0' + tmp % 10);
        }
    }
}
