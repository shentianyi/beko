using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Brilliantech.Framwork.Utils.ConfigUtil;

namespace Brilliantech.ClearInsight.Framework.Config
{
    public class ApiConfig
    {
        private static ConfigUtil config;
        private static string host;
        private static string port;
        private static string token;

        static ApiConfig()
        {
            try
            {
                config = new ConfigUtil("API", "Ini/api.ini");

                host = config.Get("Host");
                ApiUri = config.Get("ApiUri");
                BaseUri = host + ApiUri; 
                PlcOnOffPostAction = config.Get("PlcOnOffPostAction");
                PlanAction = config.Get("PlanAction");
                ConfirmPlanAction = config.Get("ConfirmPlanAction");


                CycleTimeKpiCode = config.Get("CycleTimeKpiCode");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
      
        public static string Host
        {
            get { return host; }
            set
            {
                host = value;
                BaseUri =  host + ApiUri;
                config.Set("Host", value);
                config.Save();
            }
        }

        public static string Token
        {
            get { return token; }
            set
            {
                token = value; 
                config.Set("Token", value);
                config.Save();
            }
        }
        
        public static string ApiUri { get; set; }
        public static string BaseUri { get; set; }
        public static string PlcOnOffPostAction { get; set; } 
        public static string PlanAction { get; set; }
        public static string ConfirmPlanAction { get; set; }
        public static string CycleTimeKpiCode { get; set; }
        public static string MovingTimeKpiCode { get; set; }
    }
}