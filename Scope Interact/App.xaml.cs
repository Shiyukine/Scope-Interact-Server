using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScopeInteract
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string appName = "Scope Interact.exe";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                string fold = AppDomain.CurrentDomain.BaseDirectory;
                string temp = fold + "Temp";
                if (e.Args.Length > 0 && e.Args[0] == "movefiles")
                {
                    Thread.Sleep(2000);
                    foreach (string f in Directory.GetFiles(temp, "*.*", SearchOption.AllDirectories))
                    {
                        string s = fold + Path.GetFullPath(f).Replace(temp, "");
                        if (File.Exists(s)) File.Delete(s);
                        //
                        if (!Directory.Exists(Path.GetDirectoryName(s))) Directory.CreateDirectory(Path.GetDirectoryName(s));
                        File.Move(f, s);
                    }
                    Directory.Delete(temp, true);
                    Process.Start(fold + appName);
                    App.Current.Shutdown();
                }
                else
                {
                    var ll = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location));
                    var exists = ll.Count() > 1;
                    if (exists)
                    {
                        TcpClient tcp = new TcpClient();
                        tcp.Connect("localhost", 30933);
                        SocketAsyncEventArgs sa = new SocketAsyncEventArgs();
                        sa.Completed += (sendere, ee) =>
                        {
                            if (ee.SocketError != SocketError.Success) MessageBox.Show("An instance of ScopeInteract is already opened !");
                            tcp.Close();
                            tcp.Dispose();
                            Dispatcher.Invoke(() => App.Current.Shutdown());
                        };
                        byte[] data2 = System.Text.Encoding.ASCII.GetBytes("SHOW");
                        sa.SetBuffer(data2, 0, data2.Length);
                        tcp.Client.SendAsync(sa);
                    }
                    else
                    {
                        if (File.Exists(fold + "temp.exe")) File.Delete(fold + "temp.exe");
                        MainWindow l = new MainWindow();
                        l.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ". Stacktrace :\n" + ex.StackTrace);
                MessageBox.Show(ex.InnerException.ToString());
                App.Current.Shutdown();
            }
        }
    }
}
