using System;
using Terminal.Gui;
using Application = Terminal.Gui.Application;
using System.IO.Ports;

namespace GardenSystemCharacterGui
{
    class Program
    {
		/**/
		/***************************************************************/
		/* GARDEN SYSTEM CHARACTER BASED GUI VERSION                   */
		/*-------------------------------------------------------------*/
		/* Author:        Isaac Garcia Peveri                          */
		/* Date Written:  01.07.2022                                   */
		/* Date Compiled: 01.08.2022                                   */
		/***************************************************************/
		/**/

		#region "WORKING-STORAGE"
		private string MySettingsFile = string.Empty;
		private static SerialPort SP = new SerialPort();

		//Automatic interval for watering. Now is fixed to 24 Hours, Can change by Settings
		private int MyThicks = 20; // 10 seconds of Watering if Thicks = 20. This is only a default value, Settings will override it.
		private int secondi = 0;
		public static DateTime runDay = DateTime.Now;
		public static TimeSpan TS = new TimeSpan();

		//Bools
		private static bool alreadySent = false;
		private static bool firstTime = true;
		private static bool ManualOpening = false;
		private static bool ManualClosing = false;

		//Thread for Main Logic - This is the core of the application
		private System.Timers.Timer MainTimer;
		private string SettingsString = null;
		private static LoggerCS.Log LOG = new LoggerCS.Log("default");
		#endregion

		static void Main(string[] args)
        {
            #region FORM
            /// <summary>
            /// RUN
            /// </summary>
            Application.Init();
			var top = Application.Top;
			
			// Creates the top-level window to show
			var win = new Window("GardenSystem 1.0 - C2022 by IGP")
			{
				X = 2,
				Y = 2, // Leave one row for the toplevel menu

				// By using Dim.Fill(), it will automatically resize without manual intervention
				Width = Dim.Percent(35),
				Height = Dim.Percent(90)
			};

			var winlogs = new Window("LOG")
			{
				X = 45,
				Y = 2, // Leave one row for the toplevel menu

				// By using Dim.Fill(), it will automatically resize without manual intervention
				Width = Dim.Percent(60),
				Height = Dim.Percent(50),
				ColorScheme = Colors.Base
			};

			var winsettings = new Window("Settings")
			{
				X = 45,
				Y = 18, // Leave one row for the toplevel menu

				// By using Dim.Fill(), it will automatically resize without manual intervention
				Width = Dim.Percent(60),
				Height = Dim.Percent(39),
				ColorScheme = Colors.Base
			};

			// Creates a menubar, the item "New" has a help menu.
			var menu = new MenuBar(new MenuBarItem[] {
					new MenuBarItem ("_File", new MenuItem [] {
						new MenuItem ("_New", "Creates new file", null),
						new MenuItem ("_Close", "",null),
						new MenuItem ("_Quit", "", () => { if (Quit ()) top.Running = false; })
					}),
					new MenuBarItem ("_Edit", new MenuItem [] {
						new MenuItem ("_Copy", "", null),
						new MenuItem ("C_ut", "", null),
						new MenuItem ("_Paste", "", null)
					})
				});

			//COM and Entry Field
			var LabelComPort = new Label("COM: ") { X = 6, Y = 2 };
			var ComText = new TextField("4")
			{
				X = Pos.Right(LabelComPort),
				Y = Pos.Top(LabelComPort),
				Width = 5
			};

			//Connection Status and Entry Field
			var LabelConnStatus = new Label("Connection Status: ") { X = 3, Y = 2 };
			var ConnText = new Label("")
			{
				X = Pos.Right(LabelConnStatus),
				Y = Pos.Top(LabelConnStatus),
				Width = 50
			};

			//DateTimeNow 
			var LabelDTNow = new Label("DateTime Now: ") { X = 3, Y = 6 };
			var DTNowText = new TextField("")
			{
				X = Pos.Right(LabelDTNow),
				Y = Pos.Top(LabelDTNow),
				Width = 20
			};

			//Seconds to keep pump open
			var TimePumpOpen = new Label("Keep water open for: ") { X = 6, Y = 8 };
			var TimePumpOpenText = new TextField("20")
			{
				X = Pos.Right(TimePumpOpen),
				Y = Pos.Top(TimePumpOpen),
				Width = 3
			};
			var TimePumpOpenSecs = new Label(" Seconds ") { X = 30, Y = 8 };

			//DateTime to set when Cycle starts
			var LabelDateTimeCycle = new Label("Start Cycle every day at: ") { X = 6, Y = 6 };
			var DateTimeCycleText = new TextField(DateTime.Now.ToString("HH:MM"))
			{
				X = Pos.Right(LabelDateTimeCycle),
				Y = Pos.Top(LabelDateTimeCycle),
				Width = 6
			};

			//Cycle Started or Waiting...
			var CycleStatus = new Label("WAITING FOR NEXT CYCLE...")
			{
				X = 5, Y = 11,
				ColorScheme = Colors.Error
			};

			//Cycle Started or Waiting...
			var lblTicks = new Label("Ticks: ") { X = 5, Y = 13 };
			var CycleTicks = new TextField("")
			{
				X = Pos.Right(lblTicks),
				Y = Pos.Top(lblTicks),
				ColorScheme = Colors.TopLevel,
				Width = 2
			};

			//Label Logs...
			var LvLogs = new ListView 
			{ 
				X = 1, Y = 0,
				Height = Dim.Fill(),
				Width = Dim.Fill(),
				//ColorScheme = Colors.TopLevel,
				AllowsMarking = false,
				AllowsMultipleSelection = false
			};

			var Button1 = new Button(06, 18, "Open Water");
			var Button2 = new Button(21, 18, "Close Water");

			top.Add(win);
			top.Add(winlogs);
			top.Add(winsettings);
			top.Add(menu);

			// CONTROLS on MAIN WINDOW 
			win.Add(
				// The ones with my favorite layout system, Computed
				LabelConnStatus,
				ConnText,
				LabelDTNow,
				DTNowText,
				//LabelDTNext,
				//DTNextText,
				CycleStatus,
				Button1,
				Button2,
				lblTicks,
				CycleTicks,

				// The ones laid out like an australopithecus, with Absolute positions:
				//new CheckBox(3, 6, "Remember me"),
				//new RadioGroup(3, 8, new ustring[] { "_Personal", "_Company" }, 0),
				new Label(5, 21, "F9 or ESC plus 9 activate menu")
			);

			//Controls on second window (LOG WINDOW)
			winlogs.Add(
				LvLogs
			) ;

			//Controls on third window (SETTINGS WINDOW)
			winsettings.Add(
				LabelComPort,
				ComText,
				LabelDateTimeCycle,
				DateTimeCycleText,
				TimePumpOpen,
				TimePumpOpenSecs,
				TimePumpOpenText
			);

			LabelConnStatus.Text = "Controller Disconnected";
            #endregion

            #region MAINLOOP
            Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(1), UpdateTimer);
			LOG.Add("Application Started");
			Application.Run();
			Application.Shutdown();
            #endregion

