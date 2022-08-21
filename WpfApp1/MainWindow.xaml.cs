﻿using System;
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

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        Recognizer recognizer = null;

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        public MainWindow()
        {

            InitializeComponent();
            rosh_state.Visibility=Visibility.Hidden;
            Screen rightmost = Screen.PrimaryScreen;
            Left = rightmost.WorkingArea.Right - Width;
            Top = 0;
            Topmost = true;
            Handle = new WindowInteropHelper(this).Handle;
            var notepad = Process.GetProcessesByName("dota2").FirstOrDefault();
            Console.WriteLine(notepad != null);
            if (notepad != null)
            {
                var owner = notepad.MainWindowHandle;
                var owned = Handle;
                _ = SetWindowLong(owned, -8 /*GWL_HWNDPARENT*/, owner);
            }
            recognizer = new Recognizer(Properties.Settings.Default.path_to_tesseract);
            Thread t_recognizer = new Thread(ThreadStart);
            t_recognizer.Start();
            //while (true)
            //{
            //    GetScreenshot();
            //    Thread.Sleep(2000);
            //}
            //KeyboardListener KListener = new KeyboardListener();

            //KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);

        }
        private void StartCountDown()
        {

        }
        private void ThreadStart()
        {
            while (true)
            {
                Rectangle rect = new Rectangle(0, 360, 400, 410);
                Bitmap bm = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                //Bitmap bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics g = Graphics.FromImage(bm);
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bm.Size, CopyPixelOperation.SourceCopy);
                bm.Save("test.jpg", ImageFormat.Jpeg);
                bool results = recognizer.StartRecognition(bm);
                if (results)
                {
                    Console.WriteLine("Roshan dead");
                    //if (!RoshIsDead)
                    //{
                    this.Dispatcher.Invoke(() =>
                    {
                        if (roshanTimer.TotalMinutes==8 || roshanTimer.TotalMinutes < 1)
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
                    Thread.Sleep(10000);
                }
                else
                {
                    Console.WriteLine("Didnt find roshan");
                }
                Thread.Sleep(2000);
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
                AegisTimer = new TimeSpan(0, 0, 0);
                aegis_timer.Content = "Aegis time:";
            }

        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            var notepad = Process.GetProcessesByName("dota2").FirstOrDefault();
            Console.WriteLine(notepad != null);
            if (notepad != null)
            {
                var owner = notepad.MainWindowHandle;
                var owned = this.Handle;
                var i = SetWindowLong(owned, -8 /*GWL_HWNDPARENT*/, owner);
            }
        }

        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //var window = (Window)sender;
            this.Topmost = true;
        }
    }
}