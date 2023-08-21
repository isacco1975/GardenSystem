/*
 * IGP WATERING II, Arduino UNO
 * Author:        Isaac Garcia Peveri
 * Copyrhight:    2022 Isaac Garcia Peveri
 * License:       Free Distributable and Editable
 * Date Written:  2022/11/06 (Project Start)
 * Date Compiled: 2022/11/11 (Poland Independence Day)
*/

   #include <SPI.h>
   #include <Adafruit_GFX.h>
   #include <Adafruit_PCD8544.h>

   Adafruit_PCD8544 NOKIA_5110_LCD = Adafruit_PCD8544(5, 4, 3);

   const int PIN_RELAY_1 = A5;
   const int MANUAL_WATER_BUTTON = 8;
   const int MANUAL_MODE_BUTTON = 2;
   const int MODE_2DAY_BUTTON = A0;
   const int MODE_7DAY_BUTTON = A1;
   const int MODE_14DAY_BUTTON = A2;
   const int BUZZER = 7;

   int manualWaterButtonValue = 0;

   int manualModeButtonValue = 0;
   int oldManualModeValue = 0;

   int mode2ButtonValue = 1;
   int oldMode2Value = 1;

   int mode7ButtonValue = 0;
   int oldMode7Value = 0;

   int mode14ButtonValue = 0;
   int oldMode14Value = 0;

   int asteriscNextPosition = 0;
   int stepWateringCycle = 0;
   int wateringCycleSecondsCount = 0;

   long mainCycleSecondsCount = 0;
   long maxTime = 172800; //2 days (DEFAULT)

   static const unsigned char PROGMEM IGP_LOGO [] =
   {
      B11111111, 111111111, 111111111, B11111111, B11111111, B11111111, B11111111, B11111111,
      B10000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000001,
      B10000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000001,
      B10000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000001,
      B10000000, B00000000, B11111111, B00011111, B11110000, B01111111, B11110000, B00000001,
      B10000000, B00000000, B11111111, B00111111, B11111100, B01111111, B11110000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B00000000, B01110000, B00111000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B00000000, B01110000, B00111000, B00000001,

      B10000000, B00000000, B00011000, B01110000, B00000000, B01110000, B00111000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B00000000, B01110000, B00111000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B00000000, B01110100, B11111000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B11111000, B01111111, B11110000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B01111100, B01110000, B00000000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B00011100, B01110000, B00000000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B00011100, B01110000, B00000000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B00011100, B01110000, B00000000, B00000001,

      B10000000, B00000000, B00011000, B01110000, B00011100, B01110000, B00000000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B00011100, B01110000, B00000000, B00000001,
      B10000000, B00000000, B00011000, B01110000, B00111100, B01110000, B00000000, B00000001,
      B10000000, B00000000, B11111111, B01111111, B11111000, B01110000, B00000000, B00000001,
      B10000000, B00000000, B11111111, B00111111, B11111000, B01110000, B00000000, B00000001,
      B10000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000001,
      B10000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000000, B00000001,
      B11111111, B11111111, B11111111, B11111111, B11111111, B11111111, B11111111, B11111111
   };

   void InitialLogo()
   {
      NOKIA_5110_LCD.drawBitmap(4  , 0, IGP_LOGO, 64, 24, 1);
      NOKIA_5110_LCD.setCursor(6, 30);
      NOKIA_5110_LCD.setTextSize(1);
      NOKIA_5110_LCD.setTextColor(BLACK);
      NOKIA_5110_LCD.println("IGP WATERING");
      NOKIA_5110_LCD.display();
   }

   void DisplayFixedData()
   {
      NOKIA_5110_LCD.clearDisplay();
      NOKIA_5110_LCD.fillRect(0,  0, 84, 10, BLACK);
      NOKIA_5110_LCD.drawRect(0, 10, 84, 30, BLACK);
      NOKIA_5110_LCD.drawRect(0, 30, 84, 9, BLACK);
      NOKIA_5110_LCD.drawRect(0, 39, 84, 9, BLACK);
      NOKIA_5110_LCD.setCursor(0, 2);
      NOKIA_5110_LCD.setTextSize(1);
      NOKIA_5110_LCD.setTextColor(WHITE);
      NOKIA_5110_LCD.print("IGP Watering 2");
   }

   void DisplayCountdownTimer()
   {
      NOKIA_5110_LCD.setTextSize(1,1);
      NOKIA_5110_LCD.setCursor(40, 13);
      NOKIA_5110_LCD.print(maxTime - mainCycleSecondsCount);
      NOKIA_5110_LCD.display();

      NOKIA_5110_LCD.setTextSize(2,1);
      NOKIA_5110_LCD.setCursor(31, 22);
    
      if (oldMode2Value == 1) 
      {
         NOKIA_5110_LCD.print("2DAY");
      }     

      if (oldMode7Value == 1) 
      {
         NOKIA_5110_LCD.print("7DAY");
      }     

      if (oldMode14Value == 1) 
      {
         NOKIA_5110_LCD.print("14DY");
      }     

      NOKIA_5110_LCD.display();
      NOKIA_5110_LCD.setTextSize(1,1);
   }

   void CheckManualWater()
   {
       if (manualWaterButtonValue == 1)
       {
          tone(BUZZER, 500);
          digitalWrite(PIN_RELAY_1, HIGH);

          DisplayFixedData();

          NOKIA_5110_LCD.setTextColor(BLACK);
          NOKIA_5110_LCD.setTextSize(1,2);
          NOKIA_5110_LCD.setCursor(11, 13);
          NOKIA_5110_LCD.print("WATER OPEN");
          NOKIA_5110_LCD.display();
       }
       else
       {
          digitalWrite(PIN_RELAY_1, LOW);
          tone(BUZZER, 350);
          delay(1);
          noTone(BUZZER);
       }
   }

   void OpenWaterAutomatically()
   {
      if (wateringCycleSecondsCount > maxTime)
      {
         tone(BUZZER, 500);
         digitalWrite(PIN_RELAY_1, HIGH);

         DisplayFixedData();

         stepWateringCycle = 0;

         while (stepWateringCycle < 15) // Open water for 15 seconds
         {
            asteriscNextPosition = asteriscNextPosition + 5;

            NOKIA_5110_LCD.setTextSize(1);
            NOKIA_5110_LCD.setCursor(asteriscNextPosition, 30);
            NOKIA_5110_LCD.setTextColor(BLACK);
            NOKIA_5110_LCD.print("*");
            NOKIA_5110_LCD.display();

            stepWateringCycle++;

            NOKIA_5110_LCD.setCursor(3, 40);
            NOKIA_5110_LCD.setTextColor(BLACK);
            NOKIA_5110_LCD.print("In cycle");

            delay(1000);

            DisplayCountdownTimer();

            NOKIA_5110_LCD.setTextSize(1);
            NOKIA_5110_LCD.setCursor(3, 13);
            NOKIA_5110_LCD.print("Water");
            NOKIA_5110_LCD.setCursor(3, 20);
            NOKIA_5110_LCD.print("Open");
            NOKIA_5110_LCD.display();
         }

         wateringCycleSecondsCount = 0;
      }
      else
      {
        NOKIA_5110_LCD.setTextSize(1);
        NOKIA_5110_LCD.setCursor(3, 40);
        NOKIA_5110_LCD.setTextColor(BLACK);
        NOKIA_5110_LCD.print("Waiting...");
      }
   }

   void ReadButtonValues()
   {
      manualWaterButtonValue = digitalRead(MANUAL_WATER_BUTTON);
      manualModeButtonValue = digitalRead(MANUAL_MODE_BUTTON);
      mode2ButtonValue = digitalRead(MODE_2DAY_BUTTON);
      mode7ButtonValue = digitalRead(MODE_7DAY_BUTTON);
      mode14ButtonValue = digitalRead(MODE_14DAY_BUTTON);    
   }
   
   void CheckButtonStatuses()
   {
	    if (mode2ButtonValue == 1)
      {
         oldMode7Value = 0;
         oldMode14Value = 0;
         mode7ButtonValue = 0;
         mode14ButtonValue = 0;
         oldMode2Value = 1 - oldMode2Value;
      }

      if (mode7ButtonValue == 1)
      {
         oldMode2Value = 0;
         oldMode14Value = 0;
         mode2ButtonValue = 0;
         mode14ButtonValue = 0;
         oldMode7Value = 1 - oldMode7Value;
      }

      if (mode14ButtonValue == 1)
      {
         oldMode2Value = 0;
         oldMode7Value = 0;
         mode2ButtonValue = 0;
         mode7ButtonValue = 0;
         oldMode14Value = 1 - oldMode14Value;
      }

      if (manualModeButtonValue == 1)
      {
         oldManualModeValue = 1 - oldManualModeValue;
      }

      if (oldMode2Value == 1)
      {
         maxTime = 172800;
      } 

      if (oldMode7Value == 1)
      {
         maxTime = 604800;
      }

      if (oldMode14Value == 1)
      {
         maxTime = 604800 * 2;
      }

      if (oldManualModeValue == 1)
      {
         DisplayFixedData();

         NOKIA_5110_LCD.setTextColor(BLACK);
         NOKIA_5110_LCD.setTextSize(1,2);
         NOKIA_5110_LCD.setCursor(7, 13);
         NOKIA_5110_LCD.print("MANUAL MODE");
         NOKIA_5110_LCD.display();
         NOKIA_5110_LCD.setTextSize(1);
      } 
	    else
      {
        mainCycleSecondsCount++;
        wateringCycleSecondsCount++;

        DisplayFixedData();

        NOKIA_5110_LCD.setTextColor(BLACK);
        NOKIA_5110_LCD.setTextSize(2);
        NOKIA_5110_LCD.setCursor(40, 13);

        DisplayCountdownTimer();

        if (mainCycleSecondsCount > maxTime)
        {
           mainCycleSecondsCount = 0;
        }

        OpenWaterAutomatically();

        NOKIA_5110_LCD.setTextSize(1,2);
        NOKIA_5110_LCD.setCursor(3, 13);
        NOKIA_5110_LCD.print("AUTO");
        NOKIA_5110_LCD.display();
        NOKIA_5110_LCD.setTextSize(1);

        asteriscNextPosition = 0;
        stepWateringCycle = 0;		  
	    } 	  
   }

   void setup()
   {
      pinMode(PIN_RELAY_1, OUTPUT);
      pinMode(MANUAL_WATER_BUTTON, INPUT);
      pinMode(MANUAL_MODE_BUTTON, INPUT);
      pinMode(MODE_2DAY_BUTTON, INPUT);
      pinMode(MODE_7DAY_BUTTON, INPUT);
      pinMode(MODE_14DAY_BUTTON, INPUT);
      pinMode(BUZZER, OUTPUT);

      NOKIA_5110_LCD.begin();
      NOKIA_5110_LCD.setContrast(25);
      NOKIA_5110_LCD.clearDisplay();

      InitialLogo();

      delay(4000);
   }

   void loop()
   {
      ReadButtonValues();
	    CheckButtonStatuses();
      CheckManualWater();

      delay(1000);
   }
