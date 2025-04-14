using System;
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

        private Boolean correctTurn;
        private Boolean corrX;
        private Boolean corrY;
        private Direction turningDirection;

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
            if (this.correctOrientation)
            {
                if(this.orientCounter < this.timeCorrect && !this.correctingFlag && !this.correctSxFlag) //36,04   34,77   37,93   37,97   36,28   38,23   37,41  -> media 37 gradi al secondo (speed 2)
                {
                    this.comOutBuf = "Z" + 2;
                    this.correctingFlag = true;
                }
                else if (this.orientCounter < this.timeCorrect && !this.correctingFlag && this.correctSxFlag)
                {
                    this.comOutBuf = "C" + 2;
                    this.correctingFlag = true;
                }
                else if(this.orientCounter >= this.timeCorrect)
                {
                    this.comOutBuf = "X";
                    this.correctOrientation = false;
                    this.correctingFlag = false;
                }
                this.orientCounter++;
            }


            if (this.correctTurn)
            {
                if(this.timeXTurn == 0) this.corrX = false;
                if(this.timeYTurn == 0) this.corrY = false;

                if(this.timeXTurn > 0)
                {
                    this.timeXTurn--;
                    if (!corrX && !corrY)
                    {
                        this.comOutBuf = "L" + this.roverSpeed;
                        this.corrX = true;
                    }  
                }
                else if (this.timeXTurn < 0)
                {
                    this.timeXTurn++;
                    if (!corrX && !corrY)
                    {
                        this.comOutBuf = "L" + this.roverSpeed;
                        this.corrX = true;
                    } 
                }
                else if (this.timeYTurn > 0)
                {
                    this.timeYTurn--;
                    if (!corrX && !corrY)
                    {
                        this.comOutBuf = "B" + this.roverSpeed;
                        this.corrY = true;
                    }                   
                }
                else if (this.timeYTurn < 0 && !corrX && !corrY)
                {
                    this.timeYTurn++;
                    if (!corrX && !corrY)
                    {
                        this.comOutBuf = "B" + this.roverSpeed;
                        this.corrY = true;
                    }
                    
                }
                else if(this.timeXTurn == 0 && this.timeYTurn == 0)
                {
                    this.correctTurn = false;
                    this.comOutBuf = "X";
                    this.corrX = false;
                    this.corrY = false;
                }
            }

            if(comOutBuf != null && serialPort.IsOpen)
                serialPort.Write(comOutBuf);
            comOutBuf = null;

            if (this.beginCountTurning)
            {
                this.countTurning++;
            }
        }
        private void ROVER_KeyDown(object sender, KeyEventArgs e)
        {
            this.stopTestButton_Click(null,null);
            
            if ((e.KeyCode == Keys.W || e.KeyCode == Keys.A || e.KeyCode == Keys.S || e.KeyCode == Keys.D) && this.direction == Direction.none)
            {
                this.startTargetAngle = this.currentAngle;
                //this.timerCS.Enabled = true; //messo a true nel form
                this.testRunning = true;
                this.timeRotateLeft = 0;
                this.timeRotateRight = 0;

                this.KeyDown -= ROVER_KeyDown;
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

                this.KeyDown += ROVER_KeyDown;
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
                this.manageEndTurn();
                this.beginCountTurning = false;
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
            this.impulseCountForward = 0;
            this.impulseCountRight = 0;
            this.dontRead = true;

            this.startAngleGoBack = this.currentAngle;

            this.countTurning = 0;
            this.orientCounter = 0;
        }

        private async void goBackButton_Click(object sender, EventArgs e)
        {
            //raddrizzare il rovere e azzerare il contatore spostandosi prima di cominciare con il goBack {}
            int waittime;
            this.orientCounter = 0;
            this.correctOrientation = true;
            this.correctSxFlag = false;

            this.curAngleGoBack = this.currentAngle;
            this.timeCorrect = (int)((this.curAngleGoBack - this.startAngleGoBack) * 20 / 37);  //  20/37 -> tempo per fare un grado

            if(this.timeCorrect < 0)
            {
                this.timeCorrect *= -1;
                this.correctSxFlag = true;
            }

            //aspetta che finisca la correzione   
            await Task.Delay(timeCorrect * 50 + 100);//////////////////////////////////non solido
            
            ////ricalcolo di tx e ty rispetto alla velocità corrente
            this.timeXTurn = Convert.ToInt32(this.timeXTurn);  // aggiungere /this.roverSpeed
            this.timeYTurn = Convert.ToInt32(this.timeYTurn);
            waittime = (Math.Abs((int)(timeXTurn + timeYTurn)) * 50 + 500);
            this.correctTurn = true;
            
            await Task.Delay(waittime);

            //reset tempi (Tx e Ty sono gia azzerati)
            
            this.countTurning = 0;
            /*
            this.timeXTurn = 0;
            this.timeYTurn = 0;
            this.corrX = false;
            this.corrY = false;
            this.correctTurn = false;

            */

            //goBack
            //this.comOutBuf = "G";
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
            //this.simulationStartFlag = true;
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
                if (this.currentAngle - this.startTargetAngle > 3.0)
                {
                    if(this.direction == Direction.forward)
                        comOutBuf = "N" + this.roverSpeed.ToString() + "F";
                    else if(this.direction == Direction.right)
                        comOutBuf = "N" + this.roverSpeed.ToString() + "R";
                    else if (this.direction == Direction.backwards)
                        comOutBuf = "N" + this.roverSpeed.ToString() + "B";
                    else if (this.direction == Direction.left)
                        comOutBuf = "N" + this.roverSpeed.ToString() + "L";

                    this.adjusting = true;
                }
                else if (this.currentAngle - this.startTargetAngle < -3.0)
                {
                    if (direction == Direction.forward)
                        comOutBuf = "M" + this.roverSpeed.ToString() + "F";
                    else if (direction == Direction.right)
                        comOutBuf = "M" + this.roverSpeed.ToString() + "R";
                    else if (direction == Direction.backwards)
                        comOutBuf = "M" + this.roverSpeed.ToString() + "B";
                    else if (direction == Direction.left)
                        comOutBuf = "M" + this.roverSpeed.ToString() + "L";

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

        /*
        if (adjusting)
        {
            this.countCs++;
        }
        if(this.countCs >= 10)
        {
            if (this.timeRotateLeft > 0)
            {
               if(this.direction == Direction.forward)
                    comOutBuf = "Q" + this.roverSpeed.ToString() + "03" + "04"; ////overflow buffer
               else if(this.direction == Direction.backwards)
                    comOutBuf = "E" + this.roverSpeed.ToString() + "03" + "04"; ////overflow buffer
               this.timeRotateLeft--;
            }

            if (this.timeRotateRight > 0)
            {
                if (this.direction == Direction.forward)
                   comOutBuf = "Q" + this.roverSpeed.ToString() + "09" + "04"; // overflow buffer
                else if (this.direction == Direction.backwards)
                    comOutBuf = "E" + this.roverSpeed.ToString() + "09" + "04"; // overflow buffer
                this.timeRotateRight--;
            }
            this.countCs = 0;
        }
        if (this.timeRotateLeft == 0 && this.timeRotateRight == 0)
        {
            if (this.currentAngle - startTargetAngle > 10.0)
            {
                this.adjusting = true;
                this.countCs = 0;
                this.angleRotate = Math.Abs(this.currentAngle - startTargetAngle);
                this.timeRotateLeft = (int)(angleRotate / (float)2.3); // tempo in decimi di secondo                        
            }
            else if (this.currentAngle - startTargetAngle < -10.0)
            {
                this.adjusting = true;
                this.countCs = 0;
                this.angleRotate = Math.Abs(this.currentAngle - startTargetAngle);
                this.timeRotateRight = (int)(angleRotate / (float)2.3); // tempo in decimi di secondo

            }
            else if (this.currentAngle - startTargetAngle < 10.0 && this.currentAngle - startTargetAngle > -10.0 && adjusting) //flag adjusting per non intasare il buffer
            {
                if(this.direction == Direction.forward) comOutBuf = "F" + this.roverSpeed.ToString();
                else if(this.direction == Direction.right) comOutBuf = "R" + this.roverSpeed.ToString();
                else if (this.direction == Direction.backwards) comOutBuf = "B" + this.roverSpeed.ToString();
                else if (this.direction == Direction.left) comOutBuf = "L" + this.roverSpeed.ToString();
                else comOutBuf = "X" + this.roverSpeed.ToString();
                //this.testRunning = false;
                //this.timerCS.Enabled = false;
                this.countCs = 0;
                this.adjusting = false;
                //this.comOutBuf = "X\n";
            }
        }
        
    }         
        }
    */
        private void manageEndTurn()
        {
            double vx, vy;

            int speed = this.roverSpeed * 10; ///test prima senza convertire
            int direction = this.directionRov * 30;
            double angRate = this.angularSpeedRov / 10;
            int timeTurn = this.countTurning;  // *50 ms
            double rad_per_deg = Math.PI / 180;

            vx = speed * Math.Cos(direction * rad_per_deg);
            vy = speed * Math.Sin(direction * rad_per_deg);

            //variabili globali dx e dy riferite alla velocità minima (10) e moltiplicate per il tempo (50ms)
            //dx = vx *(10/vx) * timeTurn ::::: -> timeDx = dx / roverSpeed
            //problema : potrò muovermi solo a velocità multiple di 10

            //this.timeXTurn += timeTurn * 10 / vx;
            //this.timeYTurn += timeTurn * 10 / vy;

            //non posso renderli valori assoluti perchè non punziona per gestire più svolte di seguito
            this.timeXTurn += timeTurn * vx / speed;
            this.timeYTurn = timeTurn * vy / speed;

            //azzeramento dei tempi dopo la funzione
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
