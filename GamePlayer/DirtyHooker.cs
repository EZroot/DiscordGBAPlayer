using DiscordGamePlayer.GamePlayer.Interfaces;

namespace DiscordGamePlayer.GamePlayer
{
    internal class DirtyHooker : IServiceDirtyHooker
    {
        private ProcessHelper _processHelper;
        private int _pid;

        public async Task InitializeAsync()
        {
            _processHelper = new ProcessHelper();

            _pid = await FindRetroArchPid();
            if (_pid == -1)
            {
                Utils.Debug.Log("RetroArch is not running or PID could not be found.");
            }
            Utils.Debug.Log
            ("Found pid " + _pid.ToString());
        }

        public async Task<string> PeepinTom()
        {
            string screenshotDirectory = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
            Directory.CreateDirectory(screenshotDirectory);

            string windowId = await GetWindowIdFromPid(_pid);
            string name = Guid.NewGuid().ToString() + ".png"; 
            string fullPath = Path.Combine(screenshotDirectory, name);
            
            if (!string.IsNullOrEmpty(windowId))
            {
                await Task.Delay(1500);
                string activateCommand = $"xdotool windowactivate {windowId} && xdotool windowraise {windowId}";
                await _processHelper.RunCommandAsync(activateCommand);
                // await Task.Delay(500);  // Allow some time for the window to be activated and raised

                string screenshotCommand = $"import -window {windowId} '{fullPath}'";
                await _processHelper.RunCommandAsync(screenshotCommand);
            }
            else
            {
                Utils.Debug.Log("<color=red>ERROR: No window found for PID: " + _pid);
            }
            return fullPath;
        }


        public async Task SendKeyPressToRetroArch(string key)
        {
            string windowId = await GetWindowIdFromPid(_pid);
            if (!string.IsNullOrEmpty(windowId))
            {
                string activateCommand = $"xdotool windowactivate {windowId} && xdotool windowraise {windowId}";
                await _processHelper.RunCommandAsync(activateCommand);
                await Task.Delay(1000); 

                string keyPressCommand = $"xdotool windowfocus {windowId} key {key}";
                Utils.Debug.Log($"Sent Key: {keyPressCommand}");
                await _processHelper.RunCommandAsync(keyPressCommand);
            }
            else
            {
                Utils.Debug.Log("<color=red>ERROR: No window found for PID: " + _pid);
            }
        }


        private async Task<string> GetWindowIdFromPid(int pid)
        {
            string command = $"xdotool search --pid {pid}";
            string output = await _processHelper.RunCommandAndReturnOutputAsync(command);
            return output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private async Task<int> FindRetroArchPid()
        {
            string command = "pgrep -x retroarch";
            string output = await _processHelper.RunCommandAndReturnOutputAsync(command);
            if (!string.IsNullOrEmpty(output))
            {
                return int.Parse(output.Trim().Split('\n')[0]);  
            }
            return -1; 
        }
    }
}
