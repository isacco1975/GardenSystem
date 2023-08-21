using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

//**-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*
//*         GardenSystem 1.0 C# - C 2022 by Isaac Garcia Peveri          *
//**-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*
//*                                                                      *
//*   Program description:                                               *
//*           this software controls an Arduino to turn on a solenoid    *
//*           that is connected to watering plants and flower at home.   *
//*                                                                      *
//*           Default timing is each 24 hours, can be changed.           *
//*                                                                      *
//*           Manual watering is also provided.                          *
//*                                                                      *
//**-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*

namespace GardenSystem
{
    public partial class ucStation : UserControl
    {
        #region "Working-Storage"
        //Arduino's serial port: board is communicating by default con COM3 at 9600 bauds
        public string StationName = string.Empty;

        //For Logs
        public Logger.Log Log = null;
        public SerialPort SP = null;
        public string MyPort = string.Empty; //(From Settings) 
        public string MySettingsFile = string.Empty;

        private List<LogDetail> LD = new List<LogDetail>();
        private LogDetail L = new LogDetail();

        //Automatic interval for watering. Now is fixed to 24 Hours, Can change by Settings
        private int MySeconds = 86400; //24h This is only a default value. Settings will override it.
        private int MyThicks = 20; // 10 seconds of Watering if Thicks = 20. This is only a default value, Settings will override it.
        private TimeSpan ts = new TimeSpan(0, 0, 86400); //This is only a default value, Settings will override it.  
        private int secondi = 0;
        private DateTime runDay = DateTime.Now;

        //Bools
        private bool inCycle = false;
        private bool alreadySent = false;
        private bool alreadyLogged = false;
        private bool inverter = false;
        private bool FirstTime = true;
        private bool allOK = true;
        private bool ManualOpening = false;

        //Thread for Main Logic - This is the core of the application
        public System.Windows.Threading.DispatcherTimer MainTimer = new System.Windows.Threading.DispatcherTimer();

        public object SettingsString = null;
        #endregion

        #region "INTIALIZE"
        /// <summary>
        /// Initialize
        /// </summary>
        public ucStation()
        {
            InitializeComponent();

            panel1.Visibility = Visibility.Visible;
            MainTimer.Stop();
            pbClose.IsEnabled = false;
            pbOpen.IsEnabled = false;

            secondi = 0;
            lblWatering.Background = Brushes.Transparent;
            lblWatering.Content = "Waiting for next Cycle...";
            lblSecs.Content = "00";
            secondi = 0;
            tubo.Visibility = Visibility.Hidden;
        }
        #endregion

        #region "CONNECTION"
        /// <summary>
        /// Call this method to check if port is open before to send / receive any data
        /// This method is called also by "TimerConnection" event
        /// </summary>
        /// <returns></returns>
        public bool CheckConnection(string MyPort)
        {
            bool retval = false;

            try
            {
                lblPort.Background = Brushes.Green;
                lblPort.Content = "Connected";
                globe.Visibility = Visibility.Hidden;
                retval = true;

                if (retval && FirstTime)
                {
                    FirstTime = false;
                    Log.Add("Server Connected");
                }

                if (SP != null)
                    SP.Close(); // Disable if you run on Wine

                SP = new SerialPort(MyPort);
                SP.Open();
            }
            catch (Exception ex)
            {
                retval = false;
                FirstTime = true;
                lblPort.Background = Brushes.Red;
                lblPort.Content = "Disconnected";
                lblWatering.Background = Brushes.DarkRed;
                lblWatering.Content = "WAITING FOR CONNECTION...";
                lblSecs.Content = "00";
                secondi = 0;
                tubo.Visibility = Visibility.Hidden;
                cless.Visibility = Visibility.Hidden;
                WS.Visibility = Visibility.Hidden;
                globe.Visibility = Visibility.Visible;
                lblNext.Content = runDay.ToString();
                Log.Add("Error during opening communication Port");
            }

            return retval;
        }

        /// <summary>
        /// Call this method to send a request to the Arduino board
        /// </summary>
        /// <param name="Req"></param>
        public void SendRequest(string Req)
        {
            try
            {
                SP.Write(Req);
                lblPort.Background = Brushes.Green;
                lblPort.Content = "Connected";
                Log.Add("Request sent to Server");
            }
            catch (Exception ex)
            {
                lblPort.Background = Brushes.Red;
                lblPort.Content = "Disconnected";
                Log.Add("Error during Sending Request to Server");
            }
        }
        #endregion

