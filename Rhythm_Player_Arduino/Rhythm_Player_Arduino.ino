
byte G = 1 << 0; // Green
byte R = 1 << 1; // Red
byte Y = 1 << 2; // Yellow
byte B = 1 << 3; // Blue
byte O = 1 << 4; // Orange
byte S = 1 << 5; // Strum
byte P = 1 << 6; // Power / Back
byte M = 1 << 7; // Meta Command

// Meta Commands
byte START  = 1 << 0; // Start Button (Controller One)
byte PLAYER = 1 << 1; // Player Change (Next byte is the number of players.
byte M3 = 1 << 2; // Meta 3
byte M4 = 1 << 3; // Meta 4
byte M5 = 1 << 4; // Meta 5
byte M6 = 1 << 5; // Meta 6

int players = 1;

int GREEN  = 0;
int RED    = 1;
int YELLOW = 2;
int BLUE   = 3;
int ORANGE = 4;
int STRUM  = 5;
int POWER  = 6;
int pins[3][7] = {
	{ 12, 11, 10, 9,  8,  7,  4 }, // Player One 
	{ 14, 15, 16, 17, 18, 19, 44}, // Player Two
	{ 7,  6,  5,  4,  3,  2,  45} // Player Three
};
int startPin = 3;

//int whammy = 0;
//int whammyWait = 100;
//int whammyTime = whammyWait;
//int dir = 1;
bool settingPlayers = false;
int player = 0;
  
void setup()
{
  Serial.begin(9600);
  
  for (int i = 0; i < 3; i++) {
    for (int o = 0; o < 7; o++) {
      pinMode(pins[i][o], OUTPUT);
      digitalWrite(pins[i][o], LOW);  
    }
  }
  
  pinMode(startPin, OUTPUT);
  digitalWrite(startPin, LOW);  

  //pinMode(wPin, OUTPUT);
  //analogWrite(wPin, whammy);
}
 
void loop()
{

  while(Serial.available()) {
    if (player >= players) {
      player = 0;
    }
    
    byte keys = Serial.read();
    if(settingPlayers) {
      
      settingPlayers = false;
      players = keys;
      //Serial.print("Players now set to ");
      //Serial.println(players);
      player = 0;
        return;
    }

      //Serial.print("Player ");
      //Serial.print(player+1);
      //Serial.print(" of ");
      //Serial.println(players);
      //Serial.print("Keys: ");
      //Serial.println(keys);


      if (keys & M) {
        //Serial.print("META: ");
        //Serial.println(keys);
        if (keys & START) {
          digitalWrite(startPin, HIGH);
          delay(40);
          digitalWrite(startPin, LOW);
          //Serial.println("Start Pressed.");
        } else if (keys & PLAYER) {
          settingPlayers = true;
          //Serial.println("Update players triggered.");  		
        }
      } else {
        digitalWrite(pins[player][GREEN],  (keys & G) ? HIGH : LOW);
        digitalWrite(pins[player][RED],    (keys & R) ? HIGH : LOW);
        digitalWrite(pins[player][YELLOW], (keys & Y) ? HIGH : LOW);
        digitalWrite(pins[player][BLUE],   (keys & B) ? HIGH : LOW);
        digitalWrite(pins[player][ORANGE], (keys & O) ? HIGH : LOW);
        digitalWrite(pins[player][STRUM],  (keys & S) ? HIGH : LOW);
        digitalWrite(pins[player][POWER],  (keys & P) ? HIGH : LOW);
        //Serial.print("Player ");  
        //Serial.print(player);
        //Serial.print(" keys: ");
        //Serial.print(keys);
        //Serial.println();

    }
        player++;
  }
}

// Not using whammy now... but we can add it back in later.
/*  if(whammyTime-- <= 0) {  
      if (dir == 1) {
          whammy++;
          if (whammy >= 255) {
              dir = 0;
              //analogWrite(wPin, 255);
          }
      } else {
          whammy--;
          if (whammy <= 0) {
              dir = 1;
              //analogWrite(wPin, 0);
          }
      }
      analogWrite(wPin, whammy); 
      whammyTime = whammyWait;
  }
}*/
