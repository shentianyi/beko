using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using Brilliantech.Framwork.Utils.LogUtil;
using System.Collections;
using Brilliantech.ClearInsight.Framework.Config;
using Brilliantech.ClearInsight.Framework;
using Brilliantech.ClearInsight.Framework.Model;
using ScmWcfService.Model.Message;

namespace Brilliantech.ClearInsight.AppCenter.PLC
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort sp;


        private System.Timers.Timer timer;
        
        private static int RETURN_DATA_LENGTH=0;
        private static int RETURN_DATA_GROUP_LENGTH = 0;
        private static int CONTROLS = 0;


        private static string[] merix;

        private Dictionary<int, float> timeRecords = new Dictionary<int, float>();
        private byte[] lastRecord;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //AppService app = new AppService();
            //ResponseMessage<List<ProductionPlan>> plans = app.GetPlans("P1", DateTime.Today.ToShortDateString());
            merix = BaseConfig.Sensor;

            if (BaseConfig.FXType.Equals("3U"))
            {
                RETURN_DATA_LENGTH = 16;
                CONTROLS = 48;
                RETURN_DATA_GROUP_LENGTH = 3;
               // merix = new string[48] { "X0", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "X10", "X11", "X12", "X13", "X14", "X15", "X16", "X17", "X20", "X21", "X22", "X23", "X24", "X25", "X26", "X27", "X30", "X31", "X32", "X33", "X34", "X35", "X36", "X37 ", "X40", "X41", "X42", "X43", "X44", "X45", "X46", "X47", "X50", "X51", "X52", "X53", "X54", "X55", "X56", "X57" };
            }
            else if (BaseConfig.FXType.Equals("1N"))
            {
                RETURN_DATA_LENGTH = 8;
                CONTROLS = 16;
                RETURN_DATA_GROUP_LENGTH = 1;
               // merix = new string[16] { "X0", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "X10", "X11", "X12", "X13", "X14", "X15", "X16", "X17" };
            }
            else
            {
                LogUtil.Logger.Error("Not Set FUType");
            }
            if (RETURN_DATA_LENGTH > 0)
            {
                lastRecord = new byte[RETURN_DATA_LENGTH];

                /// 初始化记录数据
                for (int i = 0; i < CONTROLS; i++)
                {
                    timeRecords.Add(i, 0);
                }
                for (int i = 0; i < RETURN_DATA_LENGTH; i++)
                {
                    lastRecord[i] = 0x30;
                }


                if (openCom())
                {
                    initTimer();
                      timer.Start();
                }
            }
        }
        private void initTimer()
        {
            timer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.timer)).BeginInit();
            timer.Enabled = false;
            timer.Interval = BaseConfig.COMTimerInterval;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            ((System.ComponentModel.ISupportInitialize)(this.timer)).EndInit();
        }

        int openCount = 0;

        bool openCom()
        {
            if (sp == null)
            {
                sp = new SerialPort(BaseConfig.COM, BaseConfig.BaudRate);
                sp.Parity = BaseConfig.Parity;
                sp.DataBits = BaseConfig.DataBits;
                sp.StopBits = BaseConfig.StopBits;


                if (!sp.IsOpen)
                {
                    try
                    {
                        sp.Open();

                        sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
                        LogUtil.Logger.Info("OpenCom Success");

                        openCount = 0;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Logger.Error("OpenCom Error");
                        LogUtil.Logger.Error(ex.Message);
                        openCount++;
                        if (openCount < 5) {
                            openCom();
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 接收到数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
          //  System.Threading.Thread.Sleep(50);
            try
            {
                //string data = sp.ReadExisting();
                //LogUtil.Logger.Info("[Read Data]" + data);

                byte[] data = new byte[sp.BytesToRead];
                sp.Read(data, 0, data.Length);

                if (data.Length == RETURN_DATA_LENGTH)
                {
                    //LogUtil.Logger.Info("[LAST DATA]" + ToHexString(lastRecord));
                    //LogUtil.Logger.Info("[OK READ DATA]" + ToHexString(data));

                    if (ToHexString(lastRecord).Equals(ToHexString(data)))
                    {
                        // 找到开着的 计时
                        byte[] ons = getOnOffState(data);
                        for (int i = 0; i < ons.Length; i++)
                        {
                            if (ons[i] == 1)
                            {
                                timeRecords[i] = timeRecords[i] + BaseConfig.COMTimerInterval;
                            }
                        }
                    }
                    else
                    {
                        // 如果不匹配，发送数据并重置计时器
                        byte[] old_ons = getOnOffState(lastRecord);
                        byte[] new_offs = getOnOffState(data);

                        List<string> codes = new List<string>();
                        List<string> values = new List<string>();
                        for (int i = 0; i < old_ons.Length; i++)
                        {

                            if (old_ons[i] == 1 && new_offs[i] == 0)
                            {
                                timeRecords[i] = timeRecords[i] + BaseConfig.COMTimerInterval;
                                // #TODO 发送数据，多个发送
                                codes.Add(merix[i].ToString());
                                values.Add(timeRecords[i].ToString());
                                // 重置计时
                                timeRecords[i] = 0;
                            }
                            else if (old_ons[i] == 1 && new_offs[i] == 1)
                            {
                                timeRecords[i] = timeRecords[i] + BaseConfig.COMTimerInterval;
                            }

                        }
                        if (codes.Count > 0)
                        {
                            AppService app = new AppService();
                            app.PostPlcData(codes, values);
                        }
                        lastRecord = data;
                    }
                }
                else
                {
                    LogUtil.Logger.Info("[BAD Read Data]" + ToHexString(data));
                }
                // 

            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error("Read Com Error");
                LogUtil.Logger.Error(ex.Message);
            }
        }

        byte[] getOnOffState(byte[] data) {
            byte[] state = new byte[RETURN_DATA_GROUP_LENGTH*16];
            for (int i = 0; i < state.Length; i++) {
                state[i] = 0;
            }
            for (int i = 0; i < RETURN_DATA_GROUP_LENGTH; i++)
            {
                byte[] group_data = new byte[4] { data[i * 4 + 1 + 2], data[i * 4 + 1 + 3], data[i * 4 + 1 + 0], data[i * 4 + 1 + 1] };
            //   group_data=new byte[4]{0x30,0x30,0x30,0x31};
                //LogUtil.Logger.Error(ASCIIEncoding.ASCII.GetString(group_data));
                //LogUtil.Logger.Error(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16));
                //LogUtil.Logger.Error(Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2).Reverse());
                //LogUtil.Logger.Error(Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2).Reverse().ToArray());

               // string bitstring = string.Empty;
                //for (int j = 0; j < group_data.Length; j++) { 
                 
                //}
                string bitstring = new string(Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2).Reverse().ToArray());

               //  LogUtil.Logger.Error(bitstring);
                
                for (int j = 0; j <bitstring.Length; j++)
                {
                    state[16 * i + j] =  bitstring[j].Equals((char)49) ? (byte)1 : (byte)0;//Convert.ToByte(Convert.ToString(bitstring[j],10));//Encoding.Default.GetBytes(bitstring[j]);//Convert.ToByte( Convert.ToInt16(bitstring[j]));
                }
            }
                LogUtil.Logger.Info(state);
            return state; 
        }

        /// <summary>
        /// 定时发送数据读
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (sp == null || (sp.IsOpen==false)) {
                    openCom();
                }
                byte[] cmd = null;
                if (BaseConfig.FXType.Equals("3U"))
                {
                    cmd = new byte[] { 0x02, 0x30, 0x31, 0x30, 0x43, 0x38, 0x30, 0x36, 0x03, 0x37, 0x35 };
                }
                else if (BaseConfig.FXType.Equals("1N"))
                {
                    cmd = new byte[] { 0x02, 0x30, 0x31, 0x30, 0x43, 0x38, 0x30, 0x32, 0x03, 0x37, 0x31 };
                }
                if (cmd != null)
                {
                    sp.Write(cmd, 0, cmd.Length);
                    LogUtil.Logger.Info("[Send CMD]" + ToHexString(cmd));
                }
                else
                {
                    LogUtil.Logger.Error("Not Set FUType");
                }
            }
            catch (Exception ex) {
                if (openCount < 5) {
                    openCom();
                }

                LogUtil.Logger.Error("Send Com Data Error");
                LogUtil.Logger.Error(ex.Message);

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sp != null)
            {
                try
                {
                    sp.Close();
                    LogUtil.Logger.Info("Close Success");
                    if (timer != null)
                    {
                        timer.Enabled = false;
                        timer.Stop();
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.Logger.Error("Close Error");
                    LogUtil.Logger.Error(ex.Message);
                }

            }
        }

        public   string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF"
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2")+" ");
                }

                hexString = strB.ToString();
            }
            return hexString;
        }
    }
}
