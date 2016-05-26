using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Brilliantech.Framwork.Utils.ConfigUtil;
using System.IO.Ports;

namespace Brilliantech.ClearInsight.Framework.Config
{
    public class BaseConfig
    {
        private static ConfigUtil config;

        static BaseConfig()
        {
            try
            {
                config = new ConfigUtil("Base", "Ini/base.ini");

                FXType = config.Get("FXType");

                List<Sensor> sensors = new List<Sensor>(); 
                string[] s=config.Get("Sensor").Split('$');
                for (int i=0;i<s.Length;i++) {
                    string[] c = s[i].Split('#')[1].Split(',');
                    string code = c[0];
                    bool trigerOff = bool.Parse(c[1]);
                    bool trigerOn = bool.Parse(c[2]);
                    bool isEmergency = bool.Parse(c[3]);

                    Sensor sensor = new Sensor()
                    {
                        Id=i,
                        Code = code,
                        OffFlagCode=code,
                        TrigOff = trigerOff,
                        TrigOn = trigerOn,
                        IsEmergency = isEmergency,
                        MinOffUpMS=int.Parse(config.Get("MinFilterMillSecond")),
                        MinOnUpMS = int.Parse(config.Get("MinOnFilterMillSecond"))
                    };

                    sensors.Add(sensor);

                    if (sensor.TrigOn) {
                        if (i > 0)
                        {
                            sensor.OnFlagCode = sensors[i - 1].Code;
                        }
                        else {

                            sensor.OnFlagCode = sensors[i].Code;
                        }
                    }

                }

                Sensors = sensors;


                COM = config.Get("COM");
                BaudRate = int.Parse(config.Get("BaudRate"));
                Parity = (Parity)int.Parse(config.Get("Parity"));
                DataBits = int.Parse(config.Get("DataBits"));
                StopBits = (StopBits)int.Parse(config.Get("StopBits"));

                COMTimerInterval = int.Parse(config.Get("COMTimerInterval"));
                KanbanTimerInterval = int.Parse(config.Get("KanbanTimerInterval"));

                FilterMillSecond = int.Parse(config.Get("FilterMillSecond"));
                MinFilterMillSecond = int.Parse(config.Get("MinFilterMillSecond"));

                DeleteFileAfterRead = bool.Parse(config.Get("DeleteFileAfterRead"));
                ReadLocalFileInterval = int.Parse(config.Get("ReadLocalFileInterval"));

                WatchNodes = config.Get("WatchNodes").Split(',').ToList();

                SaveLocal = bool.Parse(config.Get("SaveLocal"));

                OnFlag = byte.Parse(config.Get("OnFlag"));
                OffFlag = byte.Parse(config.Get("OffFlag"));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string FXType { get; set; }

        public static List<Sensor> Sensors { get; set; }


        public static string COM { get; set; }
        public static int BaudRate { get; set; }
        public static Parity Parity { get; set; }
        public static int DataBits { get; set; }
        public static StopBits StopBits { get; set; }

        public static int COMTimerInterval { get; set; }
        public static int KanbanTimerInterval { get; set; }
        public static int FilterMillSecond { get; set; }
        public static int MinFilterMillSecond { get; set; }

        public static int ReadLocalFileInterval { get; set; }
        public static bool DeleteFileAfterRead { get; set; }
        public static List<string> WatchNodes { get; set; }

        public static bool SaveLocal { get; set; }
        public static byte OnFlag { get; set; }
        public static byte OffFlag { get; set; }

        public static void SaveSensors(List<Sensor> sensors) {
            string s = string.Empty;
            for (int i = 0; i < sensors.Count; i++)
            {
                s += string.Format("{0}#{1},{2},{3},{4}$", i, sensors[i].Code, sensors[i].TrigOff, sensors[i].TrigOn, sensors[i].IsEmergency);
            }
            s = s.TrimEnd('$');
            config.Set("Sensor", s);
            config.Save();
        }
    }
}