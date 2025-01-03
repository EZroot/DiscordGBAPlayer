using DiscordGamePlayer.Services;
using DiscordGamePlayer.Services.Interfaces;

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