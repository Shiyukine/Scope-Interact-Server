using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace ScopeInteract
{
    public static class TCP
    {
        public static TcpClient tcp;
        public static UdpClient udp;
        public static TcpListener tcps;
        public static NetworkStream net;
        public static Stopwatch ms;
        static InputSimulator inp = new InputSimulator();
        static bool spaceKey = false;
        static int isc = 0;
        public static System.Windows.Forms.Timer ti = new System.Windows.Forms.Timer();

        public async static void enableTCP(MainWindow main)
        {
            try
            {
                tcps = new TcpListener(IPAddress.Any, 30931);
                tcps.Start();
                tcp = await tcps.AcceptTcpClientAsync();
                //UDP
                if (!(bool)main.isusb.IsChecked)
                {
                    udp = new UdpClient(30931);
                    BackgroundWorker bg = new BackgroundWorker();
                    bg.DoWork += (sender, e) =>
                    {
                        IPEndPoint RemoteIpEndPoint;
                        while (true)
                        {
                            RemoteIpEndPoint = null;
                            byte[] buf = udp.Receive(ref RemoteIpEndPoint);
                            string stra = Encoding.ASCII.GetString(buf).Replace(",", ".");
                            text(stra, main);
                        }
                    };
                    bg.RunWorkerAsync();
                }
                //
                AsyncCallback callback = null;
                net = null;
                net = tcp.GetStream();
                tcp.ReceiveBufferSize = 1024;
                byte[] read = new byte[tcp.ReceiveBufferSize];
                ms = Stopwatch.StartNew();
                // REPEAT FLUX
                callback = ar =>
                {
                    try
                    {
                        int bytesRead = net.EndRead(ar);
                        string stra = Encoding.ASCII.GetString(read, 0, bytesRead).Replace(",", ".");
                        text(stra, main);
                        //Repeat
                        if (!main.loaded) return;
                        net.BeginRead(read, 0, tcp.ReceiveBufferSize, callback, null);
                    }
                    catch (Exception ee)
                    {
                        main.Dispatcher.Invoke(() =>
                        {
                            if (main.loaded)
                            {
                                Infos.newErr(ee, "Error when receiving data.");
                                main.loaded = false;
                            }
                            else Infos.addLog("Err : " + ee.Message);
                        });
                    }
                };
                //Begin read
                net.BeginRead(read, 0, tcp.ReceiveBufferSize, callback, null);
                Infos._main.loaded = true;
            }
            catch (Exception ee)
            {
                if (!MainWindow.isStopping)
                {
                    Infos.newErr(ee, "Error loading TCP Server");
                    Infos._main.loaded = false;
                }
            }
        }

        private static void setFor(string stra)
        {
            try
            {
                string[] spli = stra.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string str in spli)
                {
                    fore(str);
                }
            }
            catch (Exception e)
            {
                Infos._main.Dispatcher.Invoke(() =>
                {
                    Infos.newErr(e, "Error when getting information.");
                });
            }
        }

        private static void text(string stra, MainWindow main)
        {
            main.Dispatcher.Invoke(() =>
            {
                //change info mainwindow
                if (main.loaded) Infos.addLog(stra);
                if (stra == "") main.waitState(true);
                if ((bool)main.debug_c.IsChecked && stra != "" && !stra.Contains("Max") && stra.Contains(";") && ms != null) main.debug.Content = "Debug info : " + ms.ElapsedMilliseconds + " ms";
                ms.Restart();
            });
            if (stra != "")
            {
                setFor(stra);
            }
        }

        private static void fore(string str)
        {
            Infos._main.Dispatcher.Invoke(() => isc = Infos._main.moni.SelectedIndex - 1);
            if (str.Contains("Pos"))
            {
                //move cursor
                string[] info = str.Replace("Pos:", "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                CurPos.isHovering = info.Length == 4 && info[3] == "hover";
                CurPos.currentXPos = Infos.strToDouble(info[0]);
                CurPos.currentYPos = Infos.strToDouble(info[1]);
                spaceKey = inp.InputDeviceState.IsKeyDown(VirtualKeyCode.SPACE);
                if (!CurPos.isMouse && !spaceKey)
                {
                    double ax = CurPos.currentXPos / CurPos.maxW * (isc != -1 ? System.Windows.Forms.Screen.AllScreens[isc].Bounds.Width : SystemParameters.VirtualScreenWidth);
                    double ay = CurPos.currentYPos / CurPos.maxH * (isc != -1 ? System.Windows.Forms.Screen.AllScreens[isc].Bounds.Height : SystemParameters.VirtualScreenHeight);
                    //REGARDER SEULEMENT SI LA SOURIS DEPASSE UN ECRAN avec isc = -1
                    /*
                     * if(isc == -1) 
                     * {
                     *      foreach(System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                     *      {    
                     *          if(sc.Bounds.Contains(ax, ay))
                     *          {
                     *              if(ax > sc.Bounds.Width) ax = sc.Bounds.Width
                     *              if(ay > sc.Bounds.Height) ay = sc.Bounds.Height
                     *          }
                     *      }
                     * }*/
                    sbyte statusMask = 33;
                    if (info.Length == 4 && info[3] == "hover") statusMask = 32;
                    CurPos.nx = ax;
                    CurPos.ny = ay;
                    //if (CurPos.autoMC) CurPos.npress = (byte)(CurPos.curPressure * 8);
                    //else statusMask = 32;
                    MainWindow.t.updateDigitizer((byte)statusMask, (ushort)CurPos.nx, (ushort)CurPos.ny, CurPos.npress, isc);
                }
                else
                {
                    if (isc != -1)
                    {
                        double sx = System.Windows.Forms.Screen.AllScreens[isc].Bounds.Width / SystemParameters.PrimaryScreenWidth;
                        double sy = System.Windows.Forms.Screen.AllScreens[isc].Bounds.Height / SystemParameters.PrimaryScreenHeight;
                        double x = System.Windows.Forms.Screen.AllScreens[isc].Bounds.X / SystemParameters.PrimaryScreenWidth * 65536;
                        double y = System.Windows.Forms.Screen.AllScreens[isc].Bounds.Y / SystemParameters.PrimaryScreenHeight * 65536;
                        CurPos.nx = x + (CurPos.currentXPos / CurPos.maxW * 65536 * sx) /*resize if screen[isc] width != screen[0] width*/;
                        CurPos.ny = y + (CurPos.currentYPos / CurPos.maxH * 65536 * sy);
                        //
                        //System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x + Convert.ToInt32(CurPos.nx), y + Convert.ToInt32(CurPos.ny));
                        //MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Move);
                    }
                    else
                    {
                        double sx = SystemParameters.VirtualScreenWidth / SystemParameters.PrimaryScreenWidth;
                        double sy = SystemParameters.VirtualScreenHeight / SystemParameters.PrimaryScreenHeight;
                        CurPos.nx = CurPos.currentXPos / CurPos.maxW * 65536 * sx;
                        CurPos.ny = CurPos.currentYPos / CurPos.maxH * 65536 * sy;
                    }
                    MouseOperations.SetCursorPosition(Convert.ToInt32(CurPos.nx), Convert.ToInt32(CurPos.ny));
                }
                return;
            }
            if(str.Contains("key-2-kdown"))
            {
                CurPos.nx = MouseOperations.GetCursorPosition().X;
                CurPos.ny = MouseOperations.GetCursorPosition().Y;
            }
            if (str.Contains("key-1-kdown"))
            {
                CurPos.isClicking = true;
            }
            if (str.Contains("key-1-kup"))
            {
                CurPos.isClicking = false;
            }
            if (str.Contains("move"))
            {
                string[] info = str.Replace("move:", "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                double velo_x = Infos.strToDouble(info[0]);
                double velo_y = Infos.strToDouble(info[1]);
                double velo_z = Infos.strToDouble(info[2]);
                CurPos.nx += velo_x * 2 * CurPos.curSensivity;
                CurPos.ny += velo_z * 2 * CurPos.curSensivity;
                MainWindow.t.updateDigitizer((byte)(CurPos.isClicking ? 33 : 32), (ushort)CurPos.nx, (ushort)CurPos.ny, CurPos.npress, isc);
            }
            if (str.Contains("mouse_"))
            {
                ushort x = (ushort)CurPos.nx;
                ushort y = (ushort)CurPos.ny;
                //mouse down
                if (str.Contains("click"))
                {
                    MouseOperations.MouseEventFlags f = MouseOperations.MouseEventFlags.LeftDown;
                    byte stat = 33;
                    if (str.Contains("rclick"))
                    {
                        f = MouseOperations.MouseEventFlags.RightDown;
                        stat = 35;
                    }
                    if (CurPos.isMouse) MouseOperations.MouseEvent(f);
                    else
                    {
                        if (!inp.InputDeviceState.IsKeyDown(VirtualKeyCode.SPACE)) MainWindow.t.updateDigitizer(stat, x, y, CurPos.npress, isc);
                        else MouseOperations.MouseEvent(f);
                    }
                    return;
                }
                //mouse up
                if(str.Contains("up"))
                {
                    MouseOperations.MouseEventFlags f = MouseOperations.MouseEventFlags.LeftUp;
                    if(str.Contains("rup")) f = MouseOperations.MouseEventFlags.RightUp;
                    if (CurPos.isMouse) MouseOperations.MouseEvent(f);
                    else
                    {
                        //fix issue cannot deplace drawing
                        if (!spaceKey) MainWindow.t.updateDigitizer(32, x, y, CurPos.npress, isc);
                        else
                        {
                            MouseOperations.MouseEvent(f);
                            spaceKey = false;
                        }
                    }
                    CurPos.npress = 0;
                    SmootherCursorv0.releasePoints();
                    return;
                }
                //mouse wheel
                if (str.Contains("wheel"))
                {
                    int delta = int.Parse(str.Split('_')[2]);
                    MouseOperations.MouseWheel(delta);
                    return;
                }
            }
            if (str.Contains("Max"))
            {
                //get max screen grid layout phone
                string[] info = str.Replace("Max:", "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                CurPos.maxW = Infos.strToDouble(info[0]);
                CurPos.maxH = Infos.strToDouble(info[1]);
                CurPos.autoMC = bool.Parse(info[4]);
                CurPos.isMouse = info[2] == "mouse";
                Infos._main.Dispatcher.Invoke(() =>
                {
                    if (int.Parse(info[3]) < Update.minVerCode)
                    {
                        Infos._main.loaded = false;
                        Infos.newErr(null, "Version of this client is out-dated. Please update client.");
                    }
                    if (Infos._main.loaded && !CurPos.isMouse && !MainWindow.t.IsDriverConnected)
                    {
                        //Infos._main.loaded = false;
                        Infos.newErr(null, "You must have Hawku's driver to continue.\nBut you can test without it anyway.");
                    }
                    if(!CurPos.isMouse) ti.Start();
                });
                return;
            }
            if (!CurPos.isMouse && str.Contains("key-"))
            {
                //gerer keys
                Infos._main.Dispatcher.Invoke(() =>
                {
                    string[] id = str.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!MainWindow.isSetKey)
                    {
                        //down
                        if (str.Contains("-kdown"))
                        {
                            string set = Infos.sf.getStringSetting("key_" + id[1]);
                            //if have modifier keys
                            if (set.Contains(";"))
                            {
                                List<VirtualKeyCode> modi = new List<VirtualKeyCode>();
                                List<VirtualKeyCode> nmo = new List<VirtualKeyCode>();
                                foreach (string key in Infos.sf.getStringListSetting("key_" + id[1]))
                                {
                                    VirtualKeyCode vc = KeyManager.getKeyByString(key);
                                    if (KeyManager.isKeyModifier(key)) modi.Add(vc);
                                    else nmo.Add(vc);
                                }
                                inp.Keyboard.ModifiedKeyStroke(modi, nmo);
                            }
                            else
                            {
                                //... or not
                                foreach (string key in Infos.sf.getStringListSetting("key_" + id[1]))
                                {
                                    inp.Keyboard.KeyDown(KeyManager.getKeyByString(key));
                                }
                            }
                        }
                        //key up
                        if (str.Contains("-kup"))
                        {
                            foreach (string key in Infos.sf.getStringListSetting("key_" + id[1]))
                            {
                                inp.Keyboard.KeyUp(KeyManager.getKeyByString(key));
                            }
                        }
                    }
                    else Infos._main.setKeyId = int.Parse(id[1]);
                });
                return;
            }
        }
    }
}
