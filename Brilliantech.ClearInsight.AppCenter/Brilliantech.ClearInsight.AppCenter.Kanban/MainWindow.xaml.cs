using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports; 

namespace Brilliantech.ClearInsight.AppCenter.Kanban
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer currenttime;
        public MainWindow()
        {
            InitializeComponent();

            currenttime = new System.Windows.Threading.DispatcherTimer();
            currenttime.Tick += new EventHandler(ShowCurrentTime);
            currenttime.Interval = new TimeSpan(0, 0, 0, 1);
            currenttime.Start();
        }

        CollectionViewSource viewtable = new CollectionViewSource();
        ObservableCollection<table> tables = new ObservableCollection<table>();

        private SolidColorBrush hightlightBrush = new SolidColorBrush(Colors.Red);
        static Color CellColor = Color.FromArgb(255, 64, 74, 86);
        private SolidColorBrush normarlBrush = new SolidColorBrush(CellColor);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tables.Add(new table()
            {
                Assembly = "T-1",
                Prod_Line = "PL-1",
                Planned = "50",
                Produced = "50",
                Rest = "0",
                Status = "OK"
            });

            tables.Add(new table()
            {
                Assembly = "T-2",
                Prod_Line = "PL-2",
                Planned = "40",
                Produced = "23",
                Rest = "17",
                Status = "Ongoing"
            });

            tables.Add(new table()
            {
                Assembly = "T-3",
                Prod_Line = "PL-1",
                Planned = "50",
                Produced = "0",
                Rest = "50",
                Status = "ALARM"
            });

            tables.Add(new table()
            {
                Assembly = "T-4",
                Prod_Line = "PL-2",
                Planned = "50",
                Produced = "0",
                Rest = "50",
                Status = "Standby"
            });
            viewtable.Source = tables;
            this.gridProducts.DataContext = viewtable;
        }

        private void Grid_Row_Color(object sender, DataGridRowEventArgs e)
        {
            table products = (table)e.Row.DataContext;
            Storyboard colorboard = new Storyboard();
            if (products.Status.Equals("ALARM"))
            {
                e.Row.Background = hightlightBrush;


            }
            else
            {
                e.Row.Background = normarlBrush;
            }
        }
        public void ShowCurrentTime(object sender, EventArgs e)
        {
            //获得星期
            //this.tBlockTime.Text = DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("zh-cn"));

            //获得年月日
            this.CurrentTime.Text = DateTime.Now.ToString("yyyy-MM-dd") + " ";   //yyyy年MM月dd日

            //获得时分秒
            this.CurrentTime.Text += DateTime.Now.ToString("HH:mm:ss");
        }

    }
}
