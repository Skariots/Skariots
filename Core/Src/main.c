#include "main.h"

#define RXSIZEBUF 128
#define DEVICE_ADDRESS 0x34
#define OFFSET_TO_SPEED 0.20
#define PI 3.14159265

void SystemClock_Config(void);
static void MPU_Config(void);
static void MX_GPIO_Init(void);
static void UART_Init(void);
void Timer_Init();
void I2C_Init();
void Init_RovBuf(uint8_t commandBufOutRov[4][RXSIZEBUF]);
int TransmitCommand(uint8_t commandBufOutRov[][RXSIZEBUF], int stopFlag);
int checkCommandRov(char *str, int *time, int *speed,int *direction_rov, float *angular_rate_rov);
void TranslateCommand(char *str,uint8_t comBufOutRov[][RXSIZEBUF], int speed,int direction_rov, float angular_rate_rov);
void setVelocity(int velocity, int direction, float angular_rate, int backFlag);
void speedConvert(float *v_s);
int checkCommand(char *str);
void goBack();

typedef enum state{ off = 0, on = 1} State;
typedef enum direction{forward = 0, right = 1, backwards = 2, left = 3, none = 4} Direction;

uint8_t comINbuf[RXSIZEBUF];
int numBytesIN;
uint8_t comOUTbuf[RXSIZEBUF];
int numBytesOUT;
int numBytesTransmitted;
int timeout;
int errorCommand;
int ledNumber;
int mode;
int sysClkSpeed;
int noReset2 = 0, noReset3 = 0;
int speedArray[3] = {'x','x','x'};
State ledState[3] = {off, off, off};
int status = 0;

int errorCommandRover;
int moveTick = 0;
int timeRov = 0;

int speedRov = 0;
float angular_rate_rov= 0; //per la funzione che si occupa di sterzare (da convertire da 0 a 9 a valori rad/s)
int direction_rov = 0;
int timeCountRov = 0; //tempo per far fermare i comandi
char bufCheck[RXSIZEBUF];
uint8_t commandBufOutRov[4][RXSIZEBUF];
int bytesData = 3;
int bytesI2CTransmitted = 0;
int messageTransmitted = 0;

int impulseCountEngForw;
int impulseCountEngRight;
int impulseResetFlag;
int commandReceivedFlag;  //flag di interruzione se si riceve un comando durante il goBack()
int checkRx;
Direction roverDirection;

int bufOutReady;

typedef struct{
  float a;
  float b;
  float velocity;
  int direction;
  float angular_rate;
  float wheel_diameter;
  float pulse_per_cycle;
  float speed_to_mm;
}RoverInfo;

RoverInfo rover;

uint8_t comOUTbufImpulse1[RXSIZEBUF];
uint8_t comOUTbufImpulse3[RXSIZEBUF];

TIM_HandleTypeDef htim1;
TIM_HandleTypeDef htim2;
TIM_HandleTypeDef htim3;
TIM_HandleTypeDef htim4;
UART_HandleTypeDef huart2;
UART_HandleTypeDef huart3;
I2C_HandleTypeDef hi2c1;


