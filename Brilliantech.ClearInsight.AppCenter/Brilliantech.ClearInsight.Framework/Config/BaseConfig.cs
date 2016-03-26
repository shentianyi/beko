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
                if (FXType != "Q02U")
                {
                    Sensor = config.Get("Sensor").Split(',');
                }
                else {
                    string[] sensors = config.Get("Sensor").Split(',');
                    Sensor = new string[sensors.Length];

                    for (int i = 0; i < sensors.Length; i++) {
                        Sensor[i] = sensors[i].Split('#')[1];
                    }
                }
                COM = config.Get("COM");
                BaudRate = int.Parse(config.Get("BaudRate"));
                Parity = (Parity)int.Parse(config.Get("Parity"));
                DataBits = int.Parse(config.Get("DataBits"));
                StopBits = (StopBits)int.Parse(config.Get("StopBits"));

                COMTimerInterval = int.Parse(config.Get("COMTimerInterval"));
                KanbanTimerInterval =  int.Parse(config.Get("KanbanTimerInterval"));
                FilterMillSecond = int.Parse(config.Get("FilterMillSecond"));
                DeleteFileAfterRead = bool.Parse(config.Get("DeleteFileAfterRead"));
                ReadLocalFileInterval = int.Parse(config.Get("ReadLocalFileInterval"));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string FXType { get; set; }
        public static string[] Sensor { get; set; }

        public static string COM { get; set; }
        public static int BaudRate { get; set; }
        public static Parity Parity { get; set; }
        public static int DataBits { get; set; }
        public static StopBits StopBits { get; set; }

        public static int COMTimerInterval { get; set; }
        public static int KanbanTimerInterval { get; set; }
        public static int FilterMillSecond { get; set; }

        public static int ReadLocalFileInterval { get; set; }
        public static bool DeleteFileAfterRead { get; set; }
    }
}