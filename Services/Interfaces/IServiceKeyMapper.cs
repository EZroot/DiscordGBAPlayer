using Discord.Audio;
using System.Diagnostics;

namespace DiscordGamePlayer.Services.Interfaces
{
    internal interface IServiceKeyMapper : IService
    {
        Task InitializeAsync(Dictionary<string,string> keyMap);
        /// <summary>
        /// Returns an emulator key based on the keycode being parsed
        /// </summary>
        /// <param name="keycode"></param>
        /// <returns></returns>
        string ParseKeyCode(string keycode);
    }
}
