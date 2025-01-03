using System.Diagnostics;
using DiscordGamePlayer.GamePlayer.Interfaces;

namespace DiscordGamePlayer.GamePlayer
{
    internal class DirtyHooker : IServiceDirtyHooker
    {
        private const int SCREENSHOT_DELAY = 1500;
        private const int KEYPRESS_DELAY = 1000;

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
            // await Task.Delay(500);  // Allow some time for the window to be activated and raised

            string screenshotCommand = $"import -window {windowId} '{fullPath}'";
            await _processHelper.RunCommandAsync(screenshotCommand);

            return fullPath;
        }

        public async Task SendKeyPressToEmulator(string key)
        {
            string windowId = await GetWindowIdFromPid(_pid);

            if (string.IsNullOrEmpty(windowId) || _processHelper == null)
            {
                Utils.Debug.Log($"<color=red>ERROR: No window found for PID: {_pid} OR processHelper == null");
                return;
            }

            string activateCommand = $"xdotool windowactivate {windowId} && xdotool windowraise {windowId}";
            await _processHelper.RunCommandAsync(activateCommand);
            
            await Task.Delay(KEYPRESS_DELAY);

            string keyPressCommand = $"xdotool windowfocus {windowId} key {key}";
            await _processHelper.RunCommandAsync(keyPressCommand);
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
    }
}
