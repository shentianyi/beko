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
using System.IO;
using Brilliantech.Framwork.Utils.LogUtil;
using System.Timers;
using Brilliantech.ClearInsight.Framework.Config;
using Brilliantech.ClearInsight.Framework;
using ScmWcfService.Model.Message;

namespace Brilliantech.ClearInsight.AppCenter.PLC
{
    /// <summary>
    /// LocalDataWatchWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LocalDataWatchWindow : Window
    {
        private System.Timers.Timer scanTimer;

        private static Object fileLocker = new Object();
        private static Object dirLocker = new Object();

        private static bool unlock = true;

        public LocalDataWatchWindow()
        {
            InitializeComponent();

            this.WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitTimer();
        }



        /// <summary>
        /// init timer
        /// </summary>
        private void InitTimer()
        {
            this.scanTimer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.scanTimer)).BeginInit();
            this.scanTimer.Enabled = true;
            this.scanTimer.Interval = BaseConfig.ReadLocalFileInterval;
            this.scanTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.scanTimer_Elapsed);
            ((System.ComponentModel.ISupportInitialize)(this.scanTimer)).EndInit();
        }


        /// <summary>
        /// Scan File Timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scanTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            scanTimer.Stop();
            List<string> files = GetAllFilesFromDirectory("Data\\UnHandle", "*.txt");
            foreach (string file in files)
            {

                if (IsFileClosed(file))
                {
                    if (unlock)
                    {
                        Process(file);
                    }

                }

            }
            scanTimer.Enabled = true;
            scanTimer.Start();
        }

        /// <summary>
        /// Process File
        /// </summary>
        /// <param name="fullPath"></pairam>
        private void Process(string fullPath)
        {
            bool canMoveFile = true;
          //  string toScanDir = System.IO.Path.Combine(WPCSConfig.ScanedFileClientDir, DateTime.Today.ToString("yyyy-MM-dd"));
            string processedDir = System.IO.Path.Combine("Data\\Handle", DateTime.Today.ToString("yyyy-MM-dd"));
            try
            {
                if (IsFileClosed(fullPath))
                {

                    using (FileStream fs = new FileStream(fullPath,
                    FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                           string[] data= sr.ReadLine().Split(';');
                           AppService app = new AppService();
                           string time = DateTime.Now.ToString();
                           ResponseMessage<object> msg = app.PostPlcData(data[0],data[1].Split(',').ToList(), data[2].Split(',').ToList(), data[3]);
                           canMoveFile = !msg.http_error;

                            //sw.WriteLine(string.Join(",", codes.ToArray()) + ";" + string.Join(",", values.ToArray()) + ";" + time);
                        }
                    }

                    
                    //CheckDirectory(toScanDir);
                    //fullPath = MoveFile(fullPath, System.IO.Path.Combine(toScanDir, System.IO.Path.GetFileName(fullPath)));
                    //if (fullPath != null)
                    //{
                    //   // canMoveFile = OrderSDCConverter.ParseSDCToServer(fullPath, WPCSConfig.MachineNr, WPCSConfig.UserNr, WPCSConfig.UserGroupNr);
                    //}
                }
            }
            catch (Exception e)
            {
                LogUtil.Logger.Error(e.GetType());
                canMoveFile = false;
                LogUtil.Logger.Error(e.Message);
            }
            // 是否可以访问服务 不可以访问时保持文件不处理
            if (canMoveFile)
            {
                // 是否删除文件
                if (BaseConfig.DeleteFileAfterRead)
                {
                    // 删除文件
                    if (IsFileClosed(fullPath))
                    {
                        File.Delete(fullPath);
                        LogUtil.Logger.Warn("[Delete File After Proccessed]" + fullPath);
                    }
                }
                else
                {
                    // 移动文件
                    CheckDirectory(processedDir);
                    MoveFile(fullPath, System.IO.Path.Combine(processedDir, System.IO.Path.GetFileName(fullPath)), false);
                }
            }
        }

        /// <summary>
        /// Get all files in directory
        /// </summary>
        /// <param name="direcctory"></param>
        /// <returns></returns>
        private List<string> GetAllFilesFromDirectory(string directory, string extenstion = null)
        {
            try
            {
                if (extenstion == null)
                {
                    return Directory.GetFiles(directory.Trim()).ToList();
                }
                else
                {
                    return Directory.GetFiles(directory.Trim(), extenstion).ToList();
                }

            }
            catch (Exception e)
            {
                LogUtil.Logger.Error("[Get All File Error]" + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Move file
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        /// <param name="autoRename"></param>
        private static string MoveFile(string sourceFileName, string destFileName, bool autoRename = true)
        {
            try
            {
                lock (fileLocker)
                {
                    if (File.Exists(sourceFileName))
                    {
                        if (autoRename)
                        {
                            destFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(destFileName),
                                DateTime.Now.ToString("HHmmsss") + "_"
                                + System.IO.Path.GetFileNameWithoutExtension(sourceFileName)
                                + "_" + Guid.NewGuid().ToString() + System.IO.Path.GetExtension(sourceFileName));
                        }
                        if (File.Exists(destFileName))
                        {
                            throw new IOException("Target File Exists");
                        }
                        else
                        {
                            File.Move(sourceFileName, destFileName);
                            LogUtil.Logger.Info("Move File [From]" + sourceFileName + "[To]" + destFileName);
                            return destFileName;
                        }
                    }
                    else
                    {
                        throw new IOException("Source File No Exists");
                    }
                }
            }
            catch (Exception e)
            {
                LogUtil.Logger.Error("Move File [From]" + sourceFileName + "[To]" + destFileName + "[ERROR]" + e.Message);
            }
            return null;
        }

        /// <summary>
        /// Check directory
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="autoCreate"></param>
        /// <returns></returns>
        private static bool CheckDirectory(string dirName, bool autoCreate = true)
        {
            bool result = false;
            lock (dirLocker)
            {
                if (Directory.Exists(dirName))
                {
                    result = true;
                }
                else
                {
                    if (autoCreate)
                    {
                        Directory.CreateDirectory(dirName);
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Check file is closed 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool IsFileClosed(string fileName)
        {
            try
            {
                using (File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                LogUtil.Logger.Warn(fileName + "File not close." + e.Message);
                return false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = WindowState.Minimized;
        }


    }
}