int main(void)
{
  MPU_Config();
  HAL_Init();

  // Configure the system clock
  SystemClock_Config();

  // Initialize all configured peripherals
  MX_GPIO_Init();
  
  UART_Init();
  Timer_Init();
  I2C_Init();
  Init_RovBuf(commandBufOutRov);
  TransmitCommand(commandBufOutRov,1);
  
  sysClkSpeed = HAL_RCC_GetSysClockFreq();
  
  rover.a = 193;
  rover.b = 219;
  rover.wheel_diameter = 96;
  rover.pulse_per_cycle = 4 * 11 * 131;
  rover.speed_to_mm = 5.375;
  rover.velocity = 0;
  rover.direction = 0;
  //circonferenza 301 mm
  // costante 5.375 
  // 56 sec per 10 giri speed 1 -> 53.75 mm/s
  // 28 sec per 10 giri speed 2 -> 107.5 mm/s
  // 18 sec per 10 giri speed 3 -> 167 mm/s
  // 14 sec per 10 giri speed 4 -> 215 mm/s
  // 11.5 sec per 10 giri speed 5 -> 261.7 mm/s
  // 9.5 sec per 10 giri speed 6 -> 316.8 mm/s
  // 8.1 sec per 10 giri speed 7 -> 371.6 mm/s
  // 8.1 sec per 10 giri speed 8
  // 8.1 sec per 10 giri speed 9

  timeout=numBytesIN=numBytesOUT=numBytesTransmitted=0;
  impulseCountEngForw = impulseCountEngRight = 0;
//  timePrintImpulse = 0;
  impulseResetFlag = 0;
  checkRx = 0;
  commandReceivedFlag = 0;
  roverDirection = none;
  bufOutReady = 1;
  
  HAL_UART_Receive_IT(&huart3,comINbuf,1); //riceve il primo byte e fa scattare interrupt
  
  while (1)
  {
//    if(timePrintImpulse > 1000 && impulseCountEngForw > 0){
//      sprintf(comOUTbufImpulse1,"ENG1:%d\n",impulseCountEngForw);
//      HAL_UART_Transmit_IT(&huart3,(uint8_t *)comOUTbufImpulse1,strlen(comOUTbufImpulse1));
//      HAL_Delay(10);
//      sprintf(comOUTbufImpulse3,"ENG3:%d\n",impulseCountEngForw);
//      HAL_UART_Transmit_IT(&huart3,(uint8_t *)comOUTbufImpulse3,strlen(comOUTbufImpulse3));
//      timePrintImpulse = 0;
//    }
    if(moveTick >= 100 && numBytesIN > 0){ // > 1 per il \n aggiunto su seriale
      if(comINbuf[strlen(comINbuf)-1] == '\n') comINbuf[strlen(comINbuf)-1] = '\0'; // strip del \n
      //strcpy(comOUTbuf,comINbuf);
      sprintf(comOUTbuf,"c%s\nf%d\nr%d\n",comINbuf,impulseCountEngForw,impulseCountEngRight);
      strcpy(bufCheck,comINbuf);
      errorCommandRover = checkCommandRov(bufCheck,&timeRov,&speedRov,&direction_rov,&angular_rate_rov);  
      HAL_UART_Transmit_IT(&huart3,(uint8_t *)comOUTbuf,strlen(comOUTbuf));
      //timer start timeRov
      if(!errorCommandRover){
        TranslateCommand(comINbuf,commandBufOutRov,speedRov,direction_rov,angular_rate_rov);
        TransmitCommand(commandBufOutRov,0);
      }
      moveTick = 0;
      numBytesIN = 0;
      for(int i=0;i<10;i++)
        comINbuf[i] = '\0';
      
      //strcpy(comINbuf,"");
    }
    
    if(numBytesIN >= 10){  // > 10 per non entrare con i comandi del rover
      //comINbuf[3]=0; //ha problemi con il buffer per la rotazione
      errorCommand = checkCommand(comINbuf);
      if(errorCommand == 0){
        sprintf(comOUTbuf,"\n%s OK\n",(char *)comINbuf);
        HAL_UART_Transmit_IT(&huart3,(uint8_t *)comOUTbuf,strlen(comOUTbuf));
        //HAL_I2C_Master_Transmit_IT(&hi2c1,0x34 << 1,comBufOutRov,sizeof(uint8_t) * 2);
      }
      else{
        sprintf(comOUTbuf,"\n%s ERROR\n",(char *)comINbuf);
        HAL_UART_Transmit_IT(&huart3,(uint8_t *)comOUTbuf,1);
        //HAL_I2C_Master_Transmit_IT(&hi2c1,0x34 << 1,commandBufOut,sizeof(uint8_t) * 2);
        numBytesTransmitted = 1;
      }
      numBytesIN = 0;
    }
//    else if (numBytesIN>0)
//    {   // numero caratteri insufficiente
//        if (timeout>10)
//        {
//            numBytesIN = 0;
//            strcpy(comINbuf,"");
//            timeout=0;
//        }
//    }
//    else
//    {   // niente su seriale
//      timeout=0;
//    }
    if(speedArray[0] == 's' && ledState[0] == off){
        HAL_GPIO_WritePin(LED1_GPIO_Port, LED1_Pin,GPIO_PIN_SET);
        ledState[0] = on;
      }
      if(speedArray[1] == 's' && ledState[1] == off){
        HAL_GPIO_WritePin(LED2_GPIO_Port, LED2_Pin,GPIO_PIN_SET);
        ledState[1] = on;
      }
      if(speedArray[2] == 's' && ledState[2] == off){
        HAL_GPIO_WritePin(LED3_GPIO_Port, LED3_Pin,GPIO_PIN_SET);
        ledState[2] = on;
      }
      if(speedArray[0] == 'x' && ledState[0] == on){
        HAL_GPIO_WritePin(LED1_GPIO_Port, LED1_Pin,GPIO_PIN_RESET);
        ledState[0] = off;
      }
      if(speedArray[1] == 'x' && ledState[1] == on){
        HAL_GPIO_WritePin(LED2_GPIO_Port, LED2_Pin,GPIO_PIN_RESET);
        ledState[1] = off;
      }
      if(speedArray[2] == 'x' && ledState[2] == on){
        HAL_GPIO_WritePin(LED3_GPIO_Port, LED3_Pin,GPIO_PIN_RESET);
        ledState[2] = off;
      }

  }
  /* USER CODE END 3 */
}
void Init_RovBuf(uint8_t comBufOutRov[][RXSIZEBUF]){
  comBufOutRov[0][0] = 51;
  comBufOutRov[1][0] = 52;
  comBufOutRov[2][0] = 53;
  comBufOutRov[3][0] = 54;
  
  comBufOutRov[0][1] = 0;
  comBufOutRov[1][1] = 0;
  comBufOutRov[2][1] = 0;
  comBufOutRov[3][1] = 0;
}

