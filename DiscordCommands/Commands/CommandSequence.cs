using Discord.WebSocket;
using Discord;
using DiscordGamePlayer.Services;
using DiscordGamePlayer.Events;
using DiscordGamePlayer.Events.EventArgs;
using DiscordGamePlayer.DiscordCommands.Interfaces;
using DiscordGamePlayer.GamePlayer.Interfaces;

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
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToEmulator("Left");
                        debug += "left";
                        break;

                    case "up":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToEmulator("Up");
                        debug += "up";
                        break;

                    case "right":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToEmulator("Right");
                        debug += "right";
                        break;
                    case "down":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToEmulator("Down");
                        debug += "down";
                        break;

                    case "a":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToEmulator("x");
                        debug += "a";
                        break;

                    case "b":
                        await Service.Get<IServiceDirtyHooker>().SendKeyPressToEmulator("z");
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
