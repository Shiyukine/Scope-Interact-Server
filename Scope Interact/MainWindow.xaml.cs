using AnimatedScrollViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ScopeInteract.Settings;

namespace ScopeInteract
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static HIDWrapper t = new HIDWrapper();
        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        static bool bloaded = false;
        bool windowLoaded = false;
        public static bool isStopping = false;
        public bool loaded
        {
            get
            {
                return bloaded;
            }
            set
            {
                bloaded = value;
                RemoveClickEvent(serv_load);
                if(CurPos.cur != null) MouseOperations.ChangeCursor(CurPos.cur);
                if (value)
                {
                    serv_stats.Content = "ON";
                    serv_stats.Foreground = Brushes.Lime;
                    serv_load.Content = "Stop server";
                    serv_load.Click += serv_load_Click2;
                    Infos.addLog("Connected.");
                }
                else
                {
                    a();
                    async void a()
                    {
                        serv_stats.Content = "OFF";
                        serv_stats.Foreground = Brushes.Red;
                        serv_load.Content = "Load server";
                        serv_load.Click += serv_load_Click;
                        try
                        {
                            isStopping = true;
                            TCP.ti.Stop();
                            TCP.tcps.Stop();
                            TCP.tcp.Close();
                            TCP.net.Dispose();
                            TCP.udp.Close();
                            TCP.udp.Dispose();
                            if (!(bool)isusb.IsChecked && TCP.udp != null) TCP.udp.Close();
                            else await ADBProcess.newCommand("adb reverse --remove-all");
                            isStopping = false;
                        }
                        catch { }
                        Infos.addLog("Disconnected.");
                    }
                }
            }
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "Stacktrace :\n" + ex.StackTrace);
                App.Current.Shutdown();
            }
        }

        private void main_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                welc.Content = "v" + Update.getVersion() + "." + Update.getRevision() + "\nBy Shiyukine - Aketsuky";
                Infos.Init(this);
                Set.syncSettings(this);
                moni.DropDownClosed += (sendere, ee) =>
                {
                    Infos.sf.setSetting("Monitor", moni.SelectedIndex, null);
                };
                int i = 0;
                foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
                {
                    i++;
                    moni.Items.Add(new ComboBoxItem() { Content = i.ToString(), Foreground = Brushes.Black });
                }
                Infos.addLog("Driver connected : " + t.IsDriverConnected);
                if (!t.IsDriverConnected)
                {
                    Infos.newErr(null, "Driver not found ! Please install it to use graphic tablet mode.");
                    Infos.addLog(t.ErrorLogs);
                }
                ni.Icon = new System.Drawing.Icon("Icon.ico");
                ni.Click += (sendere, args) =>
                {
                    Show();
                };
                ni.Text = "Scope Interact";
                System.Windows.Forms.MenuItem mi = new System.Windows.Forms.MenuItem() { Text = "Exit" };
                mi.Click += (s, ev) =>
                {
                    isclos = true;
                    Close();
                };
                System.Windows.Forms.ContextMenu cm = new System.Windows.Forms.ContextMenu();
                cm.MenuItems.Add(mi);
                ni.ContextMenu = cm;
                ni.Visible = true;
                Update.Init(this);
                Update.searchUpdate();
                //
                foreach (ScrollViewer sv in Infos.FindVisualChildren<ScrollViewer>(g_main))
                {
                    //sv.CanContentScroll = false;
                    int ii = 0;
                    sv.PreviewMouseWheel += (sendere, ee) =>
                    {
                        sv.InvalidateScrollInfo();
                        e.Handled = true;
                        sv.ScrollToVerticalOffset(sv.VerticalOffset);
                        ii++;
                        DoubleAnimation verticalAnimation = new DoubleAnimation();

                        verticalAnimation.From = sv.VerticalOffset;
                        int delta = ee.Delta;
                        //if (delta < 0) delta = ee.Delta * (-1);
                        int offset = 40 * ee.Delta / 120 * ii;
                        verticalAnimation.To = sv.VerticalOffset - offset;
                        verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(150));

                        Storyboard storyboard = new Storyboard();

                        storyboard.Children.Add(verticalAnimation);
                        Storyboard.SetTarget(verticalAnimation, sv);
                        Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollAnimationBehavior.VerticalOffsetProperty));
                        storyboard.Completed += (ss, eee) =>
                        {
                            ii = 0;
                        };
                        storyboard.Begin();
                    };
                }
                TCP.ti.Interval = 20;
                TCP.ti.Tick += (s, ee) =>
                {
                    if (!CurPos.isMouse && CurPos.npress != 0 && TCP.ms.ElapsedMilliseconds > 20 && !CurPos.isHovering && loaded)
                    {
                        t.updateDigitizer(33, (ushort)CurPos.nx, (ushort)CurPos.ny, CurPos.npress, moni.SelectedIndex - 1);
                    }
                };
                //
                Infos.addLog("TT Loaded.");
                connect();
                //
                connectLocalTCP();
                windowLoaded = true;
                if(!Infos.sf.getBoolSetting("Logs")) logs.Text = "";
                string localIP;
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
                ip_text.Content = "My IP: " + localIP;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "Stacktrace :\n" + ex.StackTrace);
                App.Current.Shutdown();
            }
        }

        private void logs_c_Click(object sender, RoutedEventArgs e)
        {
            logs.Text = "";
        }

        private void serv_load_Click(object sender, RoutedEventArgs e)
        {
            connect();
        }

        private void serv_load_Click2(object sender, RoutedEventArgs e)
        {
            Infos.newErr(null, "Stoping server...");
            ADBProcess.closeAllCon();
        }

        private async void connect()
        {
            Infos.addLog("Starting server...");
            //
            if ((bool)isusb.IsChecked)
            {
                Infos.newErr(null, "Searching devices...");
                if ((await ADBProcess.newCommand("adb devices")).Contains("\tdevice"))
                {
                    Infos.addLog("Device detected.");
                    Infos.addLog("Loading server...");
                    //connect phone to port 30921
                    await ADBProcess.newCommand("adb reverse tcp:30931 tcp:30931");
                    //change button
                    waitState(false);
                    Infos.newErr(null, "Please connect the client to the server.");
                    //con tcp
                    TCP.enableTCP(this);
                }
                else
                {
                    Infos.newErr(null, "Device not detected.");
                    Infos.addLog("Please check that USB debugging is enabled on your phone and that you have accepted the request from your PC.");
                }
            }
            else
            {
                Infos.addLog("Loading server...");
                //change button
                waitState(false);
                Infos.newErr(null, "Please connect the client to the server.");
                //con tcp
                TCP.enableTCP(this);
            }
        }

        private async void connectLocalTCP()
        {
            TcpClient tcp = null;
            TcpListener tcps = null;
            int port = 30933;
            try
            {
                tcps = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                tcps.Start();
                tcp = await tcps.AcceptTcpClientAsync();
            }
            catch
            {
                tcp = new TcpClient();
                await tcp.ConnectAsync("localhost", port);
            }
            NetworkStream ns = tcp.GetStream();
            tcp.ReceiveBufferSize = 1024;
            byte[] read = new byte[tcp.ReceiveBufferSize];
            //
            AsyncCallback asy = null;
            asy = (ar) =>
            {
                try
                {
                    int bytesRead = ns.EndRead(ar);
                    string stra = Encoding.ASCII.GetString(read, 0, bytesRead).Replace(",", ".");
                    Dispatcher.Invoke(() => Infos.addLog("Request show"));
                    if (stra == "SHOW")
                    {
                        Dispatcher.Invoke(() => Infos.addLog("Show"));
                        Dispatcher.Invoke(() =>
                        {
                            Show();
                            Activate();
                        });
                    }
                    if (stra == "")
                    {
                        tcps.Stop();
                        tcp.Close();
                        connectLocalTCP();
                        return;
                    }
                    ns.BeginRead(read, 0, tcp.ReceiveBufferSize, asy, null);
                }
                catch (Exception ee)
                {
                    Dispatcher.Invoke(() => Infos.newErr(ee, "Intern error"));
                }
            };
            ns.BeginRead(read, 0, tcp.ReceiveBufferSize, asy, null);
        }

        public void waitState(bool reload)
        {
            if (reload) loaded = false;
            if (!reload)
            {
                RemoveClickEvent(serv_load);
                serv_load.Click += serv_load_Click2;
                serv_stats.Content = "ON";
                serv_load.Content = "Stop server";
                serv_stats.Foreground = Brushes.Orange;
            }
            if (reload) connect();
        }

        private void main_Closed(object sender, EventArgs e)
        {
            ADBProcess.closeAllCon();
            t.Dispose();
        }

        public static void RemoveClickEvent(Button b)
        {
            var routedEventHandlers = GetRoutedEventHandlers(b, ButtonBase.ClickEvent);
            foreach (var routedEventHandler in routedEventHandlers)
                b.Click -= (RoutedEventHandler)routedEventHandler.Handler;
        }

        private static RoutedEventHandlerInfo[] GetRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
                "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
                eventHandlersStore, new object[] { routedEvent });

            return routedEventHandlers;
        }

        private void logs_e_Click(object sender, RoutedEventArgs e)
        {
            Infos.sf.setSetting("Logs", (bool)logs_e.IsChecked, null);
        }

        public static bool isSetKey = false;
        private int setKeyIda = -1;
        public int setKeyId
        {
            get { return setKeyIda; }
            set
            {
                if (value != -1)
                {
                    key_id.Content = "Key ID : " + value;
                    key_ch.Text = Infos.sf.getStringSetting("key_" + value);
                    key_gch.Visibility = Visibility.Visible;
                }
                else
                {
                    key_gch.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void keys_Click(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                //A MODIF
                if (!CurPos.isMouse /*true*/)
                {
                    key_g.Visibility = Visibility.Visible;
                    Storyboard sb = new Storyboard();
                    sb.Children.Add(Infos.addDoubleAnimation(key_g, TimeSpan.FromMilliseconds(200), 0, 1, new PropertyPath(UIElement.OpacityProperty)));
                    sb.Begin();
                    isSetKey = true;
                }
                else Infos.newErr(null, "You must use the pen to use this feature.");
            }
            else Infos.newErr(null, "The server must be enabled to change keys.");
        }

        private void key_help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://raw.githubusercontent.com/GregsStack/InputSimulatorStandard/master/src/GregsStack.InputSimulatorStandard/Native/VirtualKeyCode.cs");
        }

        private void key_back_Click(object sender, RoutedEventArgs e)
        {
            closeKeyWindow();
        }

        private void key_save_Click(object sender, RoutedEventArgs e)
        {
            string[] str = ((string)key_id.Content).Split(new string[] { " : " }, StringSplitOptions.None);
            Infos.sf.setSetting("key_" + str[1], key_ch.Text, null);
            closeKeyWindow();
        }

        private void closeKeyWindow()
        {
            isSetKey = false;
            setKeyId = -1;
            key_gch.Visibility = Visibility.Collapsed;
            Storyboard sb = new Storyboard();
            sb.Children.Add(Infos.addDoubleAnimation(key_g, TimeSpan.FromMilliseconds(200), 1, 0, new PropertyPath(UIElement.OpacityProperty)));
            sb.Completed += (sender, e) =>
            {
                key_g.Visibility = Visibility.Collapsed;
            };
            sb.Begin();
        }

        public bool isclos = false;

        private void main_Closing(object sender, CancelEventArgs e)
        {
            if (!isclos)
            {
                Hide();
                ni.ShowBalloonTip(10, "Scope Interact is minimized", "You can re-open this app on your taskbar.", System.Windows.Forms.ToolTipIcon.None);
                e.Cancel = true;
            }
            else
            {
                ni.Visible = false;
                e.Cancel = false;
            }
        }

        private void debug_c_Click(object sender, RoutedEventArgs e)
        {
            Infos.sf.setSetting("Debug", (bool)debug_c.IsChecked, null);
        }

        private void upd_Click(object sender, RoutedEventArgs e)
        {
            updpb.Value = 0;
            updpb.Maximum = 100;
            updg.Visibility = Visibility.Visible;
            Update.searchUpdate();
        }

        private void isusb_Checked(object sender, RoutedEventArgs e)
        {
            if(Infos.sf != null) Infos.sf.setSetting("UseUSB", (bool)isusb.IsChecked, null);
            if(windowLoaded) waitState(true);
        }

        private void isusb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Infos.sf != null) Infos.sf.setSetting("UseUSB", (bool)isusb.IsChecked, null);
            if(windowLoaded) waitState(true);
        }
    }
}
