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

namespace Test.WPF
{
    /// <summary>
    /// Q2GenWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Q2GenWindow : Window
    {
        public Q2GenWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string s = string.Empty;
            string index = indexTB.Text;
            Dictionary<int, int> dic = new Dictionary<int, int>();
            foreach(var i in indexTB.Text.Split(',')){
                string[] kv = i.Split('-');
                if (kv[1] != "X")
                {
                    dic.Add(int.Parse(kv[1]), int.Parse(kv[0]));
                }
            }
            for (int i = 0; i < 992; i++) {
                if (!dic.Keys.Contains(i))
                {
                    s += (i + "#X,");
                }
                else {
                    if (dic[i] < 10)
                    {
                        s += (i + "#Q-C0" + dic[i] + ",");
                    }
                    else {
                        s += (i + "#Q-C" + dic[i] + ",");
                    }
                }
            }

            resultTB.Text = s;
        }
    }
}
