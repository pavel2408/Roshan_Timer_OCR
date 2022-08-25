using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;
using System.Threading;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimeSpan roshanTimer = new TimeSpan(0, 8, 0);
        private TimeSpan AegisTimer = new TimeSpan(0, 5, 0);
        private TimeSpan additionalTimer = new TimeSpan(0, 3, 0);
        private TimeSpan second = new TimeSpan(0, 0, 1);
        private bool RoshIsDead = false;
        private IntPtr Handle;
        private DispatcherTimer timer;
        Rectangle killfeed;
        Bitmap bm;
        Graphics g;
        bool results;
        bool in_turbo;
        int killfeed_fadeaway_timeout = 10000;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        Recognizer recognizer = null;
        Thread t_recognizer = null;
        bool destroy_thread;

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        delegate void CountdownCallback();

        public MainWindow()
        {

            InitializeComponent();
            rosh_state.Visibility=Visibility.Hidden;
            turbo_flag.Visibility = Visibility.Hidden;
            Screen rightmost = Screen.PrimaryScreen;
            Left = rightmost.WorkingArea.Right - Width;
            Top = 0;
            Topmost = true;

            //Hook to the dota process
            Handle = new WindowInteropHelper(this).Handle;
            var dota_process = Process.GetProcessesByName("dota2").FirstOrDefault();
            Console.WriteLine(dota_process != null);
            if (dota_process != null)
            {
                var owner = dota_process.MainWindowHandle;
                var owned = Handle;
                _ = SetWindowLong(owned, -8 /*GWL_HWNDPARENT*/, owner);
            }

            //Parameters for recognition
            killfeed = new Rectangle(0, 360, 400, 410);

            in_turbo = false;
            destroy_thread = false;
            recognizer = new Recognizer(Properties.Settings.Default.path_to_tesseract);
            t_recognizer = new Thread(ThreadStart);
            t_recognizer.Start();

        }
        private void StartCountDown()
        {
            if (roshanTimer.TotalMinutes == 8 || roshanTimer.TotalMinutes < 1)
            {
                RoshIsDead = true;
                roshanTimer = new TimeSpan(0, 8, 0);
                AegisTimer = new TimeSpan(0, 5, 0);
                additionalTimer = new TimeSpan(0, 3, 0);
                if (timer != null && timer.IsEnabled)
                    timer.Stop();
                rosh_state.Content = "Roshan is dead";
                rosh_state.Foreground = System.Windows.Media.Brushes.Red;
                timer = new DispatcherTimer();
                timer.Tick += new EventHandler(timer_Tick);
                timer.Interval = second;
                timer.Start();
                rosh_state.Visibility = Visibility.Visible;
            }
        }

        private void ThreadStart()
        {
            CountdownCallback timer_cb;
            timer_cb = StartCountDown;
            while (!destroy_thread)
            {
                //Screenshot and crop out killfeed
                bm = new Bitmap(killfeed.Width, killfeed.Height, PixelFormat.Format32bppArgb);
                g = Graphics.FromImage(bm);
                g.CopyFromScreen(killfeed.Left, killfeed.Top, 0, 0, bm.Size, CopyPixelOperation.SourceCopy);
                //bm.Save("test.jpg", ImageFormat.Jpeg);
                results = recognizer.StartRecognition(bm);
                if (results)
                {
                    Console.WriteLine("Roshan dead");
                    //if (!RoshIsDead)
                    //{
                    this.Dispatcher.Invoke(() =>
                    {
                        timer_cb();
                    });
                    //}
                    /*else if (RoshIsDead)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            RoshIsDead = true;
                            timer.Stop();
                            roshanTimer = new TimeSpan(0, 8, 0);
                            AegisTimer = new TimeSpan(0, 5, 0);
                            additionalTimer = new TimeSpan(0, 3, 0);
                            rosh_state.Visibility = Visibility.Hidden;
                        });
                    }*/
                    Thread.Sleep(killfeed_fadeaway_timeout);
                }
                else
                {
                    Console.WriteLine("Didnt find roshan");
                }
                Thread.Sleep(500);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("ticking down..."+ roshanTimer.TotalSeconds.ToString()+"," + (roshanTimer.TotalSeconds != 0).ToString());
            if (AegisTimer.TotalSeconds!=0)
            {
                AegisTimer=AegisTimer.Subtract(second);
                aegis_timer.Content = "Aegis time: " + AegisTimer.Minutes.ToString() + ":" + AegisTimer.Seconds.ToString();
            }
            if (roshanTimer.TotalSeconds != 0)
            {
                roshanTimer=roshanTimer.Subtract(second);
                rosh_timer.Content = "Roshan time: " + roshanTimer.Minutes.ToString() + ":" + roshanTimer.Seconds.ToString();
            }
            else if (roshanTimer.TotalSeconds==0 && additionalTimer.TotalSeconds!=0)
            {
                additionalTimer=additionalTimer.Subtract(second);
                add_timer.Content = "Additional time: " + additionalTimer.Minutes.ToString() + ":" + additionalTimer.Seconds.ToString();
            }
            else if (roshanTimer.TotalSeconds==0 && additionalTimer.TotalSeconds==0)
            {
                timer.Stop();
                aegis_timer.Content = "Aegis time: ";
                rosh_timer.Content = "Roshan time: ";
                add_timer.Content = "Additional time: ";
                rosh_state.Visibility = Visibility.Hidden;
            }

        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.F4 && !RoshIsDead)
            {
                RoshIsDead = true;
                rosh_state.Content = "Roshan is dead";
                rosh_state.Foreground = System.Windows.Media.Brushes.DarkRed;
                timer =  new DispatcherTimer();
                timer.Tick += new EventHandler(timer_Tick);
                timer.Interval = second;
                timer.Start();
                rosh_state.Visibility = Visibility.Visible;
            }
            else if (e.Key==Key.F4 && RoshIsDead)
            {
                RoshIsDead = false;
                timer.Stop();
                roshanTimer = new TimeSpan(0, 8, 0);
                AegisTimer = new TimeSpan(0, 5, 0);
                additionalTimer = new TimeSpan(0, 3, 0);
                rosh_state.Visibility = Visibility.Hidden;
            }
            if(e.Key==Key.F3)
            {
                RoshIsDead = false;
                timer.Stop();
                roshanTimer = new TimeSpan(0, 8, 0);
                AegisTimer = new TimeSpan(0, 5, 0);
                additionalTimer = new TimeSpan(0, 3, 0);
                rosh_state.Visibility = Visibility.Hidden;
            }
            if(e.Key==Key.F2 && !in_turbo)
            {
                RoshIsDead = false;
                turbo_flag.Visibility = Visibility.Visible;
                in_turbo = true;
                timer.Stop();
                roshanTimer = new TimeSpan(0, 5, 0);
                AegisTimer = new TimeSpan(0, 5, 0);
                additionalTimer = new TimeSpan(0, 0, 0);
                rosh_state.Visibility = Visibility.Hidden;
            }
            else if (e.Key == Key.F2 && in_turbo)
            {
                RoshIsDead = false;
                in_turbo = false;
                RoshIsDead = false;
                timer.Stop();
                roshanTimer = new TimeSpan(0, 8, 0);
                AegisTimer = new TimeSpan(0, 5, 0);
                additionalTimer = new TimeSpan(0, 3, 0);
                rosh_state.Visibility = Visibility.Hidden;
            }

        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            var dota_process = Process.GetProcessesByName("dota2").FirstOrDefault();
            Console.WriteLine(dota_process != null);
            if (dota_process != null)
            {
                var owner = dota_process.MainWindowHandle;
                var owned = this.Handle;
                var i = SetWindowLong(owned, -8 /*GWL_HWNDPARENT*/, owner);
            }
        }

        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //var window = (Window)sender;
            this.Topmost = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            destroy_thread = true;
            t_recognizer.Join();
        }
    }
}
