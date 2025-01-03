using DiscordGamePlayer.Models;
using DiscordGamePlayer.Services.Interfaces;
namespace DiscordGamePlayer.GamePlayer.Interfaces
{
    internal interface IServiceDirtyHooker : IService
    {
        Task InitializeAsync(string emulatorName);
        Task<string> GetEmulatorScreenshot();
        Task SendKeyPressToEmulator(string key);
        Task SendKeyPressToX11Window(string key);
    }
}
