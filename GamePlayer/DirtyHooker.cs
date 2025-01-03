using System.Diagnostics;
using DiscordGamePlayer.GamePlayer.Interfaces;
using DiscordGamePlayer.Interop;

namespace DiscordGamePlayer.GamePlayer
{
    internal class DirtyHooker : IServiceDirtyHooker
    {
        private const int SCREENSHOT_DELAY = 300;
        private const int KEYPRESS_DELAY = 100;

        private ProcessHelper? _processHelper;
        private int _pid;

        public async Task InitializeAsync(string emulatorName)
        {
            _processHelper = new ProcessHelper();

            _pid = await FindEmulatorPid(emulatorName);
            if (_pid == -1)
            {
                Utils.Debug.Log("<color=red>ERROR: RetroArch is not running or PID could not be found.");
                Utils.Debug.Log("<color=red>Exiting......");
                Console.ReadLine();
                Environment.Exit(0);
            }
            Utils.Debug.Log("Found pid " + _pid.ToString());
        }

        public async Task<string> GetEmulatorScreenshot()
        {
            string screenshotDirectory = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
            Directory.CreateDirectory(screenshotDirectory);

            string windowId = await GetWindowIdFromPid(_pid);
            string name = Guid.NewGuid().ToString() + ".png";
            string fullPath = Path.Combine(screenshotDirectory, name);

            if (string.IsNullOrEmpty(windowId) || _processHelper == null)
            {
                Utils.Debug.Log($"<color=red>ERROR: No window found for PID: {_pid}  OR _processHelper = null");
                return "error - getemulatorscreenshot";
            }

            await Task.Delay(SCREENSHOT_DELAY);
            string activateCommand = $"xdotool windowactivate {windowId} && xdotool windowraise {windowId}";
            await _processHelper.RunCommandAsync(activateCommand);

            string screenshotCommand = $"import -window {windowId} '{fullPath}'";
            await _processHelper.RunCommandAsync(screenshotCommand);

            return fullPath;
        }

        //TODO: FIX THIS
        public async Task SendKeyPressToX11Window(string key)
        {
            string windowId = await GetWindowIdFromPid(_pid);
            if (string.IsNullOrEmpty(windowId))
            {
                Console.WriteLine("Failed to get window ID from PID.");
                return;
            }

            IntPtr display = DisplayHelper.GetDisplay();
            IntPtr window = new IntPtr(Convert.ToInt64(windowId, 16));
            int keycode = GetKeyCodeFromKey(key);

            if (keycode == -1)
            {
                Console.WriteLine("Unsupported key.");
                return;
            }

            X11Interop.XEvent eventToSend = new X11Interop.XEvent
            {
                type = 2,  // KeyPress
                window = window,
                keycode = (uint)keycode,
                state = 0  // No modifier keys
            };

            X11Interop.XSendEvent(display, window, false, 1L << 0, ref eventToSend);  // KeyPressMask
            X11Interop.XFlush(display);
        }

        public async Task SendKeyPressToEmulator(string key)
        {
            string windowId = await GetWindowIdFromPid(_pid);

            if (string.IsNullOrEmpty(windowId) || _processHelper == null)
            {
                Utils.Debug.Log($"<color=red>ERROR: No window found for PID: {_pid} OR processHelper == null");
                return;
            }

            string keyPressCommand = $"xdotool windowactivate {windowId} && xdotool windowraise {windowId} && xdotool windowfocus {windowId} && xdotool key --clearmodifiers {key}";
            await _processHelper.RunCommandAsync(keyPressCommand);
            
            await Task.Delay(KEYPRESS_DELAY);
        }

        private async Task<string> GetWindowIdFromPid(int pid)
        {
            if (_processHelper == null)
            {
                Utils.Debug.Log("<color=red>ERROR: ProcessHelper null.");
                return "error - getwindowidfrompid";
            }

            string command = $"xdotool search --pid {pid}";
            string output = await _processHelper.RunCommandAndReturnOutputAsync(command);
            return output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private async Task<int> FindEmulatorPid(string emulatorName)
        {
            if (_processHelper == null)
            {
                Utils.Debug.Log("<color=red>ERROR: ProcessHelper null.");
                return -1;
            }

            string command = $"pgrep -x {emulatorName}";
            string output = await _processHelper.RunCommandAndReturnOutputAsync(command);
            if (!string.IsNullOrEmpty(output))
            {
                return int.Parse(output.Trim().Split('\n')[0]);
            }

            return -1;
        }

        private int GetKeyCodeFromKey(string key)
        {
            switch (key.ToUpper())
            {
                case "X": return 53;
                case "Z": return 52;
                case "LEFT": return 113;
                case "RIGHT": return 114;
                case "UP": return 111;
                case "DOWN": return 116;
                case "ENTER": return 36;
                default: return -1;
            }
        }
    }
}
