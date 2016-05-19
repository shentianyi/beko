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
using System.Windows.Threading;
using System.IO;
using System.Threading;

namespace Brilliantech.ClearInsight.AppCenter.PLC
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort sp;


        private System.Timers.Timer sendCmdTimer;
        
        private static int RETURN_DATA_LENGTH=0;
        private static int RETURN_DATA_GROUP_LENGTH = 0;
        private static int CONTROLS = 0;


        private static string[] merix;

        private Dictionary<int, float> timeRecords = new Dictionary<int, float>();
        private Dictionary<int, float> timeLastRecords = new Dictionary<int, float>();

        private Dictionary<int, int> recordCount = new Dictionary<int, int>();
        private Dictionary<int, DateTime> timeTicker = new Dictionary<int, DateTime>();
        private Dictionary<int, DateTime> timeLastTicker = new Dictionary<int, DateTime>();

        private byte[] lastRecord;

        private object locker = new object();



        // queue
        private Queue comDataQ = new Queue();
        private Queue receiveMessageQueue;
        private Thread receiveMessageThread;
        private ManualResetEvent receivedEvent = new ManualResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            merix = BaseConfig.Sensor;

            if (BaseConfig.FXType.Equals("3U"))
            {
                RETURN_DATA_LENGTH = 16;
                CONTROLS = 48;
                RETURN_DATA_GROUP_LENGTH = 3;
             }
            else if (BaseConfig.FXType.Equals("1N"))
            {
                RETURN_DATA_LENGTH = 8;
                CONTROLS = 16;
                RETURN_DATA_GROUP_LENGTH = 1;
           }
            else if (BaseConfig.FXType == "Q02U")
            {
                // to do
                RETURN_DATA_LENGTH = 124;
                // CONTROLS = 960;
                CONTROLS = 992;
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
                    timeLastRecords.Add(i, 0);

                    recordCount.Add(i, 0);

                    DateTime current = DateTime.Now;
                    timeTicker.Add(i, current);
                    timeLastTicker.Add(i, current);

                }
                for (int i = 0; i < RETURN_DATA_LENGTH; i++)
                {
                    lastRecord[i] = 0x30;

                    if(i%2==0)
                    lastRecord[i] =(byte)Convert.ToChar(  BaseConfig.OffFlag.ToString());
                }


                if (openCom())
                {

                }

                if (BaseConfig.FXType != "Q02U")
                {
                    initTimer();
                    sendCmdTimer.Start();
                }
            }

            if (BaseConfig.SaveLocal)
            {
                new LocalDataWatchWindow().Show();
            }


            receiveMessageThread = new Thread(this.ReceiveMessageThread);
            receiveMessageQueue = Queue.Synchronized(comDataQ);
            receiveMessageThread.Start();
        }

        private void initTimer()
        {
            sendCmdTimer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.sendCmdTimer)).BeginInit();
            sendCmdTimer.Enabled = false;
            sendCmdTimer.Interval = BaseConfig.COMTimerInterval;
            sendCmdTimer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            ((System.ComponentModel.ISupportInitialize)(this.sendCmdTimer)).EndInit();
        }

        int openCount = 0;

        bool openCom()
        {
            if (sp == null || (sp.IsOpen==false))
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

        byte[] g_data=new byte[124];
        int g_index = 0;
       // int count = 0;
        /// <summary>
        /// 接收到数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //if (BaseConfig.FXType == "Q02U")
                //{
                //   // System.Threading.Thread.Sleep(50);
                //}

                byte[] data_all = new byte[sp.BytesToRead];
                sp.Read(data_all, 0, data_all.Length);
                //LogUtil.Logger.Info(data_all);
                //LogUtil.Logger.Info("-------------------------------------------");
                if (BaseConfig.FXType == "Q02U")
                {
                    string check = ToHexString(data_all);
                    // LogUtil.Logger.Info(check);
                    if (check.StartsWith("EE 1A") && check.EndsWith("0D 0A") && data_all.Length == 124)
                    {
                        // 完整的数据
                        //for (int i = 0; i < data.Length; i++) {
                        //    g_data[i] = data[i];
                        //    g_index = i;
                        //}
                    }
                    else if (check.StartsWith("EE 1A"))
                    {
                        for (int i = 0; i < data_all.Length; i++)
                        {
                            g_data[i] = data_all[i];
                        }
                        g_index = data_all.Length;
                    }
                    else if (check.EndsWith("0D 0A"))
                    {
                        for (int i = 0; i < data_all.Length; i++)
                        {
                            g_data[i + g_index] = data_all[i];
                        }
                        data_all = g_data;
                        g_index = 0;
                        if (data_all.Length % RETURN_DATA_LENGTH == 0 && data_all.Length > 0)
                        {

                            StartReceivedMessage(
                                new Message()
                                {
                                    Data = data_all,
                                    ReceivedTime = DateTime.Now
                                });
                        }
                    }
                    else
                    {
                        for (int i = 0; i < data_all.Length; i++)
                        {
                            g_data[i + g_index] = data_all[i];
                        }
                        g_index += data_all.Length;
                    }
                }
                else
                {
                    StartReceivedMessage(
                        new Message()
                        {
                            Data = data_all,
                            ReceivedTime = DateTime.Now
                        });
                }
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error("Receive Data Error");

                LogUtil.Logger.Error(ex.StackTrace);

                LogUtil.Logger.Error(ex.Message);
            }

        }

        private void StartReceivedMessage(Message receivedMessage)
        {
            receiveMessageQueue.Enqueue(receivedMessage);
            receivedEvent.Set();
        }
         
        private void ReceiveMessageThread()
        {
            while (true)
            {
                while (receiveMessageQueue.Count > 0)
                {
                    ReceivedMessage((Message)receiveMessageQueue.Dequeue());
                }

                receivedEvent.WaitOne();
                receivedEvent.Reset();
            }
        }

        private void ReceivedMessage(Message message) 
        {
            try
            {

                byte[] data_all = message.Data;
                DateTime current = message.ReceivedTime;

                if (data_all.Length % RETURN_DATA_LENGTH == 0)
                {
                    int group = data_all.Length / RETURN_DATA_LENGTH;

                    for (int g = 0; g < group; g++)
                    {
                        byte[] data = new byte[RETURN_DATA_LENGTH];
                        int ggg = 0;
                        for (int gg = g * RETURN_DATA_LENGTH; ggg < RETURN_DATA_LENGTH; gg++, ggg++)
                        {
                            data[ggg] = data_all[gg];
                        }


                        if (ByteArrayEqual(lastRecord, data))
                        {
                            // 找到开着的 计时
                            byte[] ons = getOnOffState(data);
                            for (int i = 0; i < ons.Length; i++)
                            {
                                if (ons[i] == 0)
                                {
                                    timeTicker[i] = current;
                                }
                            }
                        }
                        else
                        {
                            Dictionary<string, string> CodeValue = new Dictionary<string, string>();

                            // 如果不匹配，发送数据并重置计时器
                            byte[] old_ons = getOnOffState(lastRecord);
                            byte[] new_offs = getOnOffState(data);
                            

                            for (int i = 0; i < old_ons.Length; i++)
                            {
                                
                               // if (old_ons[i] == 1 && new_offs[i] == 0)
                                if (old_ons[i] == BaseConfig.OnFlag && new_offs[i] == BaseConfig.OffFlag)
                                {
                                    if (merix[i] != "X")
                                    {

                                        int time = (int)(current - timeLastTicker[i]).TotalMilliseconds;

                                        // 记录上一次时间
                                        timeLastTicker[i] = current;

                                        if (time < BaseConfig.MinFilterMillSecond)
                                        {
                                           // if (BaseConfig.WatchNodes.IndexOf(merix[i]) > -1)
                                                LogUtil.Logger.Info("flash:" + merix[i].ToString() + ":" + time);
                                            timeLastRecords[i] += time;
                                            // 说明是信号波动
                                            // 保持最后的时间不变, 即累计这个波动时间
                                        }
                                        else
                                        {
                                            // 如果是大于发送时间，就将数据发送到服务器
                                            if (timeLastRecords[i] < BaseConfig.MinFilterMillSecond)
                                            {
                                                timeRecords[i] = timeLastRecords[i] + (int)(current - timeTicker[i]).TotalMilliseconds;
                                            }
                                            else
                                            {
                                                timeRecords[i] = 0;
                                                timeRecords[i] = (int)(current - timeTicker[i]).TotalMilliseconds;
 
                                                LogUtil.Logger.Error(i + ":time...................." + timeRecords[i] + ":" + current.ToString() + ":" + timeTicker[i].ToString());
                                            }
                                             

                                            if (timeRecords[i] >= BaseConfig.FilterMillSecond)
                                            {
                                                if (merix[i] != "X")
                                                {
                                                    recordCount[i] += 1;

                                                   // if (BaseConfig.WatchNodes.IndexOf(merix[i]) > -1)
                                                        LogUtil.Logger.Info("i:" + i + ":code: " + merix[i].ToString() + "count:" + recordCount[i] + ":" + timeLastRecords[i].ToString() + ":" + timeRecords[i].ToString());

                                                   // LogUtil.Logger.Error(i+":time...................." + timeRecords[i]);

                                                    CodeValue.Add(merix[i].ToString(), timeRecords[i].ToString());
                                                  //  timeRecords[i] = 0;
                                                }
                                            }
                                            // 将计时器设置到当前时间
                                            timeTicker[i] = current;
                                            timeLastRecords[i] = 0;
                                        }

                                    }
                                }
                                //else if (old_ons[i] == 1 && new_offs[i] == 1)
                                else if (old_ons[i] == BaseConfig.OnFlag && new_offs[i] == BaseConfig.OnFlag)
                                {
                                    // timeRecords[i] = timeRecords[i] + (int)(current - timeTicker[i]).TotalMilliseconds; //timeRecords[i] + BaseConfig.COMTimerInterval;
                                }
                                //else if (old_ons[i] == 0 && new_offs[i] == 1)
                                else if (old_ons[i] == BaseConfig.OffFlag && new_offs[i] == BaseConfig.OnFlag)
                                {
                                    timeTicker[i] = current;
                                    timeLastTicker[i] = current;
                                }

                            }


                            if (CodeValue.Count > 0)
                            {
                               ThreadPool.QueueUserWorkItem(new WaitCallback(postData), CodeValue);
                            }
                        }

                        for (int j = 0; j < data.Length; j++)
                        {
                            lastRecord[j] = data[j];
                        }
                    }
                }
                else
                {
                    // LogUtil.Logger.Info("[BAD Read Data]" + ToHexString(data));
                }
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error("Read Com Error");

                LogUtil.Logger.Error(ex.StackTrace);

                LogUtil.Logger.Error(ex.Message);
            }
        
        }



        void postData(object cv)
        {
            Dictionary<string, string> CodeValue = (Dictionary<string, string>)cv;
            List<string> codes = CodeValue.Keys.ToList();
            List<string> values = CodeValue.Values.ToList();
            string time = DateTime.Now.ToString();
            foreach (string node in BaseConfig.WatchNodes)
            {
                if (CodeValue.Keys.Contains(node))
                {
                    LogUtil.Logger.Info(codes);
                    break;
                }
            }
            if (BaseConfig.SaveLocal)
            {
                SaveLocal(codes, values, time);
            }
            else
            {
                AppService app = new AppService();
                ResponseMessage<object> msg = app.SyncPostPlcData(codes, values, time);
                if (msg != null && msg.http_error)
                {
                    SaveLocal(codes, values, time);
                }
            }
        }

        private void SaveLocal(List<string> codes, List<string> values, string time)
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
                    sw.WriteLine(string.Join(",", codes.ToArray()) + ";" + string.Join(",", values.ToArray()) + ";" + time);
                }
            }
        }

        byte[] getOnOffState(byte[] data)
        {
            byte[] state = new byte[CONTROLS];
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = 0;
            }
            if (BaseConfig.FXType == "Q02U")
            {
                for (int i = 0; i < data.Length; i += 2)
                {

                    byte[] group_data = new byte[2] { data[i + 0], data[i + 1] };

                    for (int j = 0; j < group_data.Length; j++)
                    {
                        string bitstring = new string(Convert.ToString(group_data[j], 2).Reverse().ToArray());
                        for (int m = 0; m < bitstring.Length; m++)
                        {
                            state[i * 8 + j * 8 + m] = bitstring[m].Equals((char)49) ? (byte)1 : (byte)0;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < RETURN_DATA_GROUP_LENGTH; i++)
                {
                    byte[] group_data = new byte[4] { data[i * 4 + 1 + 2], data[i * 4 + 1 + 3], data[i * 4 + 1 + 0], data[i * 4 + 1 + 1] };

                    string bitstring = new string(Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2).Reverse().ToArray());

                    for (int j = 0; j < bitstring.Length; j++)
                    {
                        state[16 * i + j] = bitstring[j].Equals((char)49) ? (byte)1 : (byte)0;//Convert.ToByte(Convert.ToString(bitstring[j],10));//Encoding.Default.GetBytes(bitstring[j]);//Convert.ToByte( Convert.ToInt16(bitstring[j]));
                    }
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
            //     DetectCom();
            SecndCmd();
        }

        private void DetectCom()
        {
            try
            {
                if (sp == null || (sp.IsOpen == false))
                {
                    openCom();
                }
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error("Open Com Error");
                LogUtil.Logger.Error(ex.Message);
            }
        }

        private void StartCmd() 
        {
            Thread cmdThread = new Thread(SecndCmd);
            cmdThread.Start();
        }

        private void SecndCmd()
        {
            try
            {
               
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
                    // this.Dispatcher.Invoke(DispatcherPriority.Normal, (System.Windows.Forms.MethodInvoker)delegate()
                    //    {
                    sp.Write(cmd, 0, cmd.Length);
                    //  });
                  //  LogUtil.Logger.Info("[Send CMD]" + ToHexString(cmd));
                }
                else
                {
                    LogUtil.Logger.Error("Not Set FUType");
                }
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error("Send Cmd Error");
                LogUtil.Logger.Error(ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (sp != null)
            {
                try
                {
                    if (sp.IsOpen)
                    {
                        sp.Close();
                        LogUtil.Logger.Info("Close Success");
                    }
                    if (sendCmdTimer != null)
                    {
                        sendCmdTimer.Enabled = false;
                        sendCmdTimer.Stop();
                    }

                }
                catch (Exception ex)
                {
                    LogUtil.Logger.Error("Close Error");
                    LogUtil.Logger.Error(ex.Message);
                }

            }

            receiveMessageThread.Abort();

            App.Current.Shutdown();
        }


        private bool ByteArrayEqual(byte[] a1, byte[] a2) {
            if (a1 == null || a2 == null)
                return false;
            if (a1.Length != a2.Length) 
                return false;
            for (int i = 0; i < a1.Length; i++) {
                if (a1[i] != a2[i])
                    return false;
            }
            return true;
        }

        private  string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF"
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2")+" ");
                }

                hexString = strB.ToString().TrimEnd();
            }
            return hexString;
        }
    }
}
