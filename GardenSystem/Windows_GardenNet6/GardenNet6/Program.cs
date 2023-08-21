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
		/* Copyright by Isaac Garcia Peveri, 2022. All Rights Reserved */
		/***************************************************************/
		/**/

		#region "WORKING-STORAGE"
		private string MySettingsFile = string.Empty;
		private static SerialPort SP = new SerialPort("/dev/ttyACM0", 9600);

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
		private static int Ore = 0; 
		private static int Minuti = 0;

		//Thread for Main Logic - This is the core of the application
		private System.Timers.Timer MainTimer;
		private string SettingsString = null;
		private static LoggerCS.Log LOG = new LoggerCS.Log("default");
		#endregion

		static void Main(string[] args)
		{
			/// <summary>
			/// RUN
			/// </summary>
			Application.Init();
			var top = Application.Top;

			// Creates the top-level window to show
			var win = new Window("GardenSystem 1.0 - C2022 by Isaac Garcia Peveri")
			{
				X = 2,
				Y = 2, // Leave one row for the toplevel menu

				// By using Dim.Fill(), it will automatically resize without manual intervention
				Width = Dim.Percent(40),
				Height = Dim.Percent(90)
			};

			var winlogs = new Window("LOG")
			{
				X = 51,
				Y = 2, // Leave one row for the toplevel menu

				// By using Dim.Fill(), it will automatically resize without manual intervention
				Width = Dim.Percent(56),
				Height = Dim.Percent(49),
				ColorScheme = Colors.Base
			};

			var winsettings = new Window("Settings")
			{
				X = 51,
				Y = 16, // Leave one row for the toplevel menu

				// By using Dim.Fill(), it will automatically resize without manual intervention
				Width = Dim.Percent(56),
				Height = Dim.Percent(45),
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
			var LabelConnStatus = new Label("Connection Status: ") { X = 2, Y = 2 };
			var ConnText = new Label("")
			{
				X = Pos.Right(LabelConnStatus),
				Y = Pos.Top(LabelConnStatus),
				Width = 50
			};

			//DateTimeNow 
			var LabelDTNow = new Label("DateTime Now: ") { X = 2, Y = 6 };
			var DTNowText = new TextField("")
			{
				X = Pos.Right(LabelDTNow),
				Y = Pos.Top(LabelDTNow),
				Width = 20
			};

			//Seconds to keep pump open
			var TimePumpOpen = new Label("Keep water open for: ") { X = 6, Y = 6 };
			var TimePumpOpenText = new TextField("120")
			{
				X = Pos.Right(TimePumpOpen),
				Y = Pos.Top(TimePumpOpen),
				Width = 5
			};
			var TimePumpOpenSecs = new Label(" Seconds ") { X = 30, Y = 8 };

			//DateTime to set when Cycle starts
			var LabelDateTimeCycle = new Label("Start at: ") { X = 6, Y = 4 };
			var DateTimeCycleText = new TextField(DateTime.Now.ToString("HH:MM"))
			{
				X = Pos.Right(LabelDateTimeCycle),
				Y = Pos.Top(LabelDateTimeCycle),
				Width = 6
			};

			//Cycle Started or Waiting...
			var CycleStatus = new Label("WAITING FOR NEXT CYCLE...")
			{
				X = 3,
				Y = 13,
				ColorScheme = Colors.Error
			};

			//Cycle Started or Waiting...
			var lblTicks = new Label("Ticks: ") { X = 2, Y = 15 };
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
				X = 3,
				Y = 0,
				Height = Dim.Fill(),
				Width = Dim.Fill(1),
				//ColorScheme = Colors.TopLevel,
				AllowsMarking = false,
				AllowsMultipleSelection = false
			};

			var Button1 = new Button(3, 18, "Open Water");
			var Button2 = new Button(19, 18, "Close Water");

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
				CycleStatus,
				Button1,
				Button2,
				lblTicks,
				CycleTicks
			);

			//Controls on second window (LOG WINDOW)
			winlogs.Add(
				LvLogs
			);

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

			Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(1), UpdateTimer);
			LOG.Add("Application Started");
			Application.Run();
			Application.Shutdown();

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

			///
			/// MAIN LOGIC INSIDE THIS TIMER
			///
			bool UpdateTimer(MainLoop arg)
			{
				DTNowText.Text = DateTime.Now.ToString();

				try 
				{
					Ore = Convert.ToInt32(DateTimeCycleText.Text.ToString().Split(':')[0]);
					Minuti = Convert.ToInt32(DateTimeCycleText.Text.ToString().Split(':')[1]);
				} catch (Exception ex) 
				{
					LOG.Add("Convert HH:MM " + ex.Message);
				}

				int idx = 0;
				string[] wLines = LOG.Read().Split('\n');
				Array.Reverse(wLines);
				LvLogs.SetSource(wLines);
				//string pPort = "COM" + ComText.Text.ToString();
				string pPort = "/dev/ttyACM0";

				if (CheckConnection(pPort))
				{
					if (!alreadySent || ManualOpening)
					{
						if (DateTime.Now.Hour == Ore && DateTime.Now.Minute == Minuti || ManualOpening)
						{
							CycleStatus.Text = "Cycle Started: Water is open.";
							CycleStatus.ColorScheme = Colors.TopLevel;
							SendRequest("ON_1"); //Request to open Water pump
 							LOG.Add("Cycle Started, Watering now...");
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
					//Console.WriteLine(MyPort);

					try
					{
						SP.Open();

						//LabelConnStatus.ColorScheme.HotNormal;
						LabelConnStatus.Text = "Controller Connected";

						if (firstTime)
						{
							firstTime = false;
							LOG.Add("Controller Connected");
						}

						retval = true;
					}
					catch (Exception ex)
					{
						if (ex.Message.Contains("already open"))
							return true;

						retval = false;
						LabelConnStatus.Text = "Controller Disconnected";
						CycleStatus.Text = "WAITING FOR CONNECTION...";
						CycleStatus.ColorScheme = Colors.Error;
						LOG.Add("Error during opening communication Port: " + ex.Message);
					}

					return retval;
				}

				void SendRequest(string Req)
				{					
					try
					{
						SP.Write(Req);
						LOG.Add("Request sent to Contoller: " + Req);
					}
					catch (Exception ex)
					{
						LabelConnStatus.Text = "Disconnected!!!";
						LOG.Add("Error during Sending Request to Controller: " + ex.Message);
					}
				}
				#endregion
			}
		}

		/// <summary>
		/// Quit Application
		/// </summary>
		static bool Quit()
		{
			var n = MessageBox.Query(50, 7, "Quit GardenSystem", "Are you sure you want to quit Application?", "Yes", "No");
			return n == 0;
		}

	}
}