        #region "THREADS"
        /// <summary>
        /// MAIN LOGIC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MainTimer_Tick(object sender, EventArgs e)
        {
            inverter = !inverter;

            if (CheckConnection(MyPort))
            {
                Console.WriteLine(DateTime.Now.Subtract(runDay).TotalSeconds);
                lblNext.Content = runDay.AddSeconds(MySeconds / 2 - 4).ToString(); //DateTime.Now is running during watering, this is to coincide with next timing.
                lblNow.Content = DateTime.Now.ToString();

                if (!inCycle)
                {
                    if (inverter)
                        lblNext.Foreground = Brushes.Orange;
                    else
                        lblNext.Foreground = Brushes.Yellow;

                    if (!alreadyLogged)
                    {
                        alreadyLogged = true;
                        Log.Add("Waiting for next Cycle...");
                    }
                }
                else
                    lblNext.Foreground = Brushes.Orange;

                cless.Visibility = Visibility.Visible;

                if ((Convert.ToInt32(DateTime.Now.Subtract(runDay).TotalSeconds) > MySeconds - 20) || ManualOpening)
                {
                    alreadyLogged = false;
                    inCycle = true;
                    cless.Visibility = Visibility.Hidden;

                    if (!alreadySent)
                    {
                        alreadySent = true;

                        if (CheckConnection(MyPort))
                        {
                            allOK = true;
                            SendRequest("ON_1"); //Request to open Water pump
                        }
                        else
                        {
                            allOK = false;
                            alreadySent = false;
                            runDay = DateTime.Now;
                        }
                    }

                    if (allOK)
                    {
                        if (!inverter)
                            tubo.Visibility = Visibility.Hidden;
                        else
                            tubo.Visibility = Visibility.Visible;

                        lblWatering.Background = new SolidColorBrush(Color.FromRgb(00, 85, 255));
                        lblWatering.Content = "Cycle Started";
                    }
                    else
                    {
                        Log.Add("> Trying again to send request to open Pump...");
                        lblWatering.Content = "Retrying Cycle again due to error...";
                        lblWatering.Background = new SolidColorBrush(Colors.Orange);
                    }

                    secondi += 1;
                    lblSecs.Content = Convert.ToString(secondi);

                    //Stop watering after 10 seconds (20 ticks if timer is 500ms) (//TODO put "secondi" in settings)
                    if (secondi > MyThicks && allOK) //If nothing is OK, don't increase next time, but try to water plants again
                    {
                        alreadySent = false;
                        inCycle = false;
                        lblSecs.Content = "00";
                        secondi = 0;
                        lblWatering.Background = Brushes.Transparent;
                        lblWatering.Content = "Waiting for next Cycle...                                                                     ";
                        lblSecs.Content = "00";
                        secondi = 0;
                        tubo.Visibility = Visibility.Hidden;
                        runDay = DateTime.Now.AddSeconds(MySeconds);
                        lblNext.Content = runDay.ToString();
                                                               
                        if (CheckConnection(MyPort))
                        {
                            allOK = true;

                            if (!ManualOpening)
                                SendRequest("OFF_1"); //Close water, cycle finished
                        }
                        else
                        {
                            allOK = false;                                          
                            alreadySent = true;
                            runDay = DateTime.Now;
                        }
                    }
                }
            }

            int idx = 0;                               
            string[] wLines = Log.Read().Split('\n');
            TextBoxLog.Text = "";

            Array.Reverse(wLines);

            foreach (string wLine in wLines)
            {
                TextBoxLog.Text += wLine;

                idx++;

                if (idx > 20)
                    break;
            }
        }
        #endregion

        #region "BUTTONS"
        /// <summary>
        /// Manual Watering ON
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            Log.Add("Manual Watering Requested from User");
            ManualOpening = true;
            alreadySent = false;

            if (CheckConnection(MyPort))
            {
                SendRequest("ON_1");
                lblWatering.Background = new SolidColorBrush(Color.FromRgb(00, 85, 255));
                lblWatering.Content = "Cycle Started";
            }
            else
                MySeconds = 10;
        }

        /// <summary>
        /// Manual Watering OFF_1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Log.Add("Manual STOP Requested from User");
            ManualOpening = false;

            if (CheckConnection(MyPort))
                SendRequest("OFF_1");
            else
                MySeconds = 10;

            alreadySent = false;
            inCycle = false;
            lblSecs.Content = "00";
            secondi = 0;
            tubo.Visibility = Visibility.Hidden;
            lblWatering.Background = Brushes.Transparent;
            lblWatering.Content = "Waiting for next Cycle...";
            lblSecs.Content = "00";
            secondi = 0;
            tubo.Visibility = Visibility.Hidden;
            runDay = DateTime.Now.AddSeconds(MySeconds);
            lblNext.Content = runDay.ToString();
            alreadyLogged = true;
            Log.Add("Waiting for next Cycle...");
        }

        public void pbSave_Click(object sender, RoutedEventArgs e)
        {
            WriteToFile();
        }

        public void pbDisable(object sender, RoutedEventArgs e)
        {
            Log.Add("STATION DISABLED BY USER");
            panel1.Visibility = Visibility.Visible;
            MainTimer.Stop();
            pbClose.IsEnabled = false;
            pbOpen.IsEnabled = false;
        }

        public void pbEnable(object sender, RoutedEventArgs e)
        {
            Log.Add("STATION ENABLED BY USER");
            panel1.Visibility = Visibility.Hidden;
            MainTimer.Start();
            pbClose.IsEnabled = true;
            pbOpen.IsEnabled = true;
        }
        #endregion

        #region "SETTINGS"
        public string SettingsFilePath()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), MySettingsFile);
        }

        public void WriteToFile()
        {
            SingleSetting S = new SingleSetting();

            S.MainInterval = Convert.ToInt32(efInterval.Text);
            S.WaterInterval = Convert.ToInt32(efTicks.Text);
            S.ComName = efPort.Text;

            string wText = JsonConvert.SerializeObject(S, Formatting.Indented);
            System.IO.File.WriteAllText(SettingsFilePath(), wText);

            MessageBox.Show("Settngs Updated");
        }

        public void ReadFromFile()
        {
            string wText = string.Empty;

            try
            {
                if (System.IO.File.Exists(SettingsFilePath()))
                {
                    wText = System.IO.File.ReadAllText(MySettingsFile);
                    SingleSetting S = new SingleSetting();
                    S = JsonConvert.DeserializeObject<SingleSetting>(wText);

                    efInterval.Text = Convert.ToString(S.MainInterval);
                    efTicks.Text = Convert.ToString(S.WaterInterval);
                    efPort.Text = S.ComName;

                    MySeconds = S.MainInterval;
                    MyThicks = S.WaterInterval;
                    MyPort = S.ComName;
                }
                else
                    WriteToFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading Settings files: " + ex.Message);
            }
        }
        #endregion

        //END MAIN
    }

    //END CLASS
}
