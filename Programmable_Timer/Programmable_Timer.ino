/*
 * PROIGRAMMABLE TIMER I, Arduino UNO
 * Author:        Isaac Garcia Peveri
 * Copyrhight:    2023 Isaac Garcia Peveri
 * License:       Free Distributable and Editable
 * Date Written:  2023/03/04 (Project Start)
 * Date Compiled: xxxx/xx/xx ()
 * Last Edit:     20.08.2023: Added arduino Nano for more Relays
*/

   #include <SPI.h>
   #include <Adafruit_GFX.h>
   #include <Adafruit_PCD8544.h>
   #include <SoftwareSerial.h>

   Adafruit_PCD8544 NOKIA_5110_LCD = Adafruit_PCD8544(5, 4, 3);
   SoftwareSerial IGP_Serial = SoftwareSerial(6, 8);

   const int PIN_RELAY_1 = A5;
   const int PIN_RELAY_2 = A3;
   const int MANUAL_WATER_BUTTON = A0;
   const int MANUAL_MODE_BUTTON = 2;
   const int MODE_2DAY_BUTTON = 0;
   const int MODE_7DAY_BUTTON = A1;
   const int MODE_14DAY_BUTTON = 9;
   const int BUZZER = 7;
   const int BL = 10;
   const float defaultTime = 15;

   bool manualWaterButtonValue = true;

   bool HoursButtonValue = true;
   bool oldHoursButtonValue = true;

   bool MinsButtonValue = false;
   bool oldMinsButtonValue = false;

   bool SecsButtonValue = true;
   bool oldSecsButtonValue = true;

   bool DayButtonValue = true;
   bool oldDayButtonValue = true;

   int asteriscNextPosition = 0;
   int stepWateringCycle = 0;
   int wateringCycleSecondsCount = 0;

   long mainCycleSecondsCount = 0;
   long maxTime = defaultTime; // 3600 * 3; //3 hours

   int Hrs = 0; int Mins = 0; int Secs = 0;
   int SetHrs = 0; int SetMins = 0; int SetSecs = 59;
   int SetDay = 1; int DaysPassed = 0;

   void DisplayActualTimeClock()
   {
      NOKIA_5110_LCD.setTextSize(1,1);     
      NOKIA_5110_LCD.setCursor(32, 13);

      if(Hrs < 10)
      {
        NOKIA_5110_LCD.print("0");
      }

      NOKIA_5110_LCD.print(Hrs);
      NOKIA_5110_LCD.print(":");

      if(Mins < 10)
      {
        NOKIA_5110_LCD.print("0");
      }

      NOKIA_5110_LCD.print(Mins);
      NOKIA_5110_LCD.print(":");

      if(Secs < 10)
      {
        NOKIA_5110_LCD.print("0");
      }
      NOKIA_5110_LCD.print(Secs);
      NOKIA_5110_LCD.print(" ");

      //...Clock starts...
      Secs++;
      
      TimeFormalCheck();
   }

   void TimeFormalCheck()
   {
      if (Secs == 60) {
         Mins++;
         Secs = 0;
      }
      if (Mins == 60) {
         Mins = 0;
         Hrs++;
      }
      if (Hrs > 23) {
          Hrs = 0;
          DaysPassed++;
      }
      if (SetSecs == 60) {
         SetMins++;
         SetSecs = 0;
      }
      if (SetMins == 60) {
         SetMins = 0;
         SetHrs++;
      }
      if (SetHrs > 23) {
          SetHrs = 0;
      }
      //Day
      if(SetDay > 31) {
        SetDay = 1;
      }
   }

   void DisplaySetTime()
   {
      NOKIA_5110_LCD.setCursor(32, 22);

      if(SetHrs < 10)
      {
        NOKIA_5110_LCD.print("0");
      }

      NOKIA_5110_LCD.print(SetHrs);
      NOKIA_5110_LCD.print(":");

      if(SetMins < 10)
      {
        NOKIA_5110_LCD.print("0");
      }

      NOKIA_5110_LCD.print(SetMins);
      NOKIA_5110_LCD.print(":");

      if(SetSecs < 10)
      {
        NOKIA_5110_LCD.print("0");
      }
      NOKIA_5110_LCD.print(SetSecs);
      NOKIA_5110_LCD.print(" ");

      TimeFormalCheck();
   }

   void DisplayFixedData()
   {
      NOKIA_5110_LCD.clearDisplay();
      NOKIA_5110_LCD.fillRect(0,  0, 84, 10, BLACK);
      NOKIA_5110_LCD.drawRect(0, 10, 84, 30, BLACK);
      NOKIA_5110_LCD.drawRect(25, 10, 59, 21, BLACK);
      NOKIA_5110_LCD.drawRect(0, 30, 84, 9, BLACK);
      NOKIA_5110_LCD.drawRect(0, 39, 84, 9, BLACK);
      NOKIA_5110_LCD.setCursor(0, 2);
      NOKIA_5110_LCD.setTextSize(1);
      NOKIA_5110_LCD.setTextColor(WHITE);
      NOKIA_5110_LCD.print(" IGP SUPER TM");
   }

   void DisplayCountdownTimer()
   {
      DisplayActualTimeClock();
      DisplaySetTime();

      NOKIA_5110_LCD.setTextSize(1,1);
   }

   // Black button //
   void CheckManualWater()
   {
       if (manualWaterButtonValue == false)
       {
          //Sending message to arduino NANO
          IGP_Serial.write("ON", 2);

          tone(BUZZER, 15000);
          delay(50);
          noTone(BUZZER);

          digitalWrite(PIN_RELAY_1, HIGH);
          digitalWrite(PIN_RELAY_2, HIGH);
          digitalWrite(BL, HIGH);

          DisplayFixedData();

          NOKIA_5110_LCD.setContrast(45);
          NOKIA_5110_LCD.setTextColor(BLACK);
          NOKIA_5110_LCD.setTextSize(1,2);
          NOKIA_5110_LCD.setCursor(11, 13);
          NOKIA_5110_LCD.print("MANUAL CYCL");
          NOKIA_5110_LCD.display();
       }
       else
       {
          digitalWrite(PIN_RELAY_1, LOW);
          digitalWrite(PIN_RELAY_2, LOW);
          digitalWrite(BL, LOW);

          tone(BUZZER, 20000);
          delay(1);
          noTone(BUZZER);

          NOKIA_5110_LCD.setContrast(20);
          NOKIA_5110_LCD.setTextSize(1);
          NOKIA_5110_LCD.setCursor(3, 40);
          NOKIA_5110_LCD.setTextColor(BLACK);
          NOKIA_5110_LCD.print("Waiting...");
          NOKIA_5110_LCD.display();
       }
   }

   //Cycle 
   void OpenWaterAutomatically()
   {
       NOKIA_5110_LCD.setContrast(45);

       tone(BUZZER, 25000);

       digitalWrite(PIN_RELAY_1, HIGH);
       digitalWrite(PIN_RELAY_2, HIGH);
       digitalWrite(BL, HIGH);

       DisplayFixedData();

       stepWateringCycle = 0;
       int asteriscNum = 0;

       while (stepWateringCycle < maxTime) // Open water for 15 seconds
       {
          //Sending message to arduino NANO
          IGP_Serial.write("ON", 2);

          asteriscNum++;
          if (asteriscNum < 15)
          {
             asteriscNextPosition = asteriscNextPosition + 5;
             NOKIA_5110_LCD.setTextSize(1);
             NOKIA_5110_LCD.setCursor(asteriscNextPosition, 30);
             NOKIA_5110_LCD.setTextColor(BLACK);
             NOKIA_5110_LCD.print("*");
             NOKIA_5110_LCD.display();              
          } 
          else
          { 
             DisplayFixedData();
             asteriscNextPosition = 0;
             asteriscNum = 0;
          }

          stepWateringCycle++;

          NOKIA_5110_LCD.setCursor(3, 40);
          NOKIA_5110_LCD.setTextColor(BLACK);
          NOKIA_5110_LCD.print("In cycle");

          delay(999);
          DisplayCountdownTimer();

          NOKIA_5110_LCD.setTextSize(1);
          NOKIA_5110_LCD.setCursor(3, 13);
          NOKIA_5110_LCD.print("ON");
          NOKIA_5110_LCD.display();
       }

       wateringCycleSecondsCount = 0;
   }

   void ReadButtonValues()
   {
      manualWaterButtonValue = digitalRead(MANUAL_WATER_BUTTON);
      HoursButtonValue = digitalRead(MANUAL_MODE_BUTTON);
      MinsButtonValue = digitalRead(MODE_2DAY_BUTTON);
      SecsButtonValue = digitalRead(MODE_7DAY_BUTTON);
      DayButtonValue = digitalRead(MODE_14DAY_BUTTON); 

      Serial.println("");
      Serial.println("=========================");
      Serial.println(manualWaterButtonValue);
      Serial.println(HoursButtonValue);
      Serial.println(MinsButtonValue);
      Serial.println(SecsButtonValue);
      Serial.println(DayButtonValue);
      Serial.println("=========================");
      Serial.println("");
   }
   
   void CheckButtonStatuses()
   {
      //BLUE BUTTON: Set Hour
      if (HoursButtonValue == false)
      {
         SetHrs++;
      }

      //GREEN BUTTON: Set Minute
      if (MinsButtonValue == false)
      {
         SetMins++;
      } 

      //YELLOW BUTTON: Set Second
      if (SecsButtonValue == false)
      {
         SetSecs++;
      }

      //RED BUTTON: Set Day of Month from 1 to 31
      if (DayButtonValue == false)
      {
         SetDay++;
      }

      DisplayFixedData();3;
 
      NOKIA_5110_LCD.setContrast(20);
      NOKIA_5110_LCD.setTextColor(BLACK);
      NOKIA_5110_LCD.setTextSize(2);
      NOKIA_5110_LCD.setCursor(40, 13);
 
      DisplayActualTimeClock();
      DisplaySetTime();

      if (mainCycleSecondsCount > maxTime)
      {
         mainCycleSecondsCount = 0;
      }

      if (DaysPassed > 0) 
      {
         if (DaysPassed % SetDay == 0 && Hrs == SetHrs && SetMins == Mins && SetSecs == Secs)
         {
            OpenWaterAutomatically();
            DaysPassed = 0;
         }
      }

      NOKIA_5110_LCD.setTextSize(1,2);
      NOKIA_5110_LCD.setCursor(8, 14);

      if(SetDay < 10) 
      {
         NOKIA_5110_LCD.print("0");
      }

      NOKIA_5110_LCD.print(SetDay);
      NOKIA_5110_LCD.display();
      NOKIA_5110_LCD.setTextSize(1);

      asteriscNextPosition = 0;
      stepWateringCycle = 0;		

      CheckManualWater();
   }

   void setup()
   {
      Serial.begin(9600); //DEBUG
      
      pinMode(PIN_RELAY_1, OUTPUT);
      pinMode(PIN_RELAY_2, OUTPUT);
      pinMode(MANUAL_WATER_BUTTON, INPUT);
      pinMode(MANUAL_MODE_BUTTON, INPUT);
      pinMode(MODE_2DAY_BUTTON, INPUT);
      pinMode(MODE_7DAY_BUTTON, INPUT);
      pinMode(MODE_14DAY_BUTTON, INPUT);
      pinMode(BUZZER, OUTPUT);
      
      pinMode(BL, OUTPUT);

      NOKIA_5110_LCD.begin();
      NOKIA_5110_LCD.setContrast(20);
      NOKIA_5110_LCD.clearDisplay();

      IGP_Serial.begin(9600);
   }

   void loop()
   {
      ReadButtonValues();
	   CheckButtonStatuses();

      delay(950);
   }
