using System;
using System.Runtime.InteropServices;

namespace AmeisenBotCore
{
    public static class SafeNativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }
}
