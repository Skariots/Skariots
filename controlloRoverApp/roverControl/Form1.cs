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
        private int dataNumber;
        private Byte[] tmpGyro;
        private int roverSpeed;
        private HashSet<Keys> pressedKeys;
        private double startTargetAngle;
        private float []anglesReceived;
        private float currentAngle;
        private float realAngle;
        private float lastAngle;
        private Boolean testRunning;
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
        private enum Direction{forward = 1, right = 2 ,backwards = 3, left = 4, turning = 5, none = 0};
        private Direction direction;

        private double startAngleGoBack;
        private int timeCorrect; // calcolato su 50 ms
        private Boolean beginCountTurning;   //non serve più
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
        private int distXDestination;
        private int preciseTurnCounter;
        private readonly double[] speedTo_curveRadiusX; //associa ad ogni velocità il raggio di curvatura in x (in millimetri)
        private readonly double[] speedTo_curveRadiusY; //associa ad ogni velocità il raggio di curvatura in y (in millimetri)

        private double total_dx;
        private double total_dy;
        private Boolean backTrackingFlag;
        private int backTrackingCounter;

        private String dataReceivedText;
        private double angleD;
        private double beforeTurnAngle;
        public ROVER()
        {
            InitializeComponent();
            
            this.numPort = 0;
            this.comOutBuf = null;
            this.comInBufGyro = null;
            this.dataNumber = 0;
            this.tmpGyro = new byte[1];
            this.roverSpeed = 3;
            this.pressedKeys = new HashSet<Keys>();
            this.anglesReceived = new float[5];
            this.currentAngle = 0;
            this.lastAngle = 0;
            this.startTargetAngle = 0;
            this.testRunning = false; ////////////////////////////////////////////
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
            this.ANGLE_OFFSET_TOSPEED = 2.5;
            this.distYDestination = 0;
            this.distXDestination = 0;
            this.preciseTurnCounter = 0;
            this.speedTo_curveRadiusX = new double[6];
            this.speedTo_curveRadiusY = new double[6];

            this.speedTo_curveRadiusX[1] = 530; //millimetri
            this.speedTo_curveRadiusY[1] = 690; //millimetri

            this.speedTo_curveRadiusX[2] = 525; //millimetri
            this.speedTo_curveRadiusY[2] = 675; //millimetri

            this.speedTo_curveRadiusX[3] = 524; //millimetri
            this.speedTo_curveRadiusY[3] = 681; //millimetri

            this.speedTo_curveRadiusX[4] = 550; //millimetri
            this.speedTo_curveRadiusY[4] = 710; //millimetri

            this.total_dx = 0;
            this.total_dy = 0;
            this.backTrackingFlag = false;
            this.backTrackingCounter = 0;

            this.angleD = 0;
            this.beforeTurnAngle = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            int.TryParse(this.speedBox.Text.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out this.roverSpeed);
            this.dirBuf[0] = '0';
            this.dirBuf[1] = '0';
            this.angSpeedBuf[0] = '0';
            this.angSpeedBuf[1] = '0';
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
            {
                serialPort.Write(comOutBuf + ":" + this.dataNumber);
                this.dataNumber++;

                if (this.comOutBuf[0] == 'F') this.direction = Direction.forward;
                else if (this.comOutBuf[0] == 'B') this.direction = Direction.backwards;
                else if (this.comOutBuf[0] == 'L') this.direction = Direction.left;
                else if (this.comOutBuf[0] == 'R') this.direction = Direction.right;
                else if (this.comOutBuf[0] == 'Q' || this.comOutBuf[0] == 'E' || this.comOutBuf[0] == 'Z' || this.comOutBuf[0] == 'C') this.direction = Direction.turning;
            }

            if(this.dataReceivedText != null)
                this.dataReceivedBox.Text += this.dataReceivedText;

            this.dataReceivedText = null;

            if(this.dataReceivedBox.Text.Length > 300)
            {
                this.dataReceivedBox.Text = null;
            }

            if(this.dataNumber > 100) 
               this.dataNumber = 0;

            this.angleDebugBox.Text = this.angleD.ToString();
            /*
            if (this.correctOrientation)
            {
                if (this.orientCounter < this.timeCorrect && !this.correctingFlag && !this.correctSxFlag) //36,04   34,77   37,93   37,97   36,28   38,23   37,41  -> media 37 gradi al secondo (speed 2)
                {
                    this.comOutBuf = "Z" + 2;
                    this.correctingFlag = true;
                }
                else if (this.orientCounter < this.timeCorrect && !this.correctingFlag && this.correctSxFlag)
                {
                    this.comOutBuf = "C" + 2;
                    this.correctingFlag = true;
                }
                else if (this.orientCounter >= this.timeCorrect)
                {
                    this.comOutBuf = "X";
                    this.correctOrientation = false;
                    this.correctingFlag = false;
                }
                this.orientCounter++;
            }
            */
            
            if(this.beginCountTurning)
            {
                this.countTurning++;
            }
            
            this.preciseTurnCounter++;
            double diffAngle = this.currentAngle - this.startAngleGoBack;
            double DEG_TO_RAD = Math.PI / 180;

            // 53.75/20 * this.roverSpeed = mm/s;

            if (backTrackingFlag)
            {
                this.backTrackingCounter++;
            }
            else if(this.direction != Direction.none)
            {
                if (this.direction == Direction.forward)
                {
                    total_dy += 53.75 / 40 * this.roverSpeed * Math.Cos(diffAngle * DEG_TO_RAD);
                    total_dx += 53.75 / 40 * this.roverSpeed * Math.Sin(diffAngle * DEG_TO_RAD);
                }
                else if (this.direction == Direction.backwards)
                { 
                    total_dy -= 53.75 / 40 * this.roverSpeed * Math.Cos(diffAngle * DEG_TO_RAD);
                    total_dx -= 53.75 / 40 * this.roverSpeed * Math.Sin(diffAngle * DEG_TO_RAD);
                } 
                else if (this.direction == Direction.right)
                {
                    total_dy -= 53.75 / 40 * this.roverSpeed * Math.Sin(diffAngle * DEG_TO_RAD);
                    total_dx += 53.75 / 40 * this.roverSpeed * Math.Cos(diffAngle * DEG_TO_RAD);   
                }
                else if (this.direction == Direction.left)
                {
                    total_dy += 53.75 / 40 * this.roverSpeed * Math.Sin(diffAngle * DEG_TO_RAD);
                    total_dx -= 53.75 / 40 * this.roverSpeed * Math.Cos(diffAngle * DEG_TO_RAD);    
                }
            }
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
                //this.direction = Direction.forward;
                this.forwardButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.A && this.pressedKeys.Count == 0)
            {
                comOutBuf = "L" + this.roverSpeed;
                //this.direction = Direction.left;
                this.leftButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.S && this.pressedKeys.Count == 0)
            {
                comOutBuf = "B" + this.roverSpeed;
                //this.direction = Direction.backwards;
                this.backwardsButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.D && this.pressedKeys.Count == 0)
            {
                comOutBuf = "R" + this.roverSpeed;
                //this.direction = Direction.right;
                this.rightButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.Q && this.pressedKeys.Count == 0)
            {
                comOutBuf = "Z" + this.roverSpeed;
                //this.direction = Direction.turning;
                this.rotateLeftButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }
            if (e.KeyCode == Keys.E && this.pressedKeys.Count == 0)
            {
                comOutBuf = "C" + this.roverSpeed;
                //this.direction = Direction.turning;
                this.rotateRightButton.BackColor = Color.LightGray;
                this.pressedKeys.Add(e.KeyCode);
            }

            if (e.KeyCode == Keys.F && this.pressedKeys.Count == 0)
            {
                this.beforeTurnAngle = this.currentAngle;
                comOutBuf = "Q" + this.roverSpeed + new String(this.dirBuf) + new String(this.angSpeedBuf);
                this.beginCountTurning = true;
                //this.direction = Direction.turning;
                this.turningDirection = Direction.forward;
                this.pressedKeys.Add(e.KeyCode);
            }

            if (e.KeyCode == Keys.G && this.pressedKeys.Count == 0)
            {
                this.beforeTurnAngle = this.currentAngle;
                comOutBuf = "E" + this.roverSpeed + new String(this.dirBuf) + new String(this.angSpeedBuf);
                this.beginCountTurning = true;
                //this.direction = Direction.turning;
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
        private async void ROVER_KeyUp(object sender, KeyEventArgs e)
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
                await Task.Delay(500);
                UpdateDistanceEndTurn();
                this.beginCountTurning = false;
                this.countTurning = 0;  
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
            this.total_dx = 0;
            this.total_dy = 0;
            this.backTrackingCounter = 0;
            this.backTrackingFlag = false;

            //non servono più
            this.impulseCountForward = 0;
            this.impulseCountRight = 0;
            this.dontRead = true;
        }

        private void goBackButton_Click(object sender, EventArgs e)
        {
            this.comOutBuf = "G";
        }

        private void preciseTurn(int direction, int linearSpeed, double angularRate, double startTurnAngle, int destinationY, int destinationX)
        {
            int time_25ms = 0;
            int distYTurn = 0, distXTurn = 0;
            int distRemainingX, distRemainingY, distRemaining, distYFinal = 0, distXFinal = 0, s_y = 0, s_x = 0;

            double DEG_TO_RAD = Math.PI / 180;

            if (direction > 180) direction -= 360;

            double finishTurnAngle = startTurnAngle - direction;
            convertDirection(this.directionRov, this.dirBuf);
            convertAngularSpeed(this.angularSpeedRov, this.angSpeedBuf);

            distYTurn = (int)Math.Round(this.speedTo_curveRadiusY[linearSpeed - 1] * Math.Sin(Math.Abs(direction * DEG_TO_RAD)));
            distXTurn = (int)Math.Round(this.speedTo_curveRadiusX[linearSpeed - 1] * (1 - Math.Cos(Math.Abs(direction * DEG_TO_RAD))));

            if (direction >= 0)
            {
                this.dirBuf[0] = '0';
                this.dirBuf[1] = '3';
                distXTurn *= -1;
            }
            else if (direction < 0)
            {
                this.dirBuf[0] = '0';
                this.dirBuf[1] = '9';
            }

            if (destinationY < 0) // si invertono dx e dy
            {
                distXTurn *= -1;
                distYTurn *= -1;
            }

            s_y = destinationY - distYTurn;
            s_x = destinationX - distXTurn;

            /*
            distRemainingX = destinationX - distXTurn;
            //distRemainingY = (destinationY - distyTurn - s_y)   -> s_y = destinationY - distYTurn - distRemainingY;  -> se distRemaining = distYFinal allora si arriva perfettamente
            if (distRemainingX * Math.Sin(finishTurnAngle * DEG_TO_RAD) > 0) //orientamento finale concorde con distanza rimanente
            {
                distYFinal = (int)Math.Round(distRemainingX * Math.Tan(finishTurnAngle * DEG_TO_RAD)); //la distanza che deve rimanere alla fine della svolta per arrivare perfettamente alla destinazione
                s_y = destinationY - distYTurn - distYFinal;
                distXFinal = s_x;
                s_x = 0;
            }
            */

            if (s_x > 0)
            {
                this.comOutBuf = "R" + this.roverSpeed.ToString();
                time_25ms = convertMMtoTime(s_x, this.roverSpeed) / 25;
            }
            else if (s_x < 0)
            {
                this.comOutBuf = "L" + this.roverSpeed.ToString();
                time_25ms = convertMMtoTime(Math.Abs(s_x), this.roverSpeed) / 25;
            }

            this.preciseTurnCounter = 0;
            while (this.preciseTurnCounter < time_25ms)
            {
                Application.DoEvents(); //non ferma l'esecuzione di altri thread
            }

            if (s_y > 0)
            {
                this.comOutBuf = "F" + this.roverSpeed.ToString();
                time_25ms = convertMMtoTime(s_y, this.roverSpeed) / 25;
            }
            else if (s_y < 0)
            {
                this.comOutBuf = "B" + this.roverSpeed.ToString();
                time_25ms = convertMMtoTime(Math.Abs(s_y), this.roverSpeed) / 25;
            }

            this.preciseTurnCounter = 0;
            while(this.preciseTurnCounter < time_25ms)
            {
                Application.DoEvents(); //non ferma l'esecuzione di altri thread
            }

            if(destinationY >= 0)
            {
                this.comOutBuf = "Q" + this.roverSpeed + new String(this.dirBuf) + new String(this.angSpeedBuf);
            }               

            else if (destinationY < 0)
            {
                this.comOutBuf = "E" + this.roverSpeed + new String(this.dirBuf) + new String(this.angSpeedBuf);
            }

            while (Math.Abs(this.currentAngle - finishTurnAngle) > this.ANGLE_OFFSET_TOSPEED * this.roverSpeed) // dipende anche dall'angolo finale non solo dalla velocità
            {
                Application.DoEvents(); 
            }

            /*
            if(distXFinal != 0)
            {
                this.comOutBuf = "F" + this.roverSpeed;
                time_50ms = convertMMtoTime(Math.Abs(distXFinal), this.roverSpeed) / 50;
            }

            this.preciseTurnCounter = 0;
            while (this.preciseTurnCounter < time_50ms)
            {
                Application.DoEvents(); //non ferma l'esecuzione di altri thread
            }

            */

            this.comOutBuf = "X";
            this.total_dx += s_x + distXTurn;
            this.total_dy += s_y + distYTurn;
        }

        private void UpdateDistanceEndTurn()
        {
            //double seconds = (double)this.countTurning * 25 / 1000;
            double dx, dy, angle;

            //angle = this.angularSpeedRov * seconds;

            angle = this.beforeTurnAngle - this.currentAngle;
            this.angleD = angle;

            double DEG_TO_RAD = Math.PI / 180;

            /*
            if (this.directionRov > 180)
            {
                angle *= -1;
            }
            */

            dy = (int)Math.Round(this.speedTo_curveRadiusY[this.roverSpeed - 1] * Math.Sin(Math.Abs(angle * DEG_TO_RAD)));
            dx = (int)Math.Round(this.speedTo_curveRadiusX[this.roverSpeed - 1] * (1 - Math.Cos(Math.Abs(angle * DEG_TO_RAD))));

            if (this.directionRov < 180)
            {
                dx *= -1;
            }

            if (this.turningDirection < 0)
            {
                dx *= -1;
                dy *= -1;
            }

            this.total_dx += dx;
            this.total_dy += dy;
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
            try
            {
                this.gyroPort.DiscardInBuffer();
            }
            catch { };

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
            preciseTurn(this.directionRov,this.roverSpeed,this.angularSpeedRov,this.currentAngle, this.distYDestination, this.distXDestination);
        }

        private void stopTestButton_Click(object sender, EventArgs e)
        {
            this.comOutBuf = "X";
        }

        private void timerCS_Tick(object sender, EventArgs e)
        {
            if (testRunning)
            {
                if (this.currentAngle - this.startTargetAngle > 1.0)
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
                else if (this.currentAngle - this.startTargetAngle < -1.0)
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
                else
                {
                    if (direction == Direction.forward)
                        comOutBuf = "F" + this.roverSpeed.ToString();
                    else if (direction == Direction.right)
                        comOutBuf = "R" + this.roverSpeed.ToString();
                    else if (direction == Direction.backwards)
                        comOutBuf = "B" + this.roverSpeed.ToString();
                    else if (direction == Direction.left)
                        comOutBuf = "L" + this.roverSpeed.ToString();
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
            if (type == 'c' || type == '\0') this.dataReceivedText = this.comInBufSer;
            
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
            int.TryParse(this.distYTextBox.Text.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out this.distYDestination);
        }

        private void distXTextBox_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(this.distXTextBox.Text.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out this.distXDestination);
        }

        private async void goBackFastButton_Click(object sender, EventArgs e)
        {
            //aggiusta l'orientamento, inverte dx e dy e poi cammina;
            int time_25ms;
            this.backTrackingFlag = true;
            this.orientCounter = 0;
            this.correctOrientation = true;


            double finalAngle = this.startAngleGoBack - 180;

            if(finalAngle > this.currentAngle)
            {
                this.comOutBuf = "C" + this.roverSpeed;
            }
            else
            {
                this.comOutBuf = "Z" + this.roverSpeed;
            }

            while (Math.Abs(this.currentAngle-finalAngle) > 5.0)
            {
                if (Math.Abs(this.currentAngle - finalAngle) < 25.0)
                {
                    this.comOutBuf = this.comOutBuf.Substring(0, 1) + '1';
                }
                else if (Math.Abs(this.currentAngle - finalAngle) < 45.0)
                {
                    this.comOutBuf = this.comOutBuf.Substring(0, 1) + '2';
                }
                Application.DoEvents();
            }

            //correzione della direzione
            this.startTargetAngle = finalAngle;
            this.testRunning = true;

            this.total_dx *= -1;
            this.total_dy *= -1;

            if(total_dx > 1)
            {
                this.comOutBuf = "L" + this.roverSpeed;
            }
            else if(total_dx < 1)
            {
                this.comOutBuf = "R" + this.roverSpeed;
            }

            time_25ms = convertMMtoTime((int)Math.Round(Math.Abs(total_dx)), this.roverSpeed) / 25;
            this.backTrackingCounter = 0;
            while(this.backTrackingCounter < time_25ms)
            {
                Application.DoEvents();
            }

            if(total_dy > 1)
            {
                this.comOutBuf = "B" + this.roverSpeed;
            }
            else if(total_dy < 1)
            {
                this.comOutBuf = "F" + this.roverSpeed;
            }

            time_25ms = convertMMtoTime((int)Math.Round(Math.Abs(total_dy)), this.roverSpeed) / 25;
            this.backTrackingCounter = 0;
            while(this.backTrackingCounter < time_25ms)
            {
                Application.DoEvents();
            }

            resetButton_Click(null,null);

            //fine correzione della direzione
            this.testRunning = false;
        }
    }
}