int checkCommandRov(char *str,int *time, int *speed,int *direction_rov, float *angular_rate_rov){
  
  //F->forward R->reverse L->left R->right X->stop G->go back Z -> rotate left C -> rotate right
  int error = 0;
  char tmpStr[10];
  int value_angle = 0;
  char speedStr[10];
  
  
  if(str[0] == 'X'){
    if(strlen(str) > 1){
      if(str[1] == 'R'){
        impulseResetFlag = 1;
      }
    }
    return error;
  }
  
  if(str[0] == 'G'){
    return error;
  }
  
  if(strlen(str) < 2){
      error = 1;
      return error; 
  }
  if(str[0] != 'F' && str[0] != 'R' && str[0] != 'L' && str[0] != 'B' && str[0] != 'X' 
     && str[0] != 'Z' && str[0] != 'C' && str[0] != 'Y' && str[0] != 'J' && str[0] != 'U' && str[0] != 'H' && str[0] != 'N' && str[0] != 'M' && str[0] != 'Q' && str[0] != 'E')  {
    error = 1;
    return error;
  }
  strncpy(speedStr,str+1,1);
  speedStr[1] = '\0';
  if(((*speed) = atoi(speedStr)) == 0){
    error = 1;
    return error;
  }
  
  //modifica da parametro tempo a direzione (0 - 360) gradi
  //1 -> +30 gradi, 12 -> 360 gradi
  if(str[0] == 'Q' || str[0] == 'E'){
    if(strlen(str) < 6){
      error = 1;
      return error;
    }
    strncpy(tmpStr,str+2,2);
    tmpStr[2] = '\0';
    if(((value_angle) = atoi(tmpStr)) == 0){
      error = 1;
      return error;
    }
    *direction_rov = value_angle * 30;
    
    
    strncpy(tmpStr,str+4,2);
    tmpStr[2] = '\0';
    if(((*angular_rate_rov) = atof(tmpStr)) == 0){
      error = 1;
      return error;
    }
  }
//  if(strlen(str) == 2){
//    *time = 9999;
//  }
//  else{
//    strncpy(timeStr,str+2,4);
//    timeStr[4] = '\0';
//    if(((*time) = atoi(timeStr)) == 0){
//      error = 1;
//      return error;
//    }
//  }
  return error;
}

void TranslateCommand(char *str,uint8_t comBufOutRov[][RXSIZEBUF],int speed, int direction_rov, float angular_rate_rov){
  //speed valori da 0 a 9 -> solo valori positivi della velocità 0-> 130 9->255
  
  //0 -> dx dietro, 1 -> dx avanti, 2 -> sx dietro, 3 -> sx avanti
  int realSpeed = (speed * 10) % 91;
  
  if(str[0] == 'G'){
    goBack();
    comBufOutRov[0][1] = 0;
    comBufOutRov[1][1] = 0;
    comBufOutRov[2][1] = 0;
    comBufOutRov[3][1] = 0;
    return;
  }
  if(str[0] == 'F'){
    comBufOutRov[0][1] = realSpeed;
    comBufOutRov[1][1] = -1 * realSpeed;
    comBufOutRov[2][1] = -1 * realSpeed;
    comBufOutRov[3][1] = realSpeed;
  }
  if(str[0] == 'B'){
    comBufOutRov[0][1] = -1 * realSpeed;
    comBufOutRov[1][1] = realSpeed;
    comBufOutRov[2][1] = realSpeed;
    comBufOutRov[3][1] = -1 * realSpeed;
  }
  if(str[0] == 'L'){
    comBufOutRov[0][1] = -1 * realSpeed;
    comBufOutRov[1][1] = -1 * realSpeed;
    comBufOutRov[2][1] = -1 * realSpeed;
    comBufOutRov[3][1] = -1 * realSpeed;
  }
  if(str[0] == 'R'){
    comBufOutRov[0][1] = realSpeed;
    comBufOutRov[1][1] = realSpeed;
    comBufOutRov[2][1] = realSpeed;
    comBufOutRov[3][1] = realSpeed;
  }
  
  //rotazione a sinistra
  if(str[0] == 'Z'){
    comBufOutRov[0][1] = realSpeed;
    comBufOutRov[1][1] = -1 * realSpeed;
    comBufOutRov[2][1] = realSpeed;
    comBufOutRov[3][1] = -1 * realSpeed;
  }
  
  //rotazione a destra
  if(str[0] == 'C'){
    comBufOutRov[0][1] = -1 * realSpeed;
    comBufOutRov[1][1] = realSpeed;
    comBufOutRov[2][1] = -1 * realSpeed;
    comBufOutRov[3][1] = realSpeed;
  }
  
  //avanti sx in diagonale
  if(str[0] == 'Y'){
    comBufOutRov[0][1] = 0;
    comBufOutRov[1][1] = -1 * realSpeed;
    comBufOutRov[2][1] = -1 * realSpeed;
    comBufOutRov[3][1] = 0;
  }
  
  //avanti dx in diagonale
  if(str[0] == 'U'){
    comBufOutRov[0][1] = realSpeed;
    comBufOutRov[1][1] = 0;
    comBufOutRov[2][1] = 0;
    comBufOutRov[3][1] = realSpeed;
  }
  
  //indietro dx in diagonale
  if(str[0] == 'J'){
    comBufOutRov[0][1] = 0;
    comBufOutRov[1][1] = realSpeed;
    comBufOutRov[2][1] = realSpeed;
    comBufOutRov[3][1] = 0;
  }
  
  //indietro xs in diagonale
  if(str[0] == 'H'){
    comBufOutRov[0][1] = -1 * realSpeed;
    comBufOutRov[1][1] = 0;
    comBufOutRov[2][1] = 0;
    comBufOutRov[3][1] = -1 * realSpeed;
  }
  
  //stabilizza direzione verso sinistra
  if(str[0] == 'N'){
    comBufOutRov[0][1] = realSpeed + (OFFSET_TO_SPEED * realSpeed);
    comBufOutRov[1][1] = (-1 * realSpeed) - (OFFSET_TO_SPEED * realSpeed);
    comBufOutRov[2][1] = -1 * realSpeed;
    comBufOutRov[3][1] = realSpeed;
  }
  
  //stabilizza direzione verso destra
  if(str[0] == 'M'){
    comBufOutRov[0][1] = realSpeed;
    comBufOutRov[1][1] = -1 * realSpeed;
    comBufOutRov[2][1] = (-1 * realSpeed) - (OFFSET_TO_SPEED * realSpeed);
    comBufOutRov[3][1] = realSpeed + (OFFSET_TO_SPEED * realSpeed);
  }
  
  if(str[0] == 'X'){
    comBufOutRov[0][1] = 0;
    comBufOutRov[1][1] = 0;
    comBufOutRov[2][1] = 0;
    comBufOutRov[3][1] = 0;
  }
  
  //rotazione con velocità angolare
  if(str[0] == 'Q'){
    //convert angular_rate
    angular_rate_rov /= 10;
    setVelocity(realSpeed * rover.speed_to_mm,direction_rov,angular_rate_rov,0);
  }
  
  if(str[0] == 'E'){
    //convert angular_rate
    angular_rate_rov /= 10;
    setVelocity(realSpeed * rover.speed_to_mm,direction_rov,angular_rate_rov,1);
  }
  
}