            #region TIMER
            ///
            /// MAIN LOGIC INSIDE THIS TIMER
            ///
            bool UpdateTimer(MainLoop arg)
			{
				Button1.Clicked += () =>
				{
					ManualOpening = true;
					alreadySent = false;
				};

				Button2.Clicked += () =>
				{
					alreadySent = true;
					ManualClosing = true;
				};

				DTNowText.Text = DateTime.Now.ToString();
				int Ore = Convert.ToInt32(DateTimeCycleText.Text.ToString().Split(':')[0]);
				int Minuti = Convert.ToInt32(DateTimeCycleText.Text.ToString().Split(':')[1]);

				int idx = 0;
				string[] wLines = LOG.Read().Split('\n');
				Array.Reverse(wLines);
				LvLogs.SetSource(wLines);
				string pPort = "COM" + ComText.Text.ToString();

				if (CheckConnection(pPort))
				{
					if (!alreadySent || ManualOpening)
					{
						if ((DateTime.Now.Hour == Ore && DateTime.Now.Minute == Minuti) || ManualOpening)
						{
							CycleStatus.Text = "Cycle Started: Water is open.";
							CycleStatus.ColorScheme = Colors.TopLevel;
							SendRequest("ON_1"); //Request to open Water pump
							ManualOpening = false;
						}

						alreadySent = true;
					}

					CycleTicks.Text = Convert.ToString(Convert.ToInt32(DateTime.Now.Subtract(runDay).TotalSeconds));

					if (Convert.ToInt32(DateTime.Now.Subtract(runDay).TotalSeconds) > Convert.ToInt32(TimePumpOpenText.Text) || ManualClosing)
                    {
						runDay = DateTime.Now;
						SendRequest("OFF_1"); //Request to close Water pump after seconds passed
						alreadySent = false;
						LOG.Add("Cycle Finished!");
						CycleStatus.Text = "WAITING FOR NEXT CYCLE";
						CycleStatus.ColorScheme = Colors.Dialog;
						DateTimeCycleText.Text = DateTime.Now.AddDays(1).ToString("HH:mm");
						runDay = DateTime.Now.AddDays(1);
						CycleTicks.Text = "00";
						ManualClosing = false;
						ManualOpening = false;
					}
				}
				
				return true;

				#region "CONNECTION"
				/// <summary>
				/// Call this method to check if port is open before to send / receive any data
				/// This method is called also by "TimerConnection" event
				/// </summary>
				/// <returns></returns>
				bool CheckConnection(string MyPort)
				{
					bool retval = false;
					MyPort = MyPort;

					try
					{
						if (SP != null)
							SP.Close(); // Disable if you run on Wine

						SP = new SerialPort(MyPort, 9600);
						SP.Open();

						//LabelConnStatus.ColorScheme.HotNormal;
						LabelConnStatus.Text = "Controller Connected";

						if (firstTime)
                        {
							firstTime = false;
							LOG.Add("Server Connected");
						}

						retval = true;
					}
					catch (Exception ex)
					{
						retval = false;
						//LabelConnStatus.ColorScheme.Focus;
						LabelConnStatus.Text = "Controller Disconnected";
						CycleStatus.Text = "WAITING FOR CONNECTION...";
						CycleStatus.ColorScheme = Colors.Error;
						//CycleStatus.ColorScheme.HotNormal;
						LOG.Add("Error during opening communication Port");
					}

					return retval;
				}

				void SendRequest(string Req)
                {
					try
					{
						SP.Write(Req);
						LOG.Add("Request sent to Server");
					}
					catch (Exception ex)
					{
						LabelConnStatus.Text = "Disconnected!!!";
						LOG.Add("Error during Sending Request to Server");
					}
				}
				#endregion
			}
            #endregion
        }

        #region STATIC
        /// <summary>
        /// Quit Application
        /// </summary>
        static bool Quit()
		{
			var n = MessageBox.Query(50, 7, "Quit GardenSystem", "Are you sure you want to quit Application?", "Yes", "No");
			return n == 0;
		}
        #endregion
    }
}
