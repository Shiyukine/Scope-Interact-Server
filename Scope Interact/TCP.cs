using Microsoft.Win32;
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

        static void changeIsMovingX(double acc_x)
        {
            double limit = 0.3;
            if (CurPos.isMovingX == 1 && acc_x <= 0)
            {
                CurPos.isMovingX = -2;
                return;
            }
            if (CurPos.isMovingX == -1 && acc_x >= 0)
            {
                CurPos.isMovingX = 2;
                return;
            }
            if (CurPos.isMovingX == 0)
            {
                if (acc_x > limit) CurPos.isMovingX = 1;
                if (acc_x < -limit) CurPos.isMovingX = -1;
                return;
            }
            if (CurPos.isMovingX == 2 && acc_x < limit && acc_x > -limit)
            {
                CurPos.isMovingX = 0;
                return;
            }
            if (CurPos.isMovingX == -2 && acc_x < limit && acc_x > -limit)
            {
                CurPos.isMovingX = 0;
                return;
            }
        }

        static void changeIsMovingZ(double acc_z)
        {
            double limit = 0.3;
            if (CurPos.isMovingZ == 1 && acc_z <= 0)
            {
                CurPos.isMovingZ = -2;
                return;
            }
            if (CurPos.isMovingZ == -1 && acc_z >= 0)
            {
                CurPos.isMovingZ = 2;
                return;
            }
            if (CurPos.isMovingZ == 0)
            {
                if (acc_z > limit) CurPos.isMovingZ = 1;
                if (acc_z < -limit) CurPos.isMovingZ = -1;
                return;
            }
            if (CurPos.isMovingZ == 2 && acc_z < limit && acc_z > -limit)
            {
                CurPos.isMovingZ = 0;
                return;
            }
            if (CurPos.isMovingZ == -2 && acc_z < limit && acc_z > -limit)
            {
                CurPos.isMovingZ = 0;
                return;
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
                double top = 0;
                double left = 0;
                foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
                {
                    if (s.Bounds.Y < top && s.Bounds.Y < 0) top = s.Bounds.Y * -1;
                    if (s.Bounds.X < left && s.Bounds.X < 0) left = s.Bounds.X * -1;
                }
                //
                int monitor = Screen.AllScreens.ToList().IndexOf(Screen.FromPoint(Cursor.Position));
                double ux = System.Windows.Forms.Screen.AllScreens[monitor].Bounds.X + left;
                double uy = System.Windows.Forms.Screen.AllScreens[monitor].Bounds.Y + top;
                Debug.WriteLine("aa " + CurPos.nx + " " + CurPos.ny);
                CurPos.nx = Cursor.Position.X;
                CurPos.ny = Cursor.Position.Y;
                CurPos.linearVeloX = 0;
                CurPos.linearVeloY = 0;
                CurPos.linearVeloZ = 0;
                CurPos.isMovingX = 0;
                CurPos.isMovingY = 0;
                CurPos.isMovingZ = 0;
                Debug.WriteLine("ab " + CurPos.nx + " " + CurPos.ny);
                CurPos.cur = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors\", "Arrow", "");
                Debug.WriteLine(CurPos.cur);
                MouseOperations.ChangeCursor(AppDomain.CurrentDomain.BaseDirectory + @"Green.cur");
                return;
            }
            if(str.Contains("key-2-kup"))
            {
                //mandatory
                Cursor.Position = new System.Drawing.Point((int)CurPos.nx, (int)CurPos.ny);
                MouseOperations.ChangeCursor(CurPos.cur);
                return;
            }
            if (str.Contains("key-1-kdown"))
            {
                CurPos.isClicking = true;
                MainWindow.t.updateDigitizer((byte)33, (ushort)CurPos.nx, (ushort)CurPos.ny, 8, isc);
                return;
            }
            if (str.Contains("sensivity"))
            {
                CurPos.curSensivity = Infos.strToDouble(str.Replace("sensivity:", ""));
                return;
            }
            if (str.Contains("key-1-kup"))
            {
                CurPos.isClicking = false;
                return;
            }
            if (str.Contains("move"))
            {
                string[] info = str.Replace("move:", "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                double velo_x = Infos.strToDouble(info[0]);
                double velo_y = Infos.strToDouble(info[1]);
                double velo_z = Infos.strToDouble(info[2]);
                double multiplier = 2;
                double nx = CurPos.nx - (velo_z * multiplier * CurPos.curSensivity);
                double ny = CurPos.ny - (velo_x * multiplier * CurPos.curSensivity);
                double maxWidth = isc != -1 ? System.Windows.Forms.Screen.AllScreens[isc].Bounds.Width : SystemParameters.VirtualScreenWidth;
                double maxHeight = isc != -1 ? System.Windows.Forms.Screen.AllScreens[isc].Bounds.Height : SystemParameters.VirtualScreenHeight;
                double move = Math.Sqrt(velo_x * velo_x + velo_y * velo_y + velo_z * velo_z);
                if (nx >= 0 && ny >= 0 && nx <= maxWidth && ny <= maxHeight)
                {
                    CurPos.nx -= velo_z * multiplier * CurPos.curSensivity;
                    CurPos.ny -= velo_x * multiplier * CurPos.curSensivity;
                    if (move > 0.1)
                        MainWindow.t.updateDigitizer((byte)(CurPos.isClicking ? 33 : 32), (ushort)CurPos.nx, (ushort)CurPos.ny, 8, isc);
                }
                return;
            }
            if (str.Contains("resetLinear"))
            {
                CurPos.linearVeloX = 0;
                CurPos.linearVeloY = 0;
                CurPos.linearVeloZ = 0;
                return;
            }
            if (str.Contains("linear"))
            {
                string[] info = str.Replace("linear:", "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                double acc_x = Infos.strToDouble(info[0]);
                double acc_y = Infos.strToDouble(info[1]);
                double acc_z = Infos.strToDouble(info[2]);
                changeIsMovingX(acc_x);
                changeIsMovingZ(acc_z);
                //Debug.WriteLine(CurPos.isMovingZ + " z " + acc_z);
                double dT = Infos.strToDouble(info[3]) * Math.Pow(10, -3);
                /*double a = 0.8;
                double velo_x = CurPos.linearVeloX * a + (acc_x * dT) * (1 - a);
                double velo_y = CurPos.linearVeloY * a + (acc_y * dT) * (1 - a);
                double velo_z = CurPos.linearVeloZ * a + (acc_z * dT) * (1 - a);*/
                /*double velo_x = CurPos.linearVeloX + (acc_x * dT);
                double velo_y = CurPos.linearVeloY + (acc_y * dT);
                double velo_z = CurPos.linearVeloZ + (acc_z * dT);*/
                /*if (CurPos.isMovingX == 0 && Math.Abs(CurPos.linearVeloX - velo_x) < 0.01) CurPos.linearVeloX = 0;
                if (acc_y <= 0.5 && acc_y >= -0.5 && Math.Abs(CurPos.linearVeloY - velo_y) < 0.01) CurPos.linearVeloY = 0;
                if (CurPos.isMovingZ == 0 && Math.Abs(CurPos.linearVeloZ - velo_z) < 0.01) CurPos.linearVeloZ = 0;
                velo_x = CurPos.linearVeloX + (acc_x * dT);
                velo_y = CurPos.linearVeloY + (acc_y * dT);
                velo_z = CurPos.linearVeloZ + (acc_z * dT);*/
                double velo_x = CurPos.linearVeloX + acc_x * dT;
                double velo_y = CurPos.linearVeloY + acc_y * dT;
                double velo_z = CurPos.linearVeloZ + acc_z * dT;
                double dist_x = CurPos.linearDistX + CurPos.linearVeloX * dT + 0.5 * (acc_x * Math.Pow(dT, 2));
                double dist_y = CurPos.linearDistY + CurPos.linearVeloY * dT + 0.5 * (acc_y * Math.Pow(dT, 2));
                double dist_z = CurPos.linearDistZ + CurPos.linearVeloZ * dT + 0.5 * (acc_z * Math.Pow(dT, 2));
                if (CurPos.isMovingX == 0) velo_x = 0;
                if (acc_y <= 0.5 && acc_y >= -0.5 && Math.Abs(CurPos.linearVeloY - velo_y) < 0.01) CurPos.linearVeloY = 0;
                if (CurPos.isMovingZ == 0) velo_z = 0;
                double multiplier = 5;
                double nx = CurPos.nx + (velo_x * multiplier * CurPos.curSensivity);
                double ny = CurPos.ny - (velo_z * multiplier * CurPos.curSensivity);
                double maxWidth = isc != -1 ? System.Windows.Forms.Screen.AllScreens[isc].Bounds.Width : SystemParameters.VirtualScreenWidth;
                double maxHeight = isc != -1 ? System.Windows.Forms.Screen.AllScreens[isc].Bounds.Height : SystemParameters.VirtualScreenHeight;
                Infos._main.Dispatcher.Invoke(() => Infos.newErrLog(null, dist_x + "  " + dist_z));
                if (nx >= 0 && ny >= 0 && nx <= maxWidth && ny <= maxHeight)
                {
                    CurPos.nx += velo_x * multiplier * CurPos.curSensivity;
                    CurPos.ny -= velo_z * multiplier * CurPos.curSensivity;
                    MainWindow.t.updateDigitizer((byte)(CurPos.isClicking ? 33 : 32), (ushort)CurPos.nx, (ushort)CurPos.ny, 8, isc);
                }
                CurPos.linearVeloX = velo_x;
                CurPos.linearVeloY = velo_y;
                CurPos.linearVeloZ = velo_z;
                CurPos.linearDistX = dist_x;
                CurPos.linearDistY = dist_y;
                CurPos.linearDistZ = dist_z;
                return;
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
            if (str.Contains("ready"))
            {
                //get max screen grid layout phone
                string[] info = str.Replace("ready:", "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                CurPos.curSensivity = Infos.strToDouble(info[1]);
                Infos._main.Dispatcher.Invoke(() =>
                {
                    if (int.Parse(info[0]) < Update.minVerCode)
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