void setVelocity(int velocity, int direction, float angular_rate, int backFlag){
  float rad_per_deg;
  float vx,vy,vp,v1,v2,v3,v4;
  float v_s[4];
  
  if(direction > 180){
    velocity = -velocity;
    angular_rate = -angular_rate;
  }
  
  if(backFlag){
    velocity = -velocity;
  }
        
   rad_per_deg = PI / 180;
   vx = velocity * cos(direction * rad_per_deg);
   vy = velocity * sin(direction * rad_per_deg);
   vp = angular_rate * (rover.a/2 + rover.b/2);
   
   v1 = vy - vx + vp; //avanti dx
   v2 = vy + vx - vp; //avanti sx
   v3 = vy - vx - vp; //dietro sx
   v4 = vy + vx + vp; //dietro dx
   
   //vedere la corrispondenza delle ruote  
   v_s[0] = v4;
   v_s[1] = -v1;
   v_s[2] = -v3;
   v_s[3] = v2;
   
   for(int i=0;i<4;i++){
     speedConvert(&v_s[i]);
   }
   
   rover.velocity = velocity;
   rover.direction = direction;
   rover.angular_rate = angular_rate;
   
   commandBufOutRov[0][1] = v_s[0];
   commandBufOutRov[1][1] = v_s[1];
   commandBufOutRov[2][1] = v_s[2];
   commandBufOutRov[3][1] = v_s[3];
    
   //a -> dist orizzontale
   //b ->dist verticale
}

void speedConvert(float *speed){
  (*speed) /= 5.375;
  *speed = (int)(*speed);
}
  
int TransmitCommand(uint8_t commandBufOutRov[][RXSIZEBUF], int stopFlag){
      uint8_t bufOut[8];
      if(stopFlag) {
        Init_RovBuf(commandBufOutRov);
      }
      
      bufOut[0] = commandBufOutRov[0][0];
      bufOut[1] = commandBufOutRov[0][1];
      bufOut[2] = commandBufOutRov[1][1];
      bufOut[3] = commandBufOutRov[2][1];
      bufOut[4] = commandBufOutRov[3][1];
      
      
      HAL_I2C_Master_Transmit_IT(&hi2c1,0x34 << 1,bufOut,sizeof(uint8_t) * 8); 
      
      if(impulseResetFlag) {
        impulseCountEngForw = 0;
        impulseCountEngRight = 0;
        impulseResetFlag = 0;
      }
      
      
      ////////////da modificare 
      if(bufOut[1] > 90 && bufOut[3] > 90) roverDirection = left;
      else if(bufOut[1] > 0 && bufOut[3] > 90) roverDirection = forward;
      else if(bufOut[1] > 90 && bufOut[3] > 0) roverDirection = backwards;  
      else if(bufOut[1] > 0 && bufOut[3] > 0) roverDirection = right;
      else roverDirection = none;
      /////////////////////////
      
}
int checkCommand(char *str){
  errorCommand = 0;
  if(str[0] != 'L'){
    errorCommand = 1;
    return errorCommand;
  }
  ledNumber = atoi(&(str[1]));
  if(ledNumber < 1 || ledNumber > 3) {
    errorCommand = 1;
    return errorCommand;
  }
  //s->set, l->low, h->high
  if(str[2] != 's' && str[2] != 'l' && str[2] != 'h' && str[2] != 'x'){
    errorCommand = 1;
    return errorCommand;
  }
  mode = str[2];
  speedArray[ledNumber-1] = mode;
  
  return errorCommand;
}

