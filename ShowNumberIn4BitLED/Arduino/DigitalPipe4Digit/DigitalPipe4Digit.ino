#define CLEAR_COMMAND "CLEAR"
#define LOOP_COMMAND "LOOP"
#define SHOW_NUM_COMMAND "NUM"
#define NUM_LENGTH 3
#define SPLIT_CHAR ';'

#define RECEIVE_STAGE 0   // 接收命令阶段
#define EXECUTE_STAGE 1   // 执行命令阶段
#define OBSERVE_STAGE 2   // 观察阶段

#define MAX_CHARS 49

char buffer[MAX_CHARS + 1];
int charIndex = 0;

int currentStage;

int numIndex = 16;
const unsigned char LED8Pin[] = {
  2, 3, 4, 5, 6, 7, 8, 9
};//A B C D E F G Dp
const int TestNum[] = {12, 13, 10, 11};
const unsigned char LED8Code[] = {
  0x3F,    // 0
  0x06,    // 1
  0x5B,    // 2
  0x4F,    // 3
  0x66,    // 4
  0x6D,    // 5
  0x7D,    // 6
  0x07,    // 7
  0x7F,    // 8
  0x6F,    // 9
  0x77,    // A
  0x7C,    // B
  0x39,    // C
  0x5E,    // D
  0x79,    // E
  0x71     // F
};
//这个根据楼主提供的软件得来的值
void setup() {

  Serial.begin(9600);

  InitStatus();

  currentStage = RECEIVE_STAGE;
  Serial.println("Ready");
}
int a;
void loop() {
  //  Serial.print("loop:currentStage=");
  //  Serial.println(currentStage);

  switch (currentStage)
  {
    case RECEIVE_STAGE:
      ReceiveCommand();
      break;

    case EXECUTE_STAGE:
      if (Serial.available() > 0)
      {
        currentStage = RECEIVE_STAGE;
      }

      if (strcmp(buffer, CLEAR_COMMAND) == 0)
      {
        DarkLED();
      }
      else if (strcmp(buffer, LOOP_COMMAND) == 0)
      {
        LoopNum();
        currentStage = OBSERVE_STAGE;
      }
      else if (strncmp(buffer, SHOW_NUM_COMMAND, NUM_LENGTH) == 0)
      {
        char* pInt = &buffer[NUM_LENGTH + 1];
        ShowNum(atoi(pInt));
      }
      break;

    case OBSERVE_STAGE:
      if (Serial.available() > 0)
      {
        currentStage = RECEIVE_STAGE;
      }
      else
      {
        LoopNum();
      }

      break;
  }
}

void InitStatus()
{
  int i;

  for (i = 0; i < 4; i++)
  {
    pinMode(TestNum[i], OUTPUT);
    digitalWrite(TestNum[i], HIGH);
  }

  for (i = 0; i < 8; i++)
  {
    pinMode(LED8Pin[i], OUTPUT);
    digitalWrite(LED8Pin[i], HIGH);
  }
}

int TestNumIndex = 0;
int LedIndex = 0;

void LoopNum()
{
  digitalWrite(TestNum[TestNumIndex], LOW);
  LED8Show(LedIndex);
  delay(500);
  digitalWrite(TestNum[TestNumIndex], HIGH);

  LedIndex++;
  if (LedIndex >= numIndex)
  {
    LedIndex = 0;
    TestNumIndex++;
  }

  if (TestNumIndex >= 4)
  {
    TestNumIndex = 0;
  }
}
void ShowNum(int num)
{
  int mode = num;

  for (int j = 0; (j < 4) && (mode > 0); j++) {
    digitalWrite(TestNum[j], LOW);
    LED8Show(mode % 10);
    delay(1);
    mode = mode / 10;
    digitalWrite(TestNum[j], HIGH);
  }
}
void DarkLED()
{
  int i;

  for (i = 0; i < 4; i++)
  {
    digitalWrite(TestNum[i], HIGH);
  }

  for (i = 0; i < 8; i++)
  {
    digitalWrite(LED8Pin[i], LOW);
  }
}

void LED8Show(int data) {
  int i;
  int j;//j的值是0或1，代表LOW或HIGH
  char hc;//8位的比特值
  if (0 <= data < 16)
  {
    hc = LED8Code[data];
    for (i = 0; i < 8; i++)
    {
      j = bitRead(hc, i); //取8位比特值的第i位，0或1
      digitalWrite(LED8Pin[i], j); //让第i位代表的灯亮或灭
    }
  }
}

void ReceiveCommand()
{
  if (Serial.available() > 0)
  {
    char ch = Serial.read();

    if ((charIndex < MAX_CHARS) && (ch != SPLIT_CHAR))
    {
      buffer[charIndex++] = ch;
    }
    else
    {
      buffer[charIndex] = 0;
      charIndex = 0;
      currentStage = EXECUTE_STAGE;

      Serial.print("received command is ");
      Serial.println(buffer);
    }
  }
}
