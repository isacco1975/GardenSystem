
#define PIN_BUZZER 7
#define PIN_GREEN_LED 13

/*---------------------------------------
       IGP Garden System sketch 
    Author: Isaac Garcia Peveri
      Date Written: 06/07/2022
  ---------------------------------------
*/

const int PIN_RELAY_1 = A5;  
const int LowFrequencyByte = 80;
const int HiFrequencyByte = 100;
String val;

void setup()
{
    Serial.begin(9600);

    pinMode(PIN_GREEN_LED, OUTPUT);
    pinMode(PIN_BUZZER, OUTPUT);
    pinMode(PIN_RELAY_1, OUTPUT);
}

void First_Frequency() {
  
    for(int idx = 0; idx < LowFrequencyByte; idx++)
    {
      digitalWrite(PIN_BUZZER,HIGH);
      delay(10);
      
      digitalWrite(PIN_BUZZER,LOW);
      delay(10);    
    }
}

void Second_Frequency() {
    for(int idx = 0; idx < HiFrequencyByte; idx++)
    {
      digitalWrite(PIN_BUZZER, HIGH);
      delay(5); 
          
      digitalWrite(PIN_BUZZER, LOW);
      delay(5); 
    }
}

void MakeSound() 
{
    First_Frequency();
    Second_Frequency();
}

void StopSound() 
{
    noTone(PIN_BUZZER);
}

void loop()
{
    if (Serial.available())
    {
        val = Serial.readString();
    }

    Check_Relay_1();
}

void Check_Relay_1()
{
    if (val == "ON_1") //Cycle Start
    {
        MakeSound();

        digitalWrite(PIN_GREEN_LED, HIGH);
        digitalWrite(PIN_RELAY_1, HIGH);
    }
    else
    {
        if (val == "OFF_1") //Cycle stop
        {
            StopSound();

            digitalWrite(PIN_GREEN_LED, LOW);
            digitalWrite(PIN_RELAY_1, LOW);
        }
    }
}

//END   