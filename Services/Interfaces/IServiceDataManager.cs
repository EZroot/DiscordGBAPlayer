using DiscordGamePlayer.Models;
namespace DiscordGamePlayer.Services.Interfaces
{
    internal interface IServiceDataManager : IService
    {
        BotData LoadConfig();
        AnalyticData LoadAnalytics();
        Task SaveAnalytics(AnalyticData data);

    }
}
