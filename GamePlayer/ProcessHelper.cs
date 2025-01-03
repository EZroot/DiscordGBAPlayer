using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DiscordGamePlayer.GamePlayer
{
    internal class ProcessHelper
    {
        public async Task RunCommandAsync(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process proc = new Process { StartInfo = startInfo })
            {
                proc.Start();
                await proc.WaitForExitAsync();
                string output = await proc.StandardOutput.ReadToEndAsync();
                Debug.WriteLine(output);
            }
        }

        public void RunCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process proc = new Process { StartInfo = startInfo })
            {
                proc.Start();
                proc.WaitForExit();
                string output = proc.StandardOutput.ReadToEnd();
                Debug.WriteLine(output);
            }
        }

        public async Task<string> RunCommandAndReturnOutputAsync(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process proc = new Process { StartInfo = startInfo })
            {
                proc.Start();
                await proc.WaitForExitAsync();
                return await proc.StandardOutput.ReadToEndAsync();
            }
        }
    }
}
