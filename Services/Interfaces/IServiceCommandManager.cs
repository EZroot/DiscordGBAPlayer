using Discord.WebSocket;
namespace DiscordGamePlayer.Services.Interfaces
{
    internal interface IServiceCommandManager : IService
    {
        Task ExecuteCommand(SocketSlashCommand slashCommand);
        Task RegisterAllCommands(SocketGuild guild);
    }
}