void goBack(){
  checkRx = 1; // se si riceve un comando durante il goBack si interrompe il ritorno e si resettano gli impulsi
  if(impulseCountEngForw > 0){
    TranslateCommand("B2",commandBufOutRov,2,0,0);
    TransmitCommand(commandBufOutRov,0);
    while(impulseCountEngForw > 0 && !commandReceivedFlag);
    TransmitCommand(commandBufOutRov,1); //stop movement
  }
  else if(impulseCountEngForw < 0){
    TranslateCommand("F2",commandBufOutRov,2,0,0);
    TransmitCommand(commandBufOutRov,0);
    while(impulseCountEngForw < 0 && !commandReceivedFlag);
    TransmitCommand(commandBufOutRov,1); //stop movement
  }
  
  if(impulseCountEngRight > 0){
    TranslateCommand("L2",commandBufOutRov,2,0,0);
    TransmitCommand(commandBufOutRov,0);
    while(impulseCountEngRight > 0 && !commandReceivedFlag);
    TransmitCommand(commandBufOutRov,1); //stop movement
  }
  else if(impulseCountEngRight < 0){
    TranslateCommand("R2",commandBufOutRov,2,0,0);
    TransmitCommand(commandBufOutRov,0);
    while(impulseCountEngRight < 0 && !commandReceivedFlag);
    TransmitCommand(commandBufOutRov,1); //stop movement
  }
  // azzeramento impulsi nelle due direzioni
  impulseCountEngForw = 0;
  impulseCountEngRight = 0;
  checkRx = 0;
  commandReceivedFlag = 0;
}

void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart) {

    uint8_t tmp = 0;
    if (huart->Instance == USART2) {
      tmp=USART2->RDR;
      if(numBytesIN < RXSIZEBUF){
        comINbuf[numBytesIN++] = tmp;
      }
      HAL_UART_Receive_IT(huart, &tmp,1);
    }
    if (huart->Instance == USART3) {
      
      HAL_UART_Receive_IT(huart, &tmp,1);

      tmp=USART3->RDR;
      if(checkRx){
        if(tmp != 'G' && tmp != '\n'){
          commandReceivedFlag = 1;
        }        
        return;
      }
      
      if(numBytesIN < RXSIZEBUF){
        comINbuf[numBytesIN++] = tmp;
        moveTick = 0;
      }
    }
}

void HAL_UART_TxCpltCallback(UART_HandleTypeDef *huart) {
  uint8_t tmp = 0;
  if (huart->Instance == USART2){
    if(numBytesTransmitted < numBytesOUT){
      tmp = comOUTbuf[numBytesTransmitted++];
      HAL_UART_Transmit_IT(huart,&tmp,1);
    }
    else if(numBytesTransmitted >= numBytesOUT) numBytesTransmitted = 0;
  }
  
  if (huart->Instance == USART3){
    if(numBytesTransmitted < numBytesOUT){
      tmp = comOUTbuf[numBytesTransmitted++];
      HAL_UART_Transmit_IT(huart,&tmp,1);
    }
    else if(numBytesTransmitted >= numBytesOUT) numBytesTransmitted = 0;
  }
}

// Interrupt Handler per USART2
void USART2_IRQHandler(void) {
  HAL_UART_IRQHandler(&huart2);
}

void USART3_IRQHandler(void) {
  HAL_UART_IRQHandler(&huart3);
} 

void TIM1_IRQHandler(void) {
    HAL_TIM_IRQHandler(&htim1); 
}

void TIM2_IRQHandler(void) {
    HAL_TIM_IRQHandler(&htim2); 
}

void TIM3_IRQHandler(void) {
    HAL_TIM_IRQHandler(&htim3); 
}

void TIM4_IRQHandler(void) {
    HAL_TIM_IRQHandler(&htim4); 
}

void I2C1_EV_IRQHandler(void) {
    HAL_I2C_EV_IRQHandler(&hi2c1);
}

void EXTI3_IRQHandler(void)
{
    HAL_GPIO_EXTI_IRQHandler(GPIO_PIN_3);
}

void EXTI4_IRQHandler(void)
{
    HAL_GPIO_EXTI_IRQHandler(GPIO_PIN_4);
}

void HAL_I2C_MasterTxCpltCallback(I2C_HandleTypeDef *hi2c) {
    if (hi2c->Instance == I2C1) {
        messageTransmitted = 1;
    }
}


