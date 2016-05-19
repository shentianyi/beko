using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Brilliantech.ClearInsight.Framework.Config;
using Brilliantech.ClearInsight.Framework;
using Brilliantech.Framwork.Utils.LogUtil;

namespace Brilliantech.ClearInsight.AppCenter.PLC
{
    public class Sensor
    {
        public Sensor()
        {
            this.OnFlag = 1;
            this.OffFlag = 0;
            this.TrigOn = true;
            this.TrigOff = true;
            this.CurrentFlag = 0;
            this.MaxFlashMS = 0;
            this.MinUpMS = 0;

            this.OnFlagMS = 0;
            this.OnFlagTime = DateTime.Now;
            this.OffFlagMS = 0;
            this.OnFlagTime = DateTime.Now;
        }
        
        public Sensor ChangeTo(byte toFlag)
        {
            DateTime currentTime = DateTime.Now;

            if (this.TrigOff)
            {
                if (this.CurrentFlag == this.OnFlag && toFlag == this.OffFlag)
                {
                    int flashTime = (int)(currentTime - this.OnFlagTime).TotalMilliseconds;

                    if (flashTime < this.MaxFlashMS)
                    {
                        this.OnFlagMS += flashTime;
                    }
                    else
                    {
                        this.OnFlagMS = flashTime;

                        if (this.OnFlagMS >= this.MinUpMS && this.Code != "X")
                        {
                            // trigger on-off up
                            Dictionary<string, string> cv = new Dictionary<string, string>();
                            cv.Add("kpi_code", ApiConfig.CycleTimeKpiCode);
                            cv.Add("code", this.Code);
                            cv.Add("value", this.OnFlagMS.ToString());

                            LogUtil.Logger.Info("code:"+this.Code+"..........value:"+this.OnFlagMS);

                            ThreadPool.QueueUserWorkItem(new WaitCallback(PostData), cv);
                        }
                        this.OnFlagTime = currentTime;
                        this.OnFlagMS = 0;
                    }
                }
                else if (this.CurrentFlag == this.OnFlag && toFlag == this.OnFlag)
                {
                    // do nothing
                }
                else if (this.CurrentFlag == this.OffFlag && toFlag == this.OnFlag)
                {
                    this.OnFlagTime = currentTime;
                    this.OnFlagMS = 0;
                }
            }

            this.CurrentFlag = toFlag;
            return null;
        }

        public void PostData(object dic) {
            Dictionary<string, string> cv = (Dictionary<string, string>)dic;
            string kpiCode = cv["kpi_code"];
            string code = cv["code"];
            string value = cv["value"];
            string time = DateTime.Now.ToString();

            AppService app = new AppService();

            app.SyncPostOnOffData(kpiCode, code, value, time);
        }


        public int Id { get; set; }

        public string Code { get; set; }

        /// <summary>
        /// 开的状态，默认为1
        /// </summary>
        public byte OnFlag { get; set; }
        /// <summary>
        /// 关的状态，默认为0
        /// </summary>
        public byte OffFlag { get; set; }

        /// <summary>
        /// 是否触发开
        /// </summary>
        public bool TrigOn { get; set; }
        /// <summary>
        /// 是否触发关
        /// </summary>
        public bool TrigOff { get; set; }

        /// <summary>
        /// 当前状态 开/关
        /// </summary>
        public byte CurrentFlag { get; set; }


        /// <summary>
        /// 最近一次开的时间
        /// </summary>
        public DateTime OnFlagTime { get; set; }
        /// <summary>
        /// 持续开的毫秒数
        /// </summary>
        public int OnFlagMS { get; set; }

        /// <summary>
        /// 最近一次关的时间
        /// </summary>
        public DateTime OffFlagTime { get; set; }
        /// <summary>
        /// 持续关的时间
        /// </summary>
        public int OffFlagMS { get; set; }

        /// <summary>
        /// 最大闪动毫秒数
        /// </summary>
        public int MaxFlashMS { get; set; }
        /// <summary>
        /// 最小触发事件时间
        /// </summary>
        public int MinUpMS { get; set; }

        /// <summary>
        /// 是否是紧急
        /// </summary>
        public bool IsEmergency { get; set; }
    }
}
