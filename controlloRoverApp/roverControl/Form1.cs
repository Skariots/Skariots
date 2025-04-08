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
        private int countCs;
        private String comInBufSer;
        private int impulseCountForward;
        private int impulseCountRight;
        private Boolean dontRead;
        private enum Direction{forward = 1, right = 2 ,backwards = 3, left = 4, none = 0};
        private Direction direction;
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
            this.comInBufSer = null;
            this.impulseCountForward = 0;
            this.impulseCountRight = 0;
            this.dontRead = false;
            this.direction = Direction.none;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            this.currentAngle = 0;
            this.lastAngle = 0;
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
                    for (i = 1; i < 15; i++)
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
                for(int j = i;j < 15; j++)
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
            this.stopTestButton_Click(null,null);
            if (e.KeyCode == Keys.W && this.pressedKeys.Count == 0)
            {
                comOutBuf = "F" + this.roverSpeed;
                this.direction = Direction.forward;
                this.forwardButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.A && this.pressedKeys.Count == 0)
            {
                comOutBuf = "L" + this.roverSpeed;
                this.direction = Direction.left;
                this.leftButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.S && this.pressedKeys.Count == 0)
            {
                comOutBuf = "B" + this.roverSpeed;
                this.direction = Direction.backwards;
                this.backwardsButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.D && this.pressedKeys.Count == 0)
            {
                comOutBuf = "R" + this.roverSpeed;
                this.direction = Direction.right;
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

            if (e.KeyCode == Keys.G && this.pressedKeys.Count == 0)
            {
                comOutBuf = "E" + this.roverSpeed + new String(this.dirBuf) + new String(this.angSpeedBuf);
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

            if (e.KeyCode == Keys.Enter)
            {
                connectButton.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true; 
            }

            if (e.KeyCode == Keys.W || e.KeyCode == Keys.A || e.KeyCode == Keys.S || e.KeyCode == Keys.D)
            {
                comOutBuf = "X";
                this.forwardButton.BackColor = Control.DefaultBackColor;
                this.rightButton.BackColor = Control.DefaultBackColor;
                this.backwardsButton.BackColor = Control.DefaultBackColor;
                this.leftButton.BackColor = Control.DefaultBackColor;
                this.pressedKeys.Remove(e.KeyCode);
            }

            ////la funzione go back si bugga quando chiamata dopo una rotazione///////////////////////
            if (e.KeyCode == Keys.Q)
            {
                comOutBuf = "X";
                this.rotateLeftButton.BackColor = Control.DefaultBackColor;
                this.pressedKeys.Remove(e.KeyCode);
            }

            if (e.KeyCode == Keys.E)
            {
                comOutBuf = "X";
                this.rotateRightButton.BackColor = Control.DefaultBackColor;
                this.pressedKeys.Remove(e.KeyCode);
            }

            if (e.KeyCode == Keys.F || e.KeyCode == Keys.G)
            {
                comOutBuf = "X";
                this.pressedKeys.Remove(e.KeyCode);
            }

            if (e.KeyCode == Keys.G)
            {
                comOutBuf = "G";
            }

            if (e.KeyCode == Keys.P)
            {
                this.startTargetAngle = this.currentAngle;
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            comOutBuf = "XR";
            this.impulseCountForward = 0;
            this.impulseCountRight = 0;
            this.dontRead = true;
        }

        private void goBackButton_Click(object sender, EventArgs e)
        {
            comOutBuf = "G";
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

            this.currentAngle = (this.currentAngle + this.lastAngle) / 2; //media tra gli ultimi 2 valori del giroscopio
            this.lastAngle = this.currentAngle;
            this.orientationBox.Text = this.currentAngle.ToString("F2", CultureInfo.InvariantCulture);
            this.impForwTextBox.Text = this.impulseCountForward.ToString();
            this.impRigTextBox.Text = this.impulseCountRight.ToString();
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
                if(this.countCs >= 10 || this.countCs == 0)
                {
                    if (this.currentAngle - startTargetAngle > 10.0)
                    {
                        this.adjusting = true;
                        this.countCs = 0;
                        comOutBuf = "Q" + this.roverSpeed.ToString() + "03" + "02";
                    }
                    else if (this.currentAngle - startTargetAngle < -10.0)
                    {
                        this.adjusting = true;
                        this.countCs = 0;
                        comOutBuf = "Q" + this.roverSpeed.ToString() + "09" + "02";
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
                /*
                if (adjusting)
                {
                    this.countCs++;
                }
                if(this.countCs > 10)
                {
                    if (this.timeToRotateLeft > 0)
                    {
                        comOutBuf = "Q" + this.roverSpeed.ToString() + "03" + "04"; ////overflow buffer
                        this.timeToRotateLeft--;
                    }

                    if (this.timeToRotateRight > 0)
                    {
                        comOutBuf = "Q" + this.roverSpeed.ToString() + "09" + "04"; // overflow buffer
                        this.timeToRotateRight--;
                    }
                }
                if (this.countCs >= 50 || this.countCs == 0)
                {
                    if (this.currentAngle - startTargetAngle > 10.0 && !adjusting)
                    {
                        this.adjusting = true;
                        this.countCs = 0;
                        this.angleToRotate = Math.Abs(this.currentAngle - startTargetAngle);
                        this.timeToRotateLeft = (int)(angleToRotate / (float)2.4); // tempo in decimi di secondo                        
                    }
                    else if (this.currentAngle - startTargetAngle < -10.0 && !adjusting)
                    {
                        this.adjusting = true;
                        this.countCs = 0;
                        this.angleToRotate = Math.Abs(this.currentAngle - startTargetAngle);
                        this.timeToRotateRight = (int)(angleToRotate / (float)2.4); // tempo in decimi di secondo
                        
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
                */
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

        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            char type = '\0';

            try
            {
                this.comInBufSer = this.serialPort.ReadLine();
                type = this.comInBufSer[0];
                this.comInBufSer = this.comInBufSer.Substring(1);

            }
            catch { };
            if (type == 'c' || type == '\0') return;
            
            if(type == 'r')
            {
                if (this.dontRead)
                {
                    this.dontRead = false;
                    return;
                }
                int.TryParse(comInBufSer, NumberStyles.Integer, CultureInfo.InvariantCulture, out this.impulseCountRight);
            }

            if (type == 'f')
            {
                if (this.dontRead)
                {
                    return;
                }
                int.TryParse(comInBufSer, NumberStyles.Integer, CultureInfo.InvariantCulture, out this.impulseCountForward);
            }
        }
    }
}
