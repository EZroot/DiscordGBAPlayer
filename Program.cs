using DiscordGamePlayer.Services;
using DiscordGamePlayer.Services.Interfaces;
using DiscordGamePlayer.GamePlayer;

namespace DiscordGamePlayer
{
    public class Program
    {
        public static async Task Main()
        {
            await Service.Get<IServiceBotManager>().Initialize();
        }
    }
}