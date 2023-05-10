using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace ScopeInteract
{
    public class HIDWrapper : IDisposable
    {
        [DllImport("vmulticlient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr VTDCreate();

        [DllImport("vmulticlient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int VTDGetStatus(IntPtr pObject);

        [DllImport("vmulticlient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VTDDispose(IntPtr pObject);

        [DllImport("vmulticlient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VTDUpdateDigitizerAbs_Full(IntPtr pObject, byte status, ushort x, ushort y, ushort pressure, ushort tiltX, ushort tiltY);

        [DllImport("vmulticlient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void VTDGetLogs(StringBuilder str, int len);

        public HIDWrapper()
        {
            m_pNativeObject = VTDCreate();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool bDisposing)
        {
            if (m_pNativeObject != IntPtr.Zero)
            {
                VTDDispose(m_pNativeObject);
                m_pNativeObject = IntPtr.Zero;
            }
            if (bDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        ~HIDWrapper()
        {
            Dispose(false);
        }

        //STATUS : 33 LEFT CLICK | 32 HOVER | 35 RIGHT CLICK etc
        public void updateDigitizer(byte status, ushort x, ushort y, byte pressure, int monitor)
        {
            double w = SystemParameters.VirtualScreenWidth / 128;
            double h = SystemParameters.VirtualScreenHeight / 128;
            //
            double ux = 0;
            double uy = 0;
            //
            if (monitor > -1)
            {
                double top = 0;
                double left = 0;
                foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
                {
                    if (s.Bounds.Y < top && s.Bounds.Y < 0) top = s.Bounds.Y * -1;
                    if (s.Bounds.X < left && s.Bounds.X < 0) left = s.Bounds.X * -1;
                }
                //
                ux = System.Windows.Forms.Screen.AllScreens[monitor].Bounds.X + left;
                uy = System.Windows.Forms.Screen.AllScreens[monitor].Bounds.Y + top;
            }
            //
            double Xquotient = (x + ux) / w;
            double Xreste = ((x + ux) % w) * 256 / w;
            double Yquotient = (y + uy) / h;
            double Yreste = ((y + uy) % h) * 256 / h;
            //
            VTDUpdateDigitizerAbs_Full(m_pNativeObject, status, (ushort)Yquotient, pressure, (ushort)Xreste, (ushort)Xquotient, (ushort)Yreste);
        }

        public bool IsDriverConnected => Status == HIDStatus.Connected;

        public HIDStatus Status => (HIDStatus)VTDGetStatus(m_pNativeObject);

        public string ErrorLogs
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(40960);
                VTDGetLogs(stringBuilder, stringBuilder.Capacity);
                return stringBuilder.ToString();
            }
        }

        private IntPtr m_pNativeObject;
    }

    public enum HIDStatus
    {
        None,
        Connected,
        AllocFailed,
        ConnectFailed
    }
}