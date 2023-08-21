using System;
using System.Windows;

namespace GardenSystem
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //-------------------------------------------------------
            //- STATION 1
            //-------------------------------------------------------
            ucStation createstation1 = new ucStation();
            createstation1.Log = new Logger.Log("station1");
            createstation1.MySettingsFile = "station1_Settings.json";
            createstation1.StationName = "Indoor";
            createstation1.lblName.Content = createstation1.StationName;
            StationArea.Children.Add(createstation1);
            createstation1.ReadFromFile();
            //createstation1.CheckConnection("COM6");
            createstation1.MainTimer.Interval = TimeSpan.FromMilliseconds(250);
            createstation1.MainTimer.Tick += createstation1.MainTimer_Tick;
            createstation1.MainTimer.Stop();

            //-------------------------------------------------------
            //- STATION 2
            //-------------------------------------------------------
            //ucStation createstation2 = new ucStation();
            //createstation2.Log = new Logger.Log("station2");
            //createstation2.MySettingsFile = "station2_Settings.json";
            //createstation2.StationName = "Outdoor 1";
            //createstation2.lblName.Content = createstation2.StationName;
            //StationArea.Children.Add(createstation2);
            //createstation2.ReadFromFile();
            ////createstation2.CheckConnection("COM3");
            //createstation2.MainTimer.Interval = TimeSpan.FromMilliseconds(500);
            //createstation2.MainTimer.Tick += createstation2.MainTimer_Tick;
            //createstation2.MainTimer.Stop();

            //-------------------------------------------------------
            //- STATION 3
            //-------------------------------------------------------
            //ucStation createstation3 = new ucStation();
            //createstation3.Log = new Logger.Log("station3");
            //createstation3.MySettingsFile = "station3_Settings.json";
            //createstation3.StationName = "Outdoor 2";
            //createstation3.lblName.Content = createstation3.StationName;
            //StationArea.Children.Add(createstation3);
            //createstation3.ReadFromFile();
            ////createstation3.CheckConnection("COM4");
            //createstation3.MainTimer.Interval = TimeSpan.FromMilliseconds(1000);
            //createstation3.MainTimer.Tick += createstation3.MainTimer_Tick;
            //createstation3.MainTimer.Stop();
        }
    }
}
