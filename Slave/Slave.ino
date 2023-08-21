#include "SoftwareSerial.h"
char buff[3];
SoftwareSerial mserial(6, 8);
const int REL1 = 2;
const int REL2 = 3;
  
void setup() {
  // Begin the Serial at 9600 Baud
  
  Serial.begin(9600);
  Serial.println("Slave");
  mserial.begin(9600);
  pinMode(REL1, OUTPUT);
  pinMode(REL2, OUTPUT);
}

void loop()
{
  if (mserial.available() >= 5)
  {
    mserial.readBytes(buff, 3);
    
    Serial.println("Leido:");
    Serial.write(buff, 3);
    Serial.println();    

    digitalWrite(REL1, HIGH); digitalWrite(REL2, HIGH);

    delay(2000);
  }
  else
  {
    Serial.println("Esperando");
    digitalWrite(REL1, LOW); digitalWrite(REL2, LOW);
  }

  delay(250);
}
