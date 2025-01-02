using DiscordGamePlayer.Models;
using DiscordGamePlayer.Services.Interfaces;
namespace DiscordGamePlayer.GamePlayer.Interfaces
{
    internal interface IServiceDirtyHooker : IService
    {
        Task InitializeAsync();
        Task<string> PeepinTom();
        Task SendKeyPressToRetroArch(string key);
    }
}
