using System;
using System.IO;
using Microsoft.VisualBasic;

namespace LoggerCS
{
    public class Log
    {
        private string LogName;
        private object lockobj = new object();

        public Log(string pLogName)
        {
            LogName = pLogName;
        }

        private string DirectoryName()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string logPath = Path.Combine(Path.GetDirectoryName(assembly.Location), string.Format(@"Log\{0:0000}\{1:00}\{2:00}\", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
            return logPath;
        }

        private string FileName()
        {
            string logFile = Path.Combine(DirectoryName(), string.Format("{0}.txt", LogName));
            return logFile;
        }

        public void Add(string message)
        {
            lock (lockobj)
            {
                message = message.Replace(Constants.vbCrLf, " ");
                string log = string.Format(DateTime.Now.ToString("yyy/MM/dd HH:mm:ss"));

                if (!Directory.Exists(DirectoryName()))
                    Directory.CreateDirectory(DirectoryName());

                File.AppendAllText(FileName(), log + " " + message + Environment.NewLine);
            }
        }

        public string Read()
        {
            string wText = string.Empty;

            lock (lockobj)
            {
                if (File.Exists(FileName()))
                {
                    try
                    {
                        wText = File.ReadAllText(FileName());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("[Read] - " + ex.Message);
                    }
                }
            }

            return wText;
        }

        public string ReadLatest(Int16 rows)
        {
            string wText = string.Empty;
            string[] r_splittext;

            lock (lockobj)
            {
                if (File.Exists(FileName()))
                {
                    try
                    {
                        r_splittext = File.ReadAllLines(FileName());
                        //if (rows >= r_splittext.Length)
                        //    rows = r_splittext.Length;
                        for (int i = r_splittext.Length - 1; i >= r_splittext.Length - rows; i += -1)
                            wText += (r_splittext[i] + Constants.vbCrLf);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("[ReadLatest] - " + ex.Message);
                    }
                }
            }

            return wText;
        }
    }
}