void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim) {
    if (htim->Instance == TIM2 && (speedArray[0] == 'l' || speedArray[1] == 'l' || speedArray[2] == 'l')) {
      //controlla tutte le combinazioni con il timer2
      noReset2 = 0;
      if(speedArray[0] == 'l' && ledState[0] == off){
        HAL_GPIO_WritePin(LED1_GPIO_Port, LED1_Pin,GPIO_PIN_SET);
        ledState[0] = on;
        noReset2 = 1;
      }
      if(speedArray[1] == 'l' && ledState[1] == off){
        HAL_GPIO_WritePin(LED2_GPIO_Port, LED2_Pin,GPIO_PIN_SET);
        ledState[1] = on;
        noReset2 = 1;
      }
      if(speedArray[2] == 'l' && ledState[2] == off){
        HAL_GPIO_WritePin(LED3_GPIO_Port, LED3_Pin,GPIO_PIN_SET);
        ledState[2] = on;
        noReset2 = 1;
      }
      if(noReset2 == 1) return;
      if(speedArray[0] == 'l' && ledState[0] == on){
        HAL_GPIO_WritePin(LED1_GPIO_Port, LED1_Pin,GPIO_PIN_RESET);
        ledState[0] = off;
      }
      if(speedArray[1] == 'l' && ledState[1] == on){
        HAL_GPIO_WritePin(LED2_GPIO_Port, LED2_Pin,GPIO_PIN_RESET);
        ledState[1] = off;
      }
      if(speedArray[2] == 'l' && ledState[2] == on){
        HAL_GPIO_WritePin(LED3_GPIO_Port, LED3_Pin,GPIO_PIN_RESET);
        ledState[2] = off;
      }
    }
    if (htim->Instance == TIM3 && (speedArray[0] == 'h' || speedArray[1] == 'h' || speedArray[2] == 'h')) {
        //controlla tutte le combinazioni con il timer3
      noReset3 = 0;
      if(speedArray[0] == 'h' && ledState[0] == off){
        HAL_GPIO_WritePin(LED1_GPIO_Port, LED1_Pin,GPIO_PIN_SET);
        ledState[0] = on;
        noReset3 = 1;
      }
      if(speedArray[1] == 'h' && ledState[1] == off){
        HAL_GPIO_WritePin(LED2_GPIO_Port, LED2_Pin,GPIO_PIN_SET);
        ledState[1] = on;
        noReset3 = 1;
      }
      if(speedArray[2] == 'h' && ledState[2] == off){
        HAL_GPIO_WritePin(LED3_GPIO_Port, LED3_Pin,GPIO_PIN_SET);
        ledState[2] = on;
        noReset3 = 1;
      }
      if(noReset3 == 1) return;
      if(speedArray[0] == 'h' && ledState[0] == on){
        HAL_GPIO_WritePin(LED1_GPIO_Port, LED1_Pin,GPIO_PIN_RESET);
        ledState[0] = off;
      }
      if(speedArray[1] == 'h' && ledState[1] == on){
        HAL_GPIO_WritePin(LED2_GPIO_Port, LED2_Pin,GPIO_PIN_RESET);
        ledState[1] = off;
      }
      if(speedArray[2] == 'h' && ledState[2] == on){
        HAL_GPIO_WritePin(LED3_GPIO_Port, LED3_Pin,GPIO_PIN_RESET);
        ledState[2] = off;
      }
    }
    if(htim->Instance == TIM4){
      timeout++;
      moveTick++;
      timeCountRov++;
//      timePrintImpulse++;
    }
}

void HAL_GPIO_EXTI_Callback(uint16_t GPIO_Pin)
{
    if (GPIO_Pin == GPIO_PIN_3 && HAL_GPIO_ReadPin(ENG_GPIO_Port,GPIO_Pin) == GPIO_PIN_SET && roverDirection != none)
    {
      if(roverDirection == forward){
        impulseCountEngForw++;
      }
      else if(roverDirection == backwards){
        impulseCountEngForw--;
      }
      else if(roverDirection == right){
        impulseCountEngRight++;
      }
      else if(roverDirection == left){
        impulseCountEngRight--;
      }
    }
}

/**
  * @brief System Clock Configuration
  * @retval None
  */
void SystemClock_Config(void)
{
  RCC_OscInitTypeDef RCC_OscInitStruct = {0};
  RCC_ClkInitTypeDef RCC_ClkInitStruct = {0};

  /** Supply configuration update enable
  */
  HAL_PWREx_ConfigSupply(PWR_LDO_SUPPLY);

  /** Configure the main internal regulator output voltage
  */
  __HAL_PWR_VOLTAGESCALING_CONFIG(PWR_REGULATOR_VOLTAGE_SCALE3);

  while(!__HAL_PWR_GET_FLAG(PWR_FLAG_VOSRDY)) {}

  /** Initializes the RCC Oscillators according to the specified parameters
  * in the RCC_OscInitTypeDef structure.
  */
  RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSI;
  RCC_OscInitStruct.HSIState = RCC_HSI_DIV1;
  RCC_OscInitStruct.HSICalibrationValue = RCC_HSICALIBRATION_DEFAULT;
  RCC_OscInitStruct.PLL.PLLState = RCC_PLL_NONE;
  if (HAL_RCC_OscConfig(&RCC_OscInitStruct) != HAL_OK)
  {
    Error_Handler();
  }

  /** Initializes the CPU, AHB and APB buses clocks
  */
  RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK|RCC_CLOCKTYPE_SYSCLK
                              |RCC_CLOCKTYPE_PCLK1|RCC_CLOCKTYPE_PCLK2
                              |RCC_CLOCKTYPE_D3PCLK1|RCC_CLOCKTYPE_D1PCLK1;
  RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_HSI;
  RCC_ClkInitStruct.SYSCLKDivider = RCC_SYSCLK_DIV1;
  RCC_ClkInitStruct.AHBCLKDivider = RCC_HCLK_DIV1;
  RCC_ClkInitStruct.APB3CLKDivider = RCC_APB3_DIV1;
  RCC_ClkInitStruct.APB1CLKDivider = RCC_APB1_DIV1;
  RCC_ClkInitStruct.APB2CLKDivider = RCC_APB2_DIV1;
  RCC_ClkInitStruct.APB4CLKDivider = RCC_APB4_DIV1;

  if (HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_1) != HAL_OK)
  {
    Error_Handler();
  }
}

/**
  * @brief GPIO Initialization Function
  * @param None
  * @retval None
  */
