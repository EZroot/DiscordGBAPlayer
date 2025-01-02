using Discord.WebSocket;
using Discord;
using DiscordGamePlayer.Commands.Interfaces;
using DiscordGamePlayer.Services.Interfaces;
using DiscordGamePlayer.Services;
using DiscordGamePlayer.GamePlayer;
using System.Diagnostics;
using DiscordGamePlayer.GamePlayer.Interfaces;
using System.Collections;
using DiscordGamePlayer.Events;
using DiscordGamePlayer.Events.Events;

namespace DiscordGamePlayer.Commands.Commands
{
    internal class CommandSequence : IDiscordCommand
    {
        private string _commandName = "sequence";
        public string CommandName => _commandName;

        public SlashCommandBuilder Register()
        {
            return new SlashCommandBuilder()
                .WithName(_commandName)
                .WithDescription("Input a sequence of button presses")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("buttons")
                    .WithDescription("ex: left left right up a b")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true));
        }

        public async Task ExecuteAsync(SocketSlashCommand command)
        {
            var userInput = (string)command.Data.Options.FirstOrDefault();
            
            await command.RespondAsync("Sending sequence...", ephemeral: true);

            var splitInput = userInput.Split(" ");
            var debug = "";
            foreach (var word in splitInput)
            {
                switch (word.Trim().ToLower())
                {
                    case "left":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Left");
                        debug += "left";
                        break;

                    case "up":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Up");
                        debug += "up";
                        break;

                    case "right":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Right");
                        debug += "right";
                        break;
                    case "down":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Down");
                        debug += "down";
                        break;

                    case "a":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("x");
                        debug += "a";
                        break;

                    case "b":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("z");
                        debug += "b";
                        break;
                }
            }
            Utils.Debug.Log($"<color=magenta>{command.User.Username}</color> <color=white>:></color> <color=cyan>Sequence: {debug} Executed!</color>");
            await command.DeleteOriginalResponseAsync();
            // await command.ModifyOriginalResponseAsync((a)=> { a.Content = "Finished"; });
            // var contentMsg = await command.ModifyOriginalResponseAsync((m) => { m.Content = message; });
            EventHub.Raise(new EvForceReadyForUpdate());

        }
    }
}
