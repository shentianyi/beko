using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Brilliantech.ClearInsight.Framework.Config;
using Brilliantech.ClearInsight.Framework;
using Brilliantech.Framwork.Utils.LogUtil;
using System.IO;

namespace Brilliantech.ClearInsight.Framework
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

            this.OnFlagCount = 0;
            this.OffFlagCount = 0;
        }

        public Sensor ChangeTo(byte toFlag)
        {
            DateTime currentTime = DateTime.Now;

            if (this.TrigOff)
            {
                // ON-OFF
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
                            cv.Add("code", this.OffFlagCode);
                           cv.Add("value", this.OnFlagMS.ToString());
                          //  cv.Add("value", new Random().Next(30000).ToString());
                            this.OffFlagCount++;
                            LogUtil.Logger.Info("[off].....code:" + this.Code + "..........value:" + this.OnFlagMS + "...........count:" + this.OffFlagCount);

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

            if (this.TrigOn)
            {
                //OFF-ON
                if (this.CurrentFlag == this.OffFlag && toFlag == this.OnFlag)
                {
                    int flashTime = (int)(currentTime - this.OffFlagTime).TotalMilliseconds;
                    if (flashTime < this.MaxFlashMS)
                    {
                        this.OffFlagMS += flashTime;
                    }
                    else
                    {
                        this.OffFlagMS = flashTime;
                        if (this.OffFlagMS >= this.MinUpMS && this.Code != "X")
                        {
                            Dictionary<string, string> cv = new Dictionary<string, string>();
                            cv.Add("kpi_code", ApiConfig.MovingTimeKpiCode);
                            cv.Add("code", this.OnFlagCode);
                              9cv.Add("value", this.OffFlagMS.ToString());
                            //cv.Add("value", new Random().Next(30000).ToString());
                            this.OnFlagCount++;
                            LogUtil.Logger.Info("[on].....code:" + this.Code + "..........value:" + this.OffFlagMS + "...........count:" + this.OnFlagCount);

                            ThreadPool.QueueUserWorkItem(new WaitCallback(PostData), cv);
                        }
                        this.OffFlagTime = currentTime;
                        this.OnFlagMS = 0;
                    }

                }
                else if (this.CurrentFlag == this.OffFlag && toFlag == this.OffFlag)
                {
                    // do nothing
                }
                else if (this.CurrentFlag == this.OnFlag && toFlag == this.OffFlag)
                {
                    this.OffFlagTime = currentTime;
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

        public static void SaveLocal(string kpiCode, string codes, string values, string time)
        {
            // save the data in local
            string dir = System.IO.Path.Combine("Data\\UnHandle");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (FileStream fs = new FileStream(System.IO.Path.Combine(dir, DateTime.Now.ToString("yyyy-MM-dd HH-mm-sss") + Guid.NewGuid().ToString() + ".txt"),
 FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(kpiCode + ";" + codes + ";" + values + ";" + time);
                }
            }
        }

        public int Id { get; set; }

        public string Code { get; set; }

        public string OnFlagCode { get; set; }
        public string OffFlagCode { get; set; }

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
        /// 开的次数
        /// </summary>
        public int OnFlagCount { get; set; }

        /// <summary>
        /// 最近一次关的时间
        /// </summary>
        public DateTime OffFlagTime { get; set; }
        /// <summary>
        /// 持续关的时间
        /// </summary>
        public int OffFlagMS { get; set; }

        /// <summary>
        /// 关的次数
        /// </summary>
        public int OffFlagCount { get; set; }

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
