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
using Brilliantech.ClearInsight.Framework;
using Brilliantech.ClearInsight.Framework.Model;
using ScmWcfService.Model.Message;
using Brilliantech.ClearInsight.Framework.Lamp;
using System.Windows.Threading;
using System.Windows.Forms;
using Brilliantech.ClearInsight.Framework.Config; 

namespace Brilliantech.ClearInsight.AppCenter.Kanban
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer currenttime;


        private System.Timers.Timer timer;


        // 0正常, 1警报,2系统错误 
        private int status = 0;


        List<int> ids = new List<int>();
        List<string> productLines = new List<string>();

        int currentProductIndex = 0;
        bool locked=false;

        public MainWindow()
        {
            InitializeComponent();
        }

        CollectionViewSource viewtable = new CollectionViewSource();
        ObservableCollection<ProductionPlan> tables = new ObservableCollection<ProductionPlan>();

        private SolidColorBrush hightlightBrush = new SolidColorBrush(Colors.Red);
        static Color CellColor = Color.FromArgb(255, 64, 74, 86);
        private SolidColorBrush okBrush = new SolidColorBrush(Colors.Green);

        private SolidColorBrush normalBrush = new SolidColorBrush(CellColor);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            productLines = Properties.Settings.Default.ProductLines.Split(',').ToList();

            initPage();

            currenttime = new System.Windows.Threading.DispatcherTimer();
            currenttime.Tick += new EventHandler(ShowCurrentTime);
            currenttime.Interval = new TimeSpan(0, 0, 0, 1);
            currenttime.Start();



            timer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.timer)).BeginInit();
            timer.Enabled = false;
            timer.Interval =BaseConfig.KanbanTimerInterval;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

            ((System.ComponentModel.ISupportInitialize)(this.timer)).EndInit();
            timer.Start();


            LampUtil.TurnNormal();

            if (Properties.Settings.Default.OnTest)
            {

                test_btn_Checked(null, null);
            }
            else {
                test_btn_Unchecked(null, null);
            }
        }
        private void initPage() {
            if (!locked)
            {
                // to do change index
                tables = null;
                tables = getPlan();
                if (tables.Count > 0)
                {
                    productNameLabel.Content = productLines[currentProductIndex];
                    viewtable.Source = null;
                    viewtable.Source = tables;

                    this.gridProducts.DataContext = null;

                    this.gridProducts.DataContext = viewtable;
                }
            }
        }
        private ObservableCollection<ProductionPlan> getPlan() {
            ObservableCollection<ProductionPlan> bplans = new ObservableCollection<ProductionPlan>();

            try
            {
                AppService app = new AppService();
                ResponseMessage<List<ProductionPlan>> plans = app.GetPlans(productLines[currentProductIndex], DateTime.Today.ToShortDateString());

                if (plans.data.Count > 0)
                {
                    foreach (var p in plans.data)
                    {
                        if (p.Status != "生产完")
                        {
                            bplans.Add(p);
                        }
                    }
                }
            }
            catch (Exception ex) {
            
            }
            return bplans;
        }
        private void Grid_Row_Color(object sender, DataGridRowEventArgs e)
        {
            ProductionPlan products = (ProductionPlan)e.Row.DataContext;
            Storyboard colorboard = new Storyboard();
            if (products.Status.Equals("警报"))
            {
                e.Row.Background = hightlightBrush;

                // this.Dispatcher.Invoke(DispatcherPriority.Normal, (MethodInvoker)delegate()
                //{
                locked = true;
                
                LampUtil.TurnOn();
                
                ids.Add(products.Id);
                //});
            }
            else if (products.Status.Equals("生产完")) {
                e.Row.Background = okBrush;
            }
            else
            {
              e.Row.Background = normalBrush;
            }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // test push devise
            if (e.Key.Equals(Key.X))
            {
                //this.Dispatcher.Invoke(DispatcherPriority.Normal, (MethodInvoker)delegate()
                //{
                    LampUtil.TurnNormal();
                    locked = false;
                //});
                    AppService app = new AppService();
                    if (ids.Count > 0)
                    {
                        app.ConfirmPlans(ids);
                        initPage();
                    }           
            }
            //  KeyLabel.Content = e.Key;
        }


        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!locked)
            {
                currentProductIndex += 1;
            }
            else
            {

            }
            currentProductIndex = (currentProductIndex % productLines.Count);
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (MethodInvoker)delegate()
           {
               initPage();
           });
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

        private void test_btn_Checked(object sender, RoutedEventArgs e)
        {
            try {
                show_test_ch.Visibility = Visibility.Visible;
                show_test_en.Visibility = Visibility.Visible;
            }catch(Exception ex)
            {
            }
        }

        private void test_btn_Unchecked(object sender, RoutedEventArgs e)
        {
            show_test_ch.Visibility = Visibility.Hidden;
            show_test_en.Visibility = Visibility.Hidden;
        }
    }
}
