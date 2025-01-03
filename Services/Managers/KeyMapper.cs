using DiscordGamePlayer.Services.Interfaces;
using Discord;
using Discord.WebSocket;
using System.Reflection;
using DiscordGamePlayer.Utils;
using DiscordGamePlayer.DiscordCommands.Interfaces;

namespace DiscordGamePlayer.Services.Managers
{
    internal class KeyMapper : IServiceKeyMapper
    {
        private Dictionary<string,string> _keyMap;

        public async Task InitializeAsync(Dictionary<string, string> keyMap)
        {
            _keyMap = keyMap;

            foreach(var key in keyMap)
            {
                Debug.Log($"<color=green>Registered:</color><color=white> {key.Key} <-> {key.Value}");
            }
            
            await Task.CompletedTask;
        }

        public string ParseKeyCode(string keycode)
        {
            Debug.Log($"Trying to parse: {keycode}");
            Debug.Log($"Parsed: {keycode} -> {_keyMap[keycode]}");
            return _keyMap[keycode];
        }
    }
}
