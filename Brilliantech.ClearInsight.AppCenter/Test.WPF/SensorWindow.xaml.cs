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
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;
using System.Collections;

namespace Test.WPF
{
    /// <summary>
    /// SensorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SensorWindow : Window
    {
        static byte[] closeCmd = new byte[] { 0x02, 0x30, 0x30, 0x30, 0x33, 0x30, 0x36, 0x31, 0x36, 0x46, 0x42, 0x32, 0x32, 0x03, 0x39, 0x34 };

        static byte[] openCmd = new byte[] { 0x02, 0x30, 0x45, 0x30, 0x33, 0x30, 0x36, 0x31, 0x36, 0x46, 0x42, 0x32, 0x32, 0x03, 0x39, 0x34 };

        SerialPort sp;


        private System.Timers.Timer timer;

        private Queue queue1 = new Queue();
        private Queue sendMessageQueue;
        private Thread sendMessageThread;
        private ManualResetEvent sendEvent = new ManualResetEvent(false);


        public SensorWindow()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            sp = new SerialPort(ComTB.Text, 9600);

            sp.Open();

            sendMessageThread = new Thread(this.SendMessageThread);
            sendMessageQueue = Queue.Synchronized(queue1);
            sendMessageThread.Start();


            timer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.timer)).BeginInit();
            timer.Enabled = false;
            timer.Interval = int.Parse(IntervelTB.Text);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            ((System.ComponentModel.ISupportInitialize)(this.timer)).EndInit();


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closeSP();
            sendMessageThread.Abort();
        }

        bool open = false;
        int i = 0;
        int j = 0;
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (open)
            {

                j++;
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (System.Windows.Forms.MethodInvoker)delegate()
                {
                    OffCountLab.Content = j;
                });
                StartSendMessage(closeCmd);
                //sp.Write(closeCmd, 0, closeCmd.Length);
            }
            else
            {
                StartSendMessage(openCmd);
              //  sp.Write(openCmd, 0, openCmd.Length);
            }

            i += 1;
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (System.Windows.Forms.MethodInvoker)delegate()
            {
                CountLab.Content = i;
            });

            open = !open;
        }


        private void StartSendMessage(byte[] data)
        {
            sendMessageQueue.Enqueue(data);
            sendEvent.Set();
        }


        private void SendMessageThread()
        {
            while (true)
            {
                while (sendMessageQueue.Count > 0)
                {
                    SendMessage((byte[])sendMessageQueue.Dequeue());
                }
                sendEvent.WaitOne();
                sendEvent.Reset();

            }
        }
         
        private void SendMessage(byte[] data)
        {

           // Thread.Sleep(100);
            try
            {
             

                sp.Write(data, 0, data.Length);   
              
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }


        private void button2_Click(object sender, RoutedEventArgs e)
        {
            open = false;
            i = 0;
            timer.Enabled = true;
            timer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            timer.Close();
            timer = null;
            closeSP();
            sp = null;
        }

        private void closeSP() {
            if (sp != null && sp.IsOpen)
            {
                sp.Close();
            }
        }
    }
}
