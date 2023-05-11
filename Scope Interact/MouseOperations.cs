using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScopeInteract
{
    public class MouseOperations
    {
        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            MOUSEEVENTF_WHEEL = 0x0800
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public static void SetCursorPosition(int x, int y)
        {
            //SetCursorPos(x, y);
            mouse_event((int)MouseEventFlags.Move | (int)MouseEventFlags.Absolute, x, y, 0, 0);
        }

        public static void SetCursorPosition(MousePoint point)
        {
            SetCursorPos(point.X, point.Y);
        }

        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        public static void MouseEvent(MouseEventFlags value)
        {
            MousePoint position = GetCursorPosition();
            mouse_event((int)value, position.X, position.Y, 0, 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public static void MouseWheel(int delta)
        {
            MousePoint mp = GetCursorPosition();
            mouse_event((int)MouseEventFlags.MOUSEEVENTF_WHEEL, mp.X, mp.Y, delta, 0);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string fileName);

        public static void ChangeCursor(string curFile)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors\", "Arrow", curFile);
            SystemParametersInfo(SPI_SETCURSORS, 0, curFile, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        const int SPI_SETCURSORS = 0x0057;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDCHANGE = 0x02;

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
    }
}
