using System;
using System.Runtime.InteropServices;

namespace DiscordGamePlayer.Interop
{
    public static class X11Interop
    {
        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenDisplay(string display);

        [DllImport("libX11.so.6")]
        public static extern void XCloseDisplay(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern bool XSendEvent(IntPtr display, IntPtr window, bool propagate, long eventMask, ref XEvent eventSend);

        [DllImport("libX11.so.6")]
        public static extern void XFlush(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XRootWindow(IntPtr display, int screen);

        [DllImport("libX11.so.6")]
        public static extern int XDefaultScreen(IntPtr display);

        [StructLayout(LayoutKind.Sequential)]
        public struct XEvent
        {
            public int type;
            public ulong serial; // Unsigned long
            public bool send_event; // Bool
            public IntPtr display; // Display pointer
            public IntPtr window; // Window (unsigned long)
            public IntPtr root; // Window
            public IntPtr subwindow; // Window
            public ulong time; // Unsigned long
            public int x, y; // Integers
            public int x_root, y_root; // Integers
            public uint state; // Unsigned int
            public uint keycode; // Unsigned int
            public bool same_screen; // Bool

            // Additional fields might be required depending on the type of event (padding, etc.)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] pad; // Padding to match the native structure size and alignment
        }
    }
}
