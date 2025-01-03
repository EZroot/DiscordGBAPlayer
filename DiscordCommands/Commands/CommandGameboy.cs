using Discord.WebSocket;
using Discord;
using DiscordGamePlayer.DiscordCommands.Interfaces;
using DiscordGamePlayer.Events;
using DiscordGamePlayer.Events.EventArgs;

namespace DiscordGamePlayer.DiscordCommands.Commands
{
    internal class CommandGameboy : IDiscordCommand
    {
        private string _commandName = "gameboy";

        public string CommandName => _commandName;

        public SlashCommandBuilder Register()
        {

            return new SlashCommandBuilder()
            .WithName(_commandName)
            .WithDescription("Play whatever game boy games loaded");
        }

        public async Task ExecuteAsync(SocketSlashCommand command)
        {
            await command.RespondAsync("Launching Pokemon - Emerald!");
            EventHub.Raise(new EvForceReadyForUpdate());
        }

    }
}
