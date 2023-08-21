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
   const int PIN_RELAY_2 = A3;
   const int MANUAL_WATER_BUTTON = A0;
   const int MANUAL_MODE_BUTTON = 2;
   const int MODE_2DAY_BUTTON = 0;
   const int MODE_7DAY_BUTTON = A1;
   const int MODE_14DAY_BUTTON = 9;
   const int BUZZER = 7;
   const int BL = 10;
   const float defaultTime = 3600 * 2;

   bool manualWaterButtonValue = true;

   bool manualModeButtonValue = true;
   bool oldManualModeValue = true;

   bool mode2ButtonValue = false;
   bool oldMode2Value = false;

   bool mode7ButtonValue = true;
   bool oldMode7Value = true;

   bool mode14ButtonValue = true;
   bool oldMode14Value = true;

   int asteriscNextPosition = 0;
   int stepWateringCycle = 0;
   int wateringCycleSecondsCount = 0;

   long mainCycleSecondsCount = 0;
   long maxTime = defaultTime; // 3600 * 3; //3 hours

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
      NOKIA_5110_LCD.println("IGP TIMER");
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
      NOKIA_5110_LCD.print("  IGP Timer I");
   }

   void DisplayCountdownTimer()
   {
      NOKIA_5110_LCD.setTextSize(1,1);
      NOKIA_5110_LCD.setCursor(40, 13);
      NOKIA_5110_LCD.print(maxTime - mainCycleSecondsCount);
      NOKIA_5110_LCD.display();

      NOKIA_5110_LCD.setTextSize(2,1);
      NOKIA_5110_LCD.setCursor(31, 22);
    
      if (oldMode2Value == false || mode2ButtonValue == false) 
      {
         NOKIA_5110_LCD.print("3Hrs");
         oldMode2Value = false;
      }     

      if (oldMode7Value == false || mode7ButtonValue == false) 
      {
         NOKIA_5110_LCD.print("6Hrs");
         oldMode7Value = false;
      }     

      if (oldMode14Value == false || mode14ButtonValue == false) 
      {
         NOKIA_5110_LCD.print("12Hr");
         oldMode14Value = false;
      }     

      NOKIA_5110_LCD.display();
      NOKIA_5110_LCD.setTextSize(1,1);
   }

   void CheckManualWater()
   {
       if (manualWaterButtonValue == false)
       {
          tone(BUZZER, 500);
          digitalWrite(PIN_RELAY_1, HIGH);
          digitalWrite(PIN_RELAY_2, HIGH);
          digitalWrite(BL, HIGH);

          DisplayFixedData();

          NOKIA_5110_LCD.setTextColor(BLACK);
          NOKIA_5110_LCD.setTextSize(1,2);
          NOKIA_5110_LCD.setCursor(11, 13);
          NOKIA_5110_LCD.print("PWR ON");
          NOKIA_5110_LCD.display();
       }
       else
       {
          digitalWrite(PIN_RELAY_1, HIGH);
          digitalWrite(PIN_RELAY_2, HIGH);
          digitalWrite(BL, HIGH);

          tone(BUZZER, 500);
          delay(1);
          noTone(BUZZER);
       }
   }

   void OpenWaterAutomatically()
   {
      if (wateringCycleSecondsCount > maxTime)
      {
         Serial.println("SONO QUI IMBECILLE");
         tone(BUZZER, 500);
         digitalWrite(PIN_RELAY_1, LOW);
         digitalWrite(PIN_RELAY_2, LOW);
         digitalWrite(BL, LOW);

         DisplayFixedData();

         stepWateringCycle = 0;
         int asteriscNum = 0;

         while (stepWateringCycle < maxTime) // Open water for 15 seconds
         {
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
               NOKIA_5110_LCD.setContrast(20);

               DisplayFixedData();
               asteriscNextPosition = 0;
               asteriscNum = 0;
            }

            stepWateringCycle++;

            NOKIA_5110_LCD.setCursor(3, 40);
            NOKIA_5110_LCD.setTextColor(BLACK);
            NOKIA_5110_LCD.print("In cycle");

            delay(500);

            DisplayCountdownTimer();

            NOKIA_5110_LCD.setTextSize(1);
            NOKIA_5110_LCD.setCursor(3, 13);
            NOKIA_5110_LCD.print("Power");
            NOKIA_5110_LCD.setCursor(3, 20);
            NOKIA_5110_LCD.print("off");
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

      Serial.println("");
      Serial.println("=========================");
      Serial.println(manualWaterButtonValue);
      Serial.println(manualModeButtonValue);
      Serial.println(mode2ButtonValue);
      Serial.println(mode7ButtonValue);
      Serial.println(mode14ButtonValue);
      Serial.println("=========================");
      Serial.println("");
   }
   
   void CheckButtonStatuses()
   {
      NOKIA_5110_LCD.setContrast(42);

	    if (mode2ButtonValue == false)
      {
         oldMode7Value = true;
         oldMode14Value = true;
         mode7ButtonValue = true;
         mode14ButtonValue = true;
         oldMode2Value = !oldMode2Value;
         oldManualModeValue = true;
      }

      if (mode7ButtonValue == false)
      {
         oldMode2Value = true;
         oldMode14Value = true;
         mode2ButtonValue = true;
         mode14ButtonValue = true;
         oldMode7Value = !oldMode7Value;
         oldManualModeValue = true;
      }

      if (mode14ButtonValue == false)
      {
         oldMode2Value = true;
         oldMode7Value = true;
         mode2ButtonValue = true;
         mode7ButtonValue = true;
         oldMode14Value = !oldMode14Value;
         oldManualModeValue = true;
      }
/*
      if (manualModeButtonValue == false)
      {
         oldManualModeValue = false;
      }
*/
      if (oldMode2Value == false)
      {
         maxTime = defaultTime * 3;
      } 

      if (oldMode7Value == false)
      {
         maxTime = defaultTime * 6;
      }

      if (oldMode14Value == false)
      {
         maxTime = defaultTime * 12;
      }

      if (oldManualModeValue == false)
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
      NOKIA_5110_LCD.setContrast(42);
      NOKIA_5110_LCD.clearDisplay();

      InitialLogo();

      delay(4000);
   }

   void loop()
   {
      ReadButtonValues();
	    CheckButtonStatuses();

      delay(500);
   }
