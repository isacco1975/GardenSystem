       IDENTIFICATION DIVISION.
       PROGRAM-ID.    "COBOLGARDEN",  IS INITIAL.
       AUTHOR.        "ISAAC GARCIA PEVERI".
       INSTALLATION.  "ISAAC GARCIA PEVERI".
       DATE-WRITTEN.  24.08.2023.
       DATE-COMPILED. 24.08.2023.
       REMARKS.       ACUCOBOL-GT DIALECT 7.0.0.
      *
      *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-**
      *                                                                *
      *   USAGE:      TO CHANGE NUMBER OF DAYS AND NUMBER OF SECONDS   *
      *               KEEPING WATER OPEN, CHANGE THE VALUE OF THESE    *
      *               VARIABLES IN WORKING-STORAGE:                    *
      *                                                                *
      *               05 SETTINGS-NDAYS PIC  9(2)             VALUE 01.*
      *               05 SETTINGS-NSECS PIC  9(2)             VALUE 15.*
      *                                                                *
      *               I AM PLANNING TO PUT THOSE SETTINGS IN A FILE    *
      *               FOR FUTURE VERSIONS                              *
      *                                                                *
      *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-**
      *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      * E N V I R O N M E N T                                          *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
       ENVIRONMENT DIVISION.
       CONFIGURATION SECTION.
       SPECIAL-NAMES. DECIMAL-POINT IS COMMA.
      *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      * F I L E   C O N T R O L                                        *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
       FILE-CONTROL.
      *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      * F I L E   S E C T I O N                                        *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
       FILE SECTION.
      *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      * W O R K I N G   S T O R A G E   S E C T I O N                  *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
       WORKING-STORAGE SECTION.
       77 KEY-PRESSED        PIC X.
       01 DONE               PIC X.
          88 NOT-DONE        VALUE "N".
          88 ALL-DONE        VALUE "D".
      *
       77 VIDEO-DATE         PIC  X(10)           VALUE "01/01/1900".
       77 VIDEO-DATE-NEXT    PIC  X(10)           VALUE "31/12/2099".
       77 VIDEO-TIME         PIC  X(8)            VALUE "00:00:00".
       77 VIDEO-MESSAGE      PIC  X(15)           VALUE "Pump is off".
       77 BLINK-MESSAGE      PIC  X(15)           VALUE "WAIT PLEASE".
       77 WS-DAYS            PIC  9(14)           VALUE ZERO.
       77 MOD-DAYS           PIC  9(2)            VALUE ZERO.
       77 WS-DATE-NEXT       PIC  9(8)            VALUE 20991231.
       77 WS-D               PIC  99              VALUE ZERO.
       77 WS-DATE            PIC  9(8)            VALUE 20991231.
       77 WS-TIME            PIC  9(8)            VALUE ZERO.
       77 WS-CURR-TIMESTAMP  PIC 9(14)            VALUE ZERO.
       77 WS-NEXT-TIMESTAMP  PIC 9(14)            VALUE ZERO.
      *
      *-> change these 2 following variable to your needs
       01 SETTINGS-GROUP.
          05 SETTINGS-NDAYS PIC  9(2)             VALUE 01.
          05 SETTINGS-NSECS PIC  9(2)             VALUE 15.

      *-> Message to Arduino
       01 CMD-SEND-MESSAGE.
      *-> The command to start the Serial driver
          05 CMD-LINE0   PIC X(06)                VALUE "START ".
          05 CMD-LINE2   PIC X(21)       VALUE "IGP_SimpleSerial.exe ".
      *-> passing arguments to it (port name, speed, databits)
          05 CMD-LINE3   PIC X(12)                VALUE "COM3 9600 8 ".
      *-> See the .ino sketch attached: this is what arduino checks
          05 CMD-LINE4   PIC X(1)                 VALUE "1".
      *-> just a filler, nothing
          05 FILLER      PIC X(1)                 VALUE SPACE.
      *-> number of seconds from SETTINGS-NSECS
          05 CMD-LINE5   PIC 9(2)                 VALUE 15.
      *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      * S C R E E N     S E C T I O N                               *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
       SCREEN SECTION.
       01  MAIN-SCREEN.
           03 BLANK SCREEN BACKGROUND-COLOR 9 FOREGROUND-COLOR 6.
       01  INPUT-SETTINGS AUTO.
           03 LINE 3 COLUMN 28 HIGHLIGHT "AUTOMATIC WATERING SYSTEM"
              COLOR 11.
           03 LINE 4 COLUMN 28 HIGHLIGHT "2023 - ISAAC GARCIA PEVERI"
              COLOR 11.
           03 LINE 7 COLOR 8 COLUMN 20 HIGHLIGHT "ACTUAL DATE: ".
           03 PIC X(10) USING VIDEO-DATE REVERSE-VIDEO.
           03 LINE 8 COLOR 8 COLUMN 20 HIGHLIGHT "NEXT   DATE: ".
           03 PIC X(10) USING VIDEO-DATE-NEXT REVERSE-VIDEO.
           03 LINE 7 COLOR 8 COLUMN 44 HIGHLIGHT "TIME: ".
           03 PIC X(09) USING VIDEO-TIME REVERSE-VIDEO.
           03 LINE 10 COLUMN 27 HIGHLIGHT "ACTUAL STATUS: "
              COLOR 8.
           03 LINE 10 COLUMN 42 PIC X(15)
              USING VIDEO-MESSAGE COLOR 5.
           03 LINE 11 COLUMN 42 PIC X(15)
              USING BLINK-MESSAGE BLINK COLOR 5.
           03 LINE 11 COLUMN 42 PIC X(15)
              USING BLINK-MESSAGE COLOR 5.
           03 LINE 13 COLUMN 25 HIGHLIGHT "KEEP WATER OPEN FOR: "
              COLOR 8.
           03 LINE 13 COLUMN 46 PIC X(2) REVERSE-VIDEO
              USING SETTINGS-NSECS.
           03 LINE 13 COLUMN 49 HIGHLIGHT "SECONDS"
              COLOR 8.
           03 LINE 15 COLUMN 25 HIGHLIGHT "EXECUTE CYCLE EVERY: "
              COLOR 8.
           03 LINE 15 COLUMN 46 PIC X(2) REVERSE-VIDEO
              USING SETTINGS-NDAYS.
           03 LINE 15 COLUMN 49 HIGHLIGHT "DAYS"
              COLOR 8.
           03 LINE 19 COLUMN 20
              HIGHLIGHT "ENTER 1: FOR MANUAL WATER OPENING"
              COLOR 4.
           03 LINE 20 COLUMN 20
              HIGHLIGHT "EBTER X: TO EXIT APPLICATION"
              COLOR 4.
        01 KEY-INPUT.
           03 LINE 21 COLUMN 20 HIGHLIGHT "CHOICE:" COLOR 4
              REVERSE-VIDEO.
           03 LINE 21 PIC X COLUMN 28 USING KEY-PRESSED
              REVERSE-VIDEO.
           03 LINE 21 COLUMN 30
              HIGHLIGHT " THEN PRESS ENTER, TO CONFIRM"
              COLOR 4 REVERSE-VIDEO.
      *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      * P R O C E D U R E   D I V I S I O N                            *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
       PROCEDURE DIVISION.
      *----------------------------------------------------------------*
       00-MAIN.
            SET NOT-DONE                TO TRUE
            MOVE SPACES                 TO BLINK-MESSAGE
            MOVE FUNCTION CURRENT-DATE(1:14)
              TO WS-CURR-TIMESTAMP

            PERFORM 05-CALCULATE-NEXT-TS

            DISPLAY MAIN-SCREEN
            PERFORM THREAD 10-WORKING-CYCLE

            PERFORM UNTIL ALL-DONE
                ACCEPT KEY-INPUT NO-ECHO

                EVALUATE TRUE
                    WHEN KEY-PRESSED = "X"
                    WHEN KEY-PRESSED = "x"
                         SET ALL-DONE TO TRUE

                    WHEN KEY-PRESSED = "1"
                         PERFORM 15-OPEN-WATER
                END-EVALUATE
            END-PERFORM

            PERFORM 99-END
            .
      *----------------------------------------------------------------*
       05-CALCULATE-NEXT-TS.
            MOVE FUNCTION CURRENT-DATE(1:14)
                                         TO WS-NEXT-TIMESTAMP
            MOVE WS-NEXT-TIMESTAMP(7:2)  TO WS-D
            ADD  SETTINGS-NDAYS          TO WS-D
            MOVE WS-D                    TO WS-NEXT-TIMESTAMP(7:2)
            MOVE WS-NEXT-TIMESTAMP(1:8)  TO WS-DATE-NEXT
            .
      *----------------------------------------------------------------*
       15-OPEN-WATER.
            MOVE "WATER IS OPEN"   TO VIDEO-MESSAGE
            MOVE "PLEASE WAIT"     TO BLINK-MESSAGE

            DISPLAY INPUT-SETTINGS
            DISPLAY KEY-INPUT

      * -> Passing the number of seconds to the Arduino
            MOVE SETTINGS-NSECS    TO CMD-LINE5

      * -> Calling the Serial Driver and sending the message
            CALL "C$SYSTEM"  USING CMD-SEND-MESSAGE, 64

      * -> Waiting for the same time to finish
            CALL "C$SLEEP"   USING SETTINGS-NSECS

      * -> Display reset
            MOVE "Pump is off"     TO VIDEO-MESSAGE
            MOVE SPACES            TO BLINK-MESSAGE
            .
      *----------------------------------------------------------------*
       10-WORKING-CYCLE.
            PERFORM UNTIL ALL-DONE
                ACCEPT WS-DATE FROM DATE YYYYMMDD
                ACCEPT WS-TIME FROM TIME
                MOVE   FUNCTION CURRENT-DATE(1:14)
                       TO WS-CURR-TIMESTAMP

                MOVE WS-DATE(1:4)       TO VIDEO-DATE(7:)
                MOVE WS-DATE(5:2)       TO VIDEO-DATE(4:2)
                MOVE WS-DATE(7:2)       TO VIDEO-DATE(1:2)

                MOVE WS-DATE-NEXT(1:4)  TO VIDEO-DATE-NEXT(7:)
                MOVE WS-DATE-NEXT(5:2)  TO VIDEO-DATE-NEXT(4:2)
                MOVE WS-DATE-NEXT(7:2)  TO VIDEO-DATE-NEXT(1:2)

                MOVE WS-TIME(1:2)       TO VIDEO-TIME(1:2)
                MOVE WS-TIME(3:2)       TO VIDEO-TIME(4:2)
                MOVE WS-TIME(5:2)       TO VIDEO-TIME(7:2)

                DISPLAY INPUT-SETTINGS
                DISPLAY KEY-INPUT

                COMPUTE WS-DAYS
                   = WS-NEXT-TIMESTAMP - WS-CURR-TIMESTAMP

                COMPUTE MOD-DAYS
                      = FUNCTION MOD(SETTINGS-NDAYS WS-DAYS)

                IF MOD-DAYS = ZERO
                   PERFORM 05-CALCULATE-NEXT-TS
                   PERFORM 15-OPEN-WATER
                END-IF

                CALL "C$SLEEP" USING 0,500
            END-PERFORM
            .
      *----------------------------------------------------------------*
       99-END.
            STOP RUN.
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      * E O F                                                       *
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
      *++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*