static void MX_GPIO_Init(void)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};
/* USER CODE BEGIN MX_GPIO_Init_1 */
/* USER CODE END MX_GPIO_Init_1 */

  /* GPIO Ports Clock Enable */
  __HAL_RCC_GPIOB_CLK_ENABLE();
  __HAL_RCC_GPIOD_CLK_ENABLE();
  __HAL_RCC_GPIOE_CLK_ENABLE();
  __HAL_RCC_GPIOA_CLK_ENABLE();
  __HAL_RCC_GPIOC_CLK_ENABLE();

  /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(LED3_GPIO_Port, LED3_Pin, GPIO_PIN_RESET);

  /*Configure GPIO pin : LED3_Pin */
  GPIO_InitStruct.Pin = LED3_Pin;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(LED3_GPIO_Port, &GPIO_InitStruct);
  
    /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(LED2_GPIO_Port, LED2_Pin, GPIO_PIN_RESET);

  /*Configure GPIO pin : LED2_Pin */
  GPIO_InitStruct.Pin = LED2_Pin;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(LED2_GPIO_Port, &GPIO_InitStruct);
  
    /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(LED1_GPIO_Port, LED1_Pin, GPIO_PIN_RESET);

  /*Configure GPIO pin : LED1_Pin */
  GPIO_InitStruct.Pin = LED1_Pin;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(LED1_GPIO_Port, &GPIO_InitStruct);
  
  GPIO_InitStruct.Pin = ENG1_Pin;
  GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(ENG_GPIO_Port, &GPIO_InitStruct);
  
  HAL_NVIC_SetPriority(EXTI3_IRQn, 2, 0);
  HAL_NVIC_EnableIRQ(EXTI3_IRQn);
  
  GPIO_InitStruct.Pin = ENG3_Pin;
  GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(ENG_GPIO_Port, &GPIO_InitStruct);
  
  HAL_NVIC_SetPriority(EXTI4_IRQn, 2, 0);
  HAL_NVIC_EnableIRQ(EXTI4_IRQn);
 
}

void MPU_Config(void)
{
  MPU_Region_InitTypeDef MPU_InitStruct = {0};

  /* Disables the MPU */
  HAL_MPU_Disable();

  /** Initializes and configures the Region and the memory to be protected
  */
  MPU_InitStruct.Enable = MPU_REGION_ENABLE;
  MPU_InitStruct.Number = MPU_REGION_NUMBER0;
  MPU_InitStruct.BaseAddress = 0x0;
  MPU_InitStruct.Size = MPU_REGION_SIZE_4GB;
  MPU_InitStruct.SubRegionDisable = 0x87;
  MPU_InitStruct.TypeExtField = MPU_TEX_LEVEL0;
  MPU_InitStruct.AccessPermission = MPU_REGION_NO_ACCESS;
  MPU_InitStruct.DisableExec = MPU_INSTRUCTION_ACCESS_DISABLE;
  MPU_InitStruct.IsShareable = MPU_ACCESS_SHAREABLE;
  MPU_InitStruct.IsCacheable = MPU_ACCESS_NOT_CACHEABLE;
  MPU_InitStruct.IsBufferable = MPU_ACCESS_NOT_BUFFERABLE;

  HAL_MPU_ConfigRegion(&MPU_InitStruct);
  /* Enables the MPU */
  HAL_MPU_Enable(MPU_PRIVILEGED_DEFAULT);

}

static void UART_Init(void){
  
    //__HAL_RCC_GPIOD_CLK_ENABLE();
    __HAL_RCC_USART2_CLK_ENABLE();
    __HAL_RCC_USART3_CLK_ENABLE();
    
    GPIO_InitTypeDef GPIO_InitStruct = {0};

    GPIO_InitStruct.Pin = UART_RX_Pin | UART_TX_Pin;
    GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;  // Alternate function push-pull
    GPIO_InitStruct.Pull = GPIO_PULLUP;  // Resistenza di pull-up interna
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
    GPIO_InitStruct.Alternate = GPIO_AF7_USART2;  // Funzione alternativa UART2
    HAL_GPIO_Init(UART_GPIO_Port, &GPIO_InitStruct);
    
    huart2.Instance = USART2;
    huart2.Init.BaudRate = 115200;  // Baud rate
    huart2.Init.WordLength = UART_WORDLENGTH_8B;
    huart2.Init.StopBits = UART_STOPBITS_1;
    huart2.Init.Parity = UART_PARITY_NONE;
    huart2.Init.Mode = UART_MODE_TX_RX;  // Abilita trasmissione e ricezione
    huart2.Init.HwFlowCtl = UART_HWCONTROL_NONE;
    huart2.Init.OverSampling = UART_OVERSAMPLING_16;

    GPIO_InitStruct.Pin = UART3_RX_Pin | UART3_TX_Pin;
    GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;  // Alternate function push-pull
    GPIO_InitStruct.Pull = GPIO_PULLUP;  // Resistenza di pull-up interna
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
    GPIO_InitStruct.Alternate = GPIO_AF7_USART3;  // Funzione alternativa UART2
    HAL_GPIO_Init(UART_GPIO_Port, &GPIO_InitStruct);
    
    huart3.Instance = USART3;
    huart3.Init.BaudRate = 115200;  // Baud rate
    huart3.Init.WordLength = UART_WORDLENGTH_8B;
    huart3.Init.StopBits = UART_STOPBITS_1;
    huart3.Init.Parity = UART_PARITY_NONE;
    huart3.Init.Mode = UART_MODE_TX_RX;  // Abilita trasmissione e ricezione
    huart3.Init.HwFlowCtl = UART_HWCONTROL_NONE;
    huart3.Init.OverSampling = UART_OVERSAMPLING_16;
    
    HAL_NVIC_SetPriority(USART2_IRQn, 1, 0); // Priorità dell'interrupt
    HAL_NVIC_EnableIRQ(USART2_IRQn);
    HAL_NVIC_SetPriority(USART3_IRQn, 1, 0); // Priorità dell'interrupt
    HAL_NVIC_EnableIRQ(USART3_IRQn);

    if (HAL_UART_Init(&huart2) != HAL_OK) {
        // Errore nell'inizializzazione
        while (1);
    }
    if (HAL_UART_Init(&huart3) != HAL_OK) {
        // Errore nell'inizializzazione
        while (1);
    }
}
void Timer_Init(){
  __HAL_RCC_TIM2_CLK_ENABLE();  // Abilita il clock per TIM2
  __HAL_RCC_TIM3_CLK_ENABLE();  // Abilita il clock per TIM3
  __HAL_RCC_TIM4_CLK_ENABLE();  // Abilita il clock per TIM4
  
  
  htim2.Instance = TIM2;
  htim2.Init.Prescaler = 6399;  // Divide il clock (10kHz se HCLK = 64MHz)
  htim2.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim2.Init.Period = 9999;  // 1 secondo (10kHz / 10000)
  htim2.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim2.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  
  htim3.Instance = TIM3;
  htim3.Init.Prescaler = 6399;  // Divide il clock (10kHz se HCLK = 64MHz)
  htim3.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim3.Init.Period = 4999;  // 0.5 secondi (10kHz / 5000)
  htim3.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim3.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  
  htim4.Instance = TIM4;
  htim4.Init.Prescaler = 6399;  // Divide il clock (10kHz se HCLK = 64MHz)
  htim4.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim4.Init.Period = 10;  // 1 millisecondo
  htim4.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim4.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  
  if (HAL_TIM_Base_Init(&htim2) != HAL_OK) {
      Error_Handler();
  }
  if (HAL_TIM_Base_Init(&htim3) != HAL_OK) {
      Error_Handler();
  }
  if (HAL_TIM_Base_Init(&htim4) != HAL_OK) {
      Error_Handler();
  } 

  HAL_NVIC_SetPriority(TIM2_IRQn, 3, 0);
  HAL_NVIC_EnableIRQ(TIM2_IRQn);
  HAL_NVIC_SetPriority(TIM3_IRQn, 2, 0);
  HAL_NVIC_EnableIRQ(TIM3_IRQn);
  HAL_NVIC_SetPriority(TIM4_IRQn, 0, 0);
  HAL_NVIC_EnableIRQ(TIM4_IRQn);
  
  HAL_TIM_Base_Start_IT(&htim2);
  HAL_TIM_Base_Start_IT(&htim3);
  HAL_TIM_Base_Start_IT(&htim4);
}

