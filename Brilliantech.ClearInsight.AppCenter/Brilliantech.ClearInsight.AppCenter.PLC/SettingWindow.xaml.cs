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
using Brilliantech.ClearInsight.Framework.Config;
using Brilliantech.ClearInsight.Framework;
using System.Windows.Threading;

namespace Brilliantech.ClearInsight.AppCenter.PLC
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }
        int colCount = 10;
        int rowCount = 1;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < BaseConfig.Sensors.Count; i++)
            {
                BaseConfig.Sensors[i].FlagChanged += new Sensor.FlagChangedEventHandler(SettingWindow_FlagChanged);
            }
            colCount = SensorGrid.ColumnDefinitions.Count;

            rowCount = BaseConfig.Sensors.Count / colCount + (BaseConfig.Sensors.Count % colCount > 0 ? 1 : 0);

            for (int i = 0; i < rowCount; i++)
            {
                SensorGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(170)
                });
            }

            for (int i = 0; i < BaseConfig.Sensors.Count; i++)
            {
                int row = i / colCount;
                int col = i % colCount;

                Sensor sensor = BaseConfig.Sensors[i];

                StackPanel sp = new StackPanel() { Tag = i };

                string code = sensor.Code;
                Button button = new Button() { Content = sensor.Code, Height = 50, Tag = "button_" + i, Margin = new Thickness(0, 0, 0, 0) };
             
                sp.Children.Add(button);

                CheckBox enableCB = new CheckBox() { Content = "Enabled", IsChecked = !sensor.Code.Equals("X"), Margin = new Thickness(0, 5, 0, 0), Tag = "enable_" + i };

                sp.Children.Add(enableCB);

                TextBox tb = new TextBox() { Text=sensor.Code,Margin= new Thickness(0,5,0,0), Height = 20, Tag = "code_" + i };
       

                sp.Children.Add(tb);

                CheckBox cycleCB = new CheckBox() { Content = "Cycle", IsChecked = sensor.TrigOff, Margin = new Thickness(0, 5, 0, 0), Tag = "cycle_" + i };
       
                sp.Children.Add(cycleCB);


                CheckBox movingCB = new CheckBox() { Content = "Moving", IsChecked = sensor.TrigOn, Margin = new Thickness(0, 5, 0, 0), Tag = "moving_" + i };
         
                sp.Children.Add(movingCB);


                cycleCB.Checked += new RoutedEventHandler(cycleCB_Checked);
                movingCB.Checked += new RoutedEventHandler(cycleCB_Checked);

                CheckBox scramCB = new CheckBox() { Content = "Scram", IsChecked = sensor.IsEmergency, Margin = new Thickness(0, 5, 0, 0), Tag = "scram_" + i };
              
                sp.Children.Add(scramCB);
                scramCB.Checked += new RoutedEventHandler(scramCB_Checked);

                sp.SetValue(Grid.RowProperty, row);
                sp.SetValue(Grid.ColumnProperty, col);
                SensorGrid.Children.Add(sp);

            }
        }

        void cycleCB_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            string tag = cb.Tag.ToString().Split('_')[1];

            CheckBox scramCB = findByTag("scram_" + tag) as CheckBox;
            scramCB.IsChecked = false;
        }

        void scramCB_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            string tag = cb.Tag.ToString().Split('_')[1];

            CheckBox cycleCB = findByTag("cycle_" + tag) as CheckBox;
            cycleCB.IsChecked = false;

            CheckBox movingCB = findByTag("moving_" + tag) as CheckBox;
            movingCB.IsChecked = false;
        }

        Control findByTag(string tag)
        {
            foreach (var sp in SensorGrid.Children)
            {
                if (sp is StackPanel)
                {
                    foreach (var c in (sp as StackPanel).Children)
                    {
                        if ((c as Control).Tag.ToString().Equals(tag))
                        {
                            return c as Control;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 触发开关事件的操作
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="toFlag"></param>
        void SettingWindow_FlagChanged(Sensor sensor, byte toFlag)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (System.Windows.Forms.MethodInvoker)delegate()
               {
                   foreach (var sp in SensorGrid.Children)
                   {
                       if (sp is StackPanel)
                       {
                           foreach (var c in (sp as StackPanel).Children)
                           {
                               if (c is Button)
                               {
                                   if ((c as Button).Tag.ToString().Equals("button_" + sensor.Id))
                                   {
                                       if (toFlag == 1)
                                       {
                                           (c as Button).Background = new SolidColorBrush(Colors.Yellow);
                                       }
                                       else
                                       {
                                           (c as Button).Background = new SolidColorBrush(Colors.Gray);
                                       }
                                   }
                               }
                           }
                       }
                   }
               });
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Sensor> sensors = new List<Sensor>();
            string tag = string.Empty;
            foreach (var sp in SensorGrid.Children)
            {
                if (sp is StackPanel)
                {
                    StackPanel spp = sp as StackPanel;
                    tag = spp.Tag.ToString();

                    CheckBox enableCB = null;
                    Button codeBtn = null;
                    TextBox codeTB = null;
                    CheckBox cycleCB = null;
                    CheckBox movingCB = null;
                    CheckBox scramCB = null;

                    foreach (var c in spp.Children)
                    {
                        string cTag = (c as Control).Tag.ToString();

                        if (cTag.Equals("enable_" + tag))
                        {
                            enableCB = c as CheckBox;
                        }
                        else if (cTag.Equals("button_" + tag))
                        {
                            codeBtn = c as Button;
                        }
                        else if (cTag.Equals("code_" + tag))
                        {
                            codeTB = c as TextBox;
                        }
                        else if (cTag.Equals("cycle_" + tag))
                        {
                            cycleCB = c as CheckBox;
                        }
                        else if (cTag.Equals("moving_" + tag))
                        {
                            movingCB = c as CheckBox;
                        }
                        else if (cTag.Equals("scram_" + tag))
                        {
                            scramCB = c as CheckBox;
                        }
                    }

                    if (enableCB.IsChecked.Value)
                    {
                        Sensor sensor = new Sensor()
                        {
                            Code = codeTB.Text,
                            TrigOff = cycleCB.IsChecked.Value,
                            TrigOn = movingCB.IsChecked.Value,
                            IsEmergency = scramCB.IsChecked.Value
                        };
                        sensors.Add(sensor);
                    }
                    else
                    {
                        Sensor sensor = new Sensor()
                        {
                            Code = "X",
                            TrigOff = false,
                            TrigOn = false,
                            IsEmergency = false
                        };
                        sensors.Add(sensor);
                    }
                }
            }

            BaseConfig.SaveSensors(sensors);
            MessageBox.Show("Save Success! Please Restart the Application!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
