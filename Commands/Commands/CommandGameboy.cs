using Discord.WebSocket;
using Discord;
using DiscordGamePlayer.Commands.Interfaces;
using DiscordGamePlayer.Services.Interfaces;
using DiscordGamePlayer.Services;
using DiscordGamePlayer.GamePlayer;
using System.Diagnostics;
using DiscordGamePlayer.GamePlayer.Interfaces;
using DiscordGamePlayer.Events;
using DiscordGamePlayer.Events.Events;

namespace DiscordGamePlayer.Commands.Commands
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
