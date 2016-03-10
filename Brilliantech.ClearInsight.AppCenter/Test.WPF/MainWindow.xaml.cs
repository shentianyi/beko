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
using Brilliantech.ClearInsight.Framework.Lamp;

namespace Test.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // test push devise
            if (e.Key.Equals(Key.X))
            {
                LampUtil.TurnOff();
            }
            KeyLabel.Content = e.Key;
        }

        private void TurnOnBtn_Click(object sender, RoutedEventArgs e)
        {
            LampUtil.TurnOn();
        }


        private void TurnOff_Click(object sender, RoutedEventArgs e)
        {
            LampUtil.TurnOff();
        }
    }
}
