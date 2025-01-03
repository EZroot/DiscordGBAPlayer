using Discord;
using Discord.WebSocket;

namespace DiscordGamePlayer.DiscordCommands.Interfaces
{
    internal interface IDiscordCommand
    {
        string CommandName { get; }
        SlashCommandBuilder Register();
        Task ExecuteAsync(SocketSlashCommand options);
    }
}