void I2C_Init(){
  //porta B già abilitata
  __HAL_RCC_I2C1_CLK_ENABLE();
  
  GPIO_InitTypeDef GPIO_InitStruct = {0};

    // Configura PB6 (SCL) e PB7 (SDA) come Alternate Function, Open Drain, Pull-up
    GPIO_InitStruct.Pin = I2C_SCL_Pin | I2C_SDA_Pin;
    GPIO_InitStruct.Mode = GPIO_MODE_AF_OD;
    GPIO_InitStruct.Pull = GPIO_PULLUP;
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
    GPIO_InitStruct.Alternate = GPIO_AF4_I2C1; // AF4 per I2C1

    HAL_GPIO_Init(I2C_GPIO_Port, &GPIO_InitStruct);
    
    //configura I2C
    hi2c1.Instance = I2C1;  // Seleziona I2C1
    hi2c1.Init.Timing = 0x00C0216C; // Valori per 100kHz
    hi2c1.Init.OwnAddress1 = 0;  // Non usiamo indirizzo slave
    hi2c1.Init.AddressingMode = I2C_ADDRESSINGMODE_7BIT;
    hi2c1.Init.DualAddressMode = I2C_DUALADDRESS_DISABLE;
    hi2c1.Init.OwnAddress2 = 0;
    hi2c1.Init.OwnAddress2Masks = I2C_OA2_NOMASK;
    hi2c1.Init.GeneralCallMode = I2C_GENERALCALL_DISABLE;
    hi2c1.Init.NoStretchMode = I2C_NOSTRETCH_DISABLE;
    
    HAL_I2C_Init(&hi2c1);
    
    __HAL_I2C_ENABLE_IT(&hi2c1, I2C_IT_TXI | I2C_IT_TCI); //buffer pronto | trasmissione completata
    
    HAL_NVIC_SetPriority(I2C1_EV_IRQn, 0, 0);
    HAL_NVIC_EnableIRQ(I2C1_EV_IRQn);

    if (HAL_I2C_Init(&hi2c1) != HAL_OK) {
        // Gestisci errore
        while (1);
    }
}

void Error_Handler(void)
{
  /* USER CODE BEGIN Error_Handler_Debug */
  /* User can add his own implementation to report the HAL error return state */
  __disable_irq();
  while (1)
  {
  }
  /* USER CODE END Error_Handler_Debug */
}

#ifdef  USE_FULL_ASSERT
/**
  * @brief  Reports the name of the source file and the source line number
  *         where the assert_param error has occurred.
  * @param  file: pointer to the source file name
  * @param  line: assert_param error line source number
  * @retval None
  */
void assert_failed(uint8_t *file, uint32_t line)
{
  /* USER CODE BEGIN 6 */
  /* User can add his own implementation to report the file name and line number,
     ex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */
  /* USER CODE END 6 */
}
#endif /* USE_FULL_ASSERT */
