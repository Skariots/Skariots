using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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
        private float realAngle;
        private float lastAngle;
        private Boolean testRunning;
        private Boolean adjusting;
        private Boolean simulationStartFlag;
        private int directionRov;
        private float angularSpeedRov;
        private char[] dirBuf;
        private char[] angSpeedBuf;
        
        private String comInBufSer;
        private int impulseCountForward;
        private int impulseCountRight;
        private Boolean dontRead;

        private int countCs;
        private int timeRotateRight;
        private int timeRotateLeft;
        private double angleRotate;
        private enum Direction{forward = 1, right = 2 ,backwards = 3, left = 4, none = 0};
        private Direction direction;

        private double startAngleGoBack;
        private double curAngleGoBack;
        private int timeCorrect; // calcolato su 50 ms
        private Boolean beginCountTurning;
        private int countTurning; //contatore per quando si sta girando (ogni 50 ms +1)
        private double timeXTurn;
        private double timeYTurn;
        private Boolean correctOrientation;
        private int orientCounter;
        private Boolean correctingFlag;
        private Boolean correctSxFlag;

        //non più usate
        private Boolean correctTurn;
        private Boolean corrX;
        private Boolean corrY;
        private Direction turningDirection;
        //
        private double ANGLE_OFFSET_TOSPEED;
        private int distYDestination;
        private int preciseTurnCounter;
        private readonly double[,] distanceCoeff_speed;  //coefficienti della rette che legano la distanza y percorsa con l'angolo fatto per velocità da 0-6
        public ROVER()
        {
            InitializeComponent();
            
            this.numPort = 0;
            this.comOutBuf = null;
            this.comInBufGyro = null;
            this.tmpGyro = new byte[1];
            this.roverSpeed = 3;
            this.pressedKeys = new HashSet<Keys>();
            this.anglesReceived = new float[5];
            this.currentAngle = 0;
            this.lastAngle = 0;
            this.startTargetAngle = 0;
            this.testRunning = false; ////////////////////////////////////////////
            this.adjusting = false;
            this.simulationStartFlag = false;
            this.timerCS.Enabled = true;
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
            this.timeRotateLeft = 0;
            this.timeRotateRight = 0;
            this.angleRotate = 0;
            this.startAngleGoBack = 0;
            this.curAngleGoBack = 0;
            this.beginCountTurning = false;
            this.countTurning = 0;
            this.timeXTurn = 0;
            this.timeYTurn = 0;
            this.correctOrientation = false;
            this.orientCounter = 0;
            this.correctingFlag = false;
            this.timeCorrect = 0;
            this.correctSxFlag = false;
            this.correctTurn = false;
            this.corrX = false;
            this.corrY = false;
            this.turningDirection = Direction.none;
            this.ANGLE_OFFSET_TOSPEED = 2.7;
            this.distYDestination = 0;
            this.preciseTurnCounter = 0;
            this.distanceCoeff_speed = new double[6, 3];

            this.distanceCoeff_speed[1, 0] = -46.084;
            this.distanceCoeff_speed[1, 1] = 14.826;
            this.distanceCoeff_speed[1, 2] = -0.0806;

            this.distanceCoeff_speed[4,0] = -28.017;
            this.distanceCoeff_speed[4,1] = 14.962;
            this.distanceCoeff_speed[4,2] = -0.082;            

            this.distanceCoeff_speed[5, 0] = -36.454;
            this.distanceCoeff_speed[5, 1] = 17.687;
            this.distanceCoeff_speed[5, 2] = -0.0968;
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
            this.speedBox.Text = this.roverSpeed.ToString();
        }

        private void commandTimer_Tick(object sender, EventArgs e)
        { 
            if(comOutBuf != null && serialPort.IsOpen)
                serialPort.Write(comOutBuf);
            comOutBuf = null;

            if (this.beginCountTurning)
            {
                this.countTurning++;
            }

            this.preciseTurnCounter++;
        }
        private void ROVER_KeyDown(object sender, KeyEventArgs e)
        {
            
           if ((e.KeyCode == Keys.W || e.KeyCode == Keys.A || e.KeyCode == Keys.S || e.KeyCode == Keys.D) && this.direction == Direction.none)
            {
                this.startTargetAngle = this.currentAngle;
                this.testRunning = true;
                this.timeRotateLeft = 0;
                this.timeRotateRight = 0;

            }
            
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
                this.beginCountTurning = true;
                this.turningDirection = Direction.forward;
                this.pressedKeys.Add(e.KeyCode);
            }

            if (e.KeyCode == Keys.G && this.pressedKeys.Count == 0)
            {
                comOutBuf = "E" + this.roverSpeed + new String(this.dirBuf) + new String(this.angSpeedBuf);
                this.beginCountTurning = true;
                this.turningDirection = Direction.backwards;
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
                this.direction = Direction.none;
                this.testRunning = false;
            }

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
                this.turningDirection = Direction.none;
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
            this.startAngleGoBack = this.currentAngle;
            
            this.impulseCountForward = 0;
            this.impulseCountRight = 0;
            this.dontRead = true;

        }

        private void goBackButton_Click(object sender, EventArgs e)
        {
            this.comOutBuf = "G";
        }

        private void preciseTurn(int direction, int linearSpeed, double angularRate, double startTurnAngle, int destination)
        {
            int time_50ms = 0;
            double distYTurn;

            if (direction > 180) direction -= 360;

            double finishTurnAngle = startTurnAngle - direction;
            convertDirection(this.directionRov, this.dirBuf);
            convertAngularSpeed(this.angularSpeedRov, this.angSpeedBuf);            

            if (direction > 0)
            {
                this.dirBuf[0] = '0';
                this.dirBuf[1] = '3';
            }
            else if (direction < 0)
            {
                this.dirBuf[0] = '0';
                this.dirBuf[1] = '9';
            }
            
            distYTurn = this.distanceCoeff_speed[linearSpeed - 1, 2] * Math.Pow(direction, 2) + this.distanceCoeff_speed[linearSpeed - 1, 1] * Math.Abs(direction) + this.distanceCoeff_speed[linearSpeed - 1, 0];

            if (destination >= 0)
            {
                this.comOutBuf = "F" + this.roverSpeed.ToString();
                time_50ms = convertMMtoTime(destination - (int)Math.Round(distYTurn), this.roverSpeed) / 50;
            }
            else if (destination < 0)
            {
                this.comOutBuf = "B" + this.roverSpeed.ToString();
                time_50ms = convertMMtoTime((destination * -1) - (int)Math.Round(distYTurn), this.roverSpeed) / 50;
            }
            
            this.preciseTurnCounter = 0;

            while(this.preciseTurnCounter < time_50ms)
            {
                Application.DoEvents(); //non ferma l'esecuzione di altri thread
            }

            if(destination >= 0)
            {
                this.comOutBuf = "Q" + this.roverSpeed + new String(this.dirBuf) + new String(this.angSpeedBuf);
            }               

            else if (destination < 0)
            {
                this.comOutBuf = "E" + this.roverSpeed + new String(this.dirBuf) + new String(this.angSpeedBuf);
            }

            while (Math.Abs(this.currentAngle - finishTurnAngle) > this.ANGLE_OFFSET_TOSPEED * this.roverSpeed) // dipende anche dall'angolo finale non solo dalla velocità
            {
                Application.DoEvents(); 
            }

            this.comOutBuf = "X";
        }

        private void gyroPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try{
                comInBufGyro = gyroPort.ReadLine();
            }
            catch{ };
        }

        private void gyroTimer_Tick(object sender, EventArgs e)
        {
            if (float.TryParse(comInBufGyro, NumberStyles.Float, CultureInfo.InvariantCulture, out float angle))
            {
                this.currentAngle = angle;
                this.realAngle = angle;
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
            this.realAngleTextBox.Text = this.realAngle.ToString();
            this.impForwTextBox.Text = this.impulseCountForward.ToString();
            this.impRigTextBox.Text = this.impulseCountRight.ToString();
        }

        private void startTestButton_Click(object sender, EventArgs e)
        {
            preciseTurn(this.directionRov,this.roverSpeed,this.angularSpeedRov,this.currentAngle, this.distYDestination);
        }

        private void stopTestButton_Click(object sender, EventArgs e)
        {
            this.comOutBuf = "X\n";
        }

        private void timerCS_Tick(object sender, EventArgs e)
        {
            if (testRunning)
            {
                if (this.currentAngle - this.startTargetAngle > 3.0)
                {
                    if (!adjusting)
                    {
                        if (this.direction == Direction.forward)
                            comOutBuf = "N" + this.roverSpeed.ToString() + "F";
                        else if (this.direction == Direction.right)
                            comOutBuf = "N" + this.roverSpeed.ToString() + "R";
                        else if (this.direction == Direction.backwards)
                            comOutBuf = "N" + this.roverSpeed.ToString() + "B";
                        else if (this.direction == Direction.left)
                            comOutBuf = "N" + this.roverSpeed.ToString() + "L";
                    }
                    this.adjusting = true;
                }
                else if (this.currentAngle - this.startTargetAngle < -3.0)
                {
                    if (!adjusting)
                    {
                        if (direction == Direction.forward)
                            comOutBuf = "M" + this.roverSpeed.ToString() + "F";
                        else if (direction == Direction.right)
                            comOutBuf = "M" + this.roverSpeed.ToString() + "R";
                        else if (direction == Direction.backwards)
                            comOutBuf = "M" + this.roverSpeed.ToString() + "B";
                        else if (direction == Direction.left)
                            comOutBuf = "M" + this.roverSpeed.ToString() + "L";
                    }
                    this.adjusting = true;
                }
                else if(this.adjusting)
                {
                    if (direction == Direction.forward)
                        comOutBuf = "F" + this.roverSpeed.ToString();
                    if (direction == Direction.right)
                        comOutBuf = "R" + this.roverSpeed.ToString();
                    if (direction == Direction.backwards)
                        comOutBuf = "B" + this.roverSpeed.ToString();
                    if (direction == Direction.left)
                        comOutBuf = "L" + this.roverSpeed.ToString();

                    this.adjusting = false;
                }
            }
        }

        private void directionBox_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(this.directionBox.Text.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out this.directionRov);
            convertDirection(this.directionRov,this.dirBuf);
        }

        private void convertDirection(int dir, char[]dirbuf)
        {
            int tmpDir;
            tmpDir = dir / 30;

            dirBuf[0] = (char)('0' + tmpDir / 10);
            dirBuf[1] = (char)('0' + tmpDir % 10);
        }

        private void angularSpeedBox_TextChanged(object sender, EventArgs e)
        {
            float.TryParse(this.angularSpeedBox.Text.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out this.angularSpeedRov);

            convertAngularSpeed(this.angularSpeedRov, this.angSpeedBuf);
        }
        
        private void convertAngularSpeed(double v,char[] angBuf)
        {
            double tmp;
            tmp = v * 10;
            tmp = (int)tmp;
            angBuf[0] = (char)('0' + tmp / 10);
            angBuf[1] = (char)('0' + tmp % 10);
        }

        int convertMMtoTime(int distMM, int speed) //milllisecondi necessari a percorrere la distanza richiesta alla velocità data
        {
            double time;
            double mm_ms = speed * 53.75 / 1000;
            time = distMM/mm_ms;
            int timeInt = (int)Math.Round(time);

            return timeInt;
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

        private void distTextBox_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(this.distTextBox.Text.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out this.distYDestination);
        }
    }
}
