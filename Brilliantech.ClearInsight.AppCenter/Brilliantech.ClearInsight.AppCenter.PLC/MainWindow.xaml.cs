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


        private static byte[,] merix;

        private Dictionary<int, int> timeRecords = new Dictionary<int, int>();
        private byte[] lastRecord;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (BaseConfig.FXType.Equals("3U"))
            {
                RETURN_DATA_LENGTH = 16;
                CONTROLS = 48;
                RETURN_DATA_GROUP_LENGTH = 3;
                merix = new byte[3, 16] { 
                { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17},
                {0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37 }, 
                { 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57} };
            }
            else if (BaseConfig.FXType.Equals("1N"))
            {
                RETURN_DATA_LENGTH = 8;
                CONTROLS = 16;
                RETURN_DATA_GROUP_LENGTH = 1;
                merix = new byte[1, 16] {{ 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17 }};
            }
            else {
                LogUtil.Logger.Error("Not Set FUType");
            }
            if (RETURN_DATA_LENGTH > 0)
            {
               
                lastRecord = new byte[RETURN_DATA_LENGTH];

                /// 初始化记录数据
                for (int i = 0; i < CONTROLS; i++)
                {
                    timeRecords.Add(i, 0);
                    lastRecord[i] = 0;
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
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Logger.Error("OpenCom Error");
                        LogUtil.Logger.Error(ex.Message);
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
                    LogUtil.Logger.Info("[OK READ DATA]" + ToHexString(data));
                    if (ToHexString(lastRecord).Equals(ToHexString(data)))
                    {
                        // 找到开着的 计时
                       byte[] ons= getOnOffState(data);
                       for (int i = 0; i < ons.Length; i++) {
                           if (ons[i] == 1) {
                               timeRecords[i] = timeRecords[i] + BaseConfig.COMTimerInterval;
                           }
                       }
                    }
                    else {
                        // 如果不匹配，发送数据并重置计时器
                        byte[] old_ons = getOnOffState(lastRecord);
                        byte[] new_offs = getOnOffState(data);
                        for (int i = 0; i < old_ons.Length; i++)
                        {
                            if (old_ons[i] == 1 && new_offs[i] == 0)
                            {

                                // #TODO 发送数据，多个发送

                                // 重置计时
                                timeRecords[i] = 0;
                            }
                            else if(old_ons[i]==1 && new_offs[i]==1){
                                timeRecords[i] = timeRecords[i] + BaseConfig.COMTimerInterval;
                            }
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
                string bitstring = new string(Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2).Reverse().ToArray());

                LogUtil.Logger.Error(bitstring);
                
                for (int j = 0; j <bitstring.Length; j++)
                {
                    state[16 * i + j] = bitstring[j].Equals((char)49) ? (byte)1 : (byte)0;//Convert.ToByte(Convert.ToString(bitstring[j],10));//Encoding.Default.GetBytes(bitstring[j]);//Convert.ToByte( Convert.ToInt16(bitstring[j]));
                }
            }
            return state; 
        }

        /// <summary>
        /// 定时发送数据读
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            byte[] cmd = null;
            if (BaseConfig.FXType.Equals("3U"))
            {
                cmd = new byte[] { 0x02, 0x30, 0x31, 0x30, 0x43, 0x38, 0x30, 0x36, 0x03, 0x37, 0x35 };
            }
            else if (BaseConfig.FXType.Equals("1N")) 
            {
                cmd = new byte[] { 0x02, 0x30, 0x31, 0x30, 0x43, 0x38, 0x30, 0x32, 0x03, 0x33, 0x35 };
            }
            if (cmd != null)
            {
                sp.Write(cmd, 0, cmd.Length);
                LogUtil.Logger.Info("[Send CMD]" + ToHexString(cmd));
            }
            else {
                LogUtil.Logger.Error("Not Set FUType");
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
