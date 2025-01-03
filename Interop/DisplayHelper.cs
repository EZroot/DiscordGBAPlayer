namespace DiscordGamePlayer.Interop
{
    public static class DisplayHelper
    {
        private static IntPtr _display = IntPtr.Zero;

        public static IntPtr GetDisplay()
        {
            if (_display == IntPtr.Zero)
            {
                _display = X11Interop.XOpenDisplay(null);
                if (_display == IntPtr.Zero)
                {
                    throw new Exception("Failed to open display");
                }
            }
            return _display;
        }

        public static void CloseDisplay()
        {
            if (_display != IntPtr.Zero)
            {
                X11Interop.XCloseDisplay(_display);
                _display = IntPtr.Zero;
            }
        }
    }
}
