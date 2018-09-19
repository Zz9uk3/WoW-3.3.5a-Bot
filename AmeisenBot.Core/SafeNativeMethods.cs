using System;
using System.Runtime.InteropServices;

namespace AmeisenBotCore
{
    public static class SafeNativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        //[return: MarshalAs(UnmanagedType.U4)]
        //public static extern int GetIpNetTable(IntPtr pIpNetTable, [MarshalAs(UnmanagedType.U4)] ref int pdwSize, bool bOrder);
    }
}
