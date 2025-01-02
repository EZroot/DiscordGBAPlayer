using Discord;
using Discord.WebSocket;
using DiscordGamePlayer.Events;
using DiscordGamePlayer.Events.Events;
using DiscordGamePlayer.GamePlayer.Interfaces;
using DiscordGamePlayer.Models;
using DiscordGamePlayer.Services.Interfaces;
using DiscordGamePlayer.Utils;
using System.Globalization;

namespace DiscordGamePlayer.Services.Managers
{
    internal class BotManager : IServiceBotManager
    {
        string[] _numberEmojis = new string[]
        {
            "\u2B05\uFE0F", // Left arrow (⬅️)
            "\u2B06\uFE0F", // Up arrow (⬆️)
            "\u27A1\uFE0F", // Right arrow (➡️)
            "\u2B07\uFE0F", // Down arrow (⬇️)
            "\uD83C\uDD70\uFE0F", // Regional indicator symbol letter A (🅰️) for A button
            "\uD83C\uDD71\uFE0F", // Regional indicator symbol letter B (🅱️) for B button
            "\uD83D\uDD1A", // Heavy plus sign for A button (➕)
            "\uD83D\uDD1B", // Heavy minus sign for B button (➖)
        };

        private DiscordSocketClient? _client;
        private BotData _botData;
        private BotTimer _botTimer;
        private SocketGuild? _guild;
        private IUserMessage? _lastMessage;
        private bool _readyForUpdate;
        public DiscordSocketClient? Client => _client;
        public async Task Initialize()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildVoiceStates
            });

            _client.Log += Ev_Log;
            _client.Ready += Ev_ClientReady;
            _client.ReactionAdded += Ev_ReactionAddedAsync;
            _client.Connected += SubscribeToEvents;
            _client.Disconnected += UnsubscribeToEvents;
            _client.ButtonExecuted += Ev_ButtonExecutedAsync;

            _botTimer = new BotTimer(10000);
            await Service.Get<IServiceAnalytics>().Initialize();
            await Service.Get<IServiceDirtyHooker>().InitializeAsync();
            _botData = Service.Get<IServiceDataManager>().LoadConfig();

            await _client.LoginAsync(TokenType.Bot, _botData.ApiKey);
            await _client.StartAsync();

            await UpdateBotStatus();

            // Block this task
            await Task.Delay(-1);
        }

        private async Task Ev_ButtonExecutedAsync(SocketMessageComponent component)
        {
            await component.DeferAsync();

            //TODO: clean this up
            switch (component.Data.CustomId)
            {
                case "press_0":
                    // await component.RespondAsync("You pressed left!");
                    await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Left");
                    break;
                case "press_1":
                    // await component.RespondAsync("You pressed up!");
                    await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Up");
                    break;
                case "press_2":
                    // await component.RespondAsync("You pressed right!");
                    await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Right");
                    break;
                case "press_3":
                    // await component.RespondAsync("You pressed down!");
                    await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Down");
                    break;
                case "press_4":
                    // await component.RespondAsync("You pressed A!");
                    await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("x");
                    break;
                case "press_5":
                    // await component.RespondAsync("You pressed B!");
                    await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("z");
                    break;
                    // case "press_6":
                    //     await component.RespondAsync("You pressed Start!");
                    //     await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Enter");
                    //     break;
                    // case "press_7":
                    //     await component.RespondAsync("You pressed Select!");
                    //     await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Left");
                    //     break;
            }
            Debug.Log($"<color=magenta>{component.User.Username}</color> <color=white>:></color> Button Sent: <color=cyan>{component.Data.CustomId}</color>");
            EventHub.Raise(new EvForceReadyForUpdate());
        }

        private async Task Ev_ClientReady()
        {
            // Ensure you have the correct guild ID (Replace it with your server id)
            var botData = Service.Get<IServiceDataManager>().LoadConfig();
            ulong guildId = ulong.Parse(botData.GuildId);
            if (guildId == 0) Debug.Log("<color=red>Invalid guild id. Bot may not work correctly. (Registering commands)</color>");
            _guild = _client?.GetGuild(guildId);

            // - Clear all server slash commands ---
            //  await SlashCommandClear(_guild); 
            // -------------------------------------------------

            if (_guild != null) await Service.Get<IServiceCommandManager>().RegisterAllCommands(_guild);
            if (_client != null) _client.SlashCommandExecuted += Ev_SlashCommandHandler;
        }

        private async Task Ev_SlashCommandHandler(SocketSlashCommand command)
        {
            _ = Task.Run(async () =>
            {
                await Service.Get<IServiceCommandManager>().ExecuteCommand(command);
            });
        }

        private async Task Ev_ReactionAddedAsync(Cacheable<IUserMessage, ulong> cacheable, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var message = await cacheable.GetOrDownloadAsync();
            if (message == null || message.Author.Id != _client.CurrentUser.Id) return;
            _ = Task.Run(async () =>
            {

                // Replace with your custom emoji ID and name
                //var emojiIds = new ulong[] { 429753831199342592, 466478794367041557, 466477774455177247, 582418378178822144 };
                for (int i = 0; i < _numberEmojis.Length; i++)
                {
                    //For custom emotes
                    //var emote = Emote.Parse($"<:warrior{i}:{emojiIds[i]}>");
                    // if (reaction.Emote is Emote e && e.Id == emojiIds[i])
                    if (GetUnicodeCodePoints(reaction.Emote.Name) == GetUnicodeCodePoints(_numberEmojis[i]))
                    {
                        var user = reaction.User.IsSpecified ? reaction.User.Value : null;
                        if (user == null) return;
                        if (user.IsBot) return;


                        // i = 0 "\u2B05\uFE0F", // Left arrow (⬅️)
                        // i = 1  "\u2B06\uFE0F", // Up arrow (⬆️)
                        // i = 2  "\u27A1\uFE0F", // Right arrow (➡️)
                        // i = 3  "\u2B07\uFE0F", // Down arrow (⬇️)
                        // i = 4  "\uD83C\uDD70\uFE0F", // Regional indicator symbol letter A (🅰️) for A button
                        // i = 5  "\uD83C\uDD71\uFE0F", // Regional indicator symbol letter B (🅱️) for B button

                        switch (i)
                        {
                            case 0:
                                await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Left");
                                break;

                            case 1:
                                await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Up");
                                break;

                            case 2:
                                await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Right");
                                break;
                            case 3:
                                await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("Down");
                                break;

                            case 4:
                                await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("x");
                                break;

                            case 5:
                                await Service.Get<IServiceDirtyHooker>().SendKeyPressToRetroArch("z");
                                break;

                        }

                        Debug.Log($"<color=red>{user.Username}</color> <color=white>picked </color> i={i} <color=cyan>{reaction.Emote.Name}#</color>");
                        EventHub.Raise(new EvForceReadyForUpdate());
                        // var results = Service.Get<IServiceYtdlp>().SearchResults;
                        // _ = Task.Run(async () =>
                        // {
                        //     await Service.Get<IServiceAudioManager>().PlaySong(results[i].Title, results[i].Url);
                        // });
                        // await message.ModifyAsync((m) => m.Content = $"{user.Username} Picked emote {reaction.Emote.Name}");
                        // await Task.Delay(5000); 
                        // await message.DeleteAsync();
                    }
                }
            });
        }

        private static Task Ev_Log(LogMessage msg)
        {
            var colorTag = msg.Severity == LogSeverity.Error || msg.Severity == LogSeverity.Critical ? "red" : "white";
            colorTag = msg.Severity == LogSeverity.Warning ? "yellow" : colorTag;
            Debug.Log($"<color={colorTag}>{msg.ToString()}</color>");
            return Task.CompletedTask;
        }

        private async Task SubscribeToEvents()
        {
            EventHub.Subscribe<EvForceReadyForUpdate>((a) =>
            {
                Task.Run(async () =>
                {
                    var path = await Service.Get<IServiceDirtyHooker>().PeepinTom();
                    // Debug.Log("Trying to send file");
                    var channel = _guild.GetTextChannel(ulong.Parse(_botData.ChannelId));
                    if (_lastMessage != null)
                    {
                        await _lastMessage.DeleteAsync();
                        _lastMessage = null;
                    }

                    var buttonBuilder = new ComponentBuilder();

                    // Add buttons for each emoji in the array
                    for (int i = 0; i < _numberEmojis.Length; i++)
                    {
                        string? emoji = _numberEmojis[i];
                        // Determine button style based on the emoji for demonstration (customize as needed)
                        ButtonStyle style = GetStyleByIndex(i);

                        // Create each button with the emoji as the label and custom ID
                        buttonBuilder.WithButton(
                            // label: new Emoji(emoji).Name,   // Use the emoji itself as a label
                            customId: "press_" + i,     // Unique custom ID for each button
                            style: style,                   // Style based on the emoji
                            emote: new Emoji(emoji)         // The actual emoji displayed on the button
                        );
                    }
                    var msg = await channel.SendFileAsync(path, components: buttonBuilder.Build());
                    if (_lastMessage == null) _lastMessage = msg;
                    
                    // for (int i = 0; i < _numberEmojis.Length; i++)
                    // {
                    //     //var emote = Emote.Parse($"<:warrior{i}:{emojiIds[i]}>");
                    //     await msg.AddReactionAsync(new Emoji(_numberEmojis[i]));
                    // }
                });
            });

            EventHub.Subscribe<EvOnTimerLoop>((a) =>
            {
                Task.Run(async () =>
                {

                    // Debug.Log($"Timer called! Rdy:{_readyForUpdate}");
                    if (_client == null) return;
                    if (Service.Get<IServiceFFmpeg>().IsSongPlaying) return;
                    await UpdateBotStatus();

                    if (_readyForUpdate)
                    {
                        _readyForUpdate = false;
                        var path = await Service.Get<IServiceDirtyHooker>().PeepinTom();
                        var msg = await _guild.DefaultChannel.SendFileAsync(path);
                        if (_lastMessage != null)
                        {
                            await _lastMessage.DeleteAsync();
                            _lastMessage = null;
                        }
                        for (int i = 0; i < _numberEmojis.Length; i++)
                        {
                            //var emote = Emote.Parse($"<:warrior{i}:{emojiIds[i]}>");
                            await msg.AddReactionAsync(new Emoji(_numberEmojis[i]));
                        }
                    }
                });
            });

            EventHub.Subscribe<EvOnFFmpegExit>((a) =>
            {
                if (Service.Get<IServiceAudioManager>().SongCount > 0) return;
                Task.Run(async () =>
                {
                    if (_client == null) return;
                    await UpdateBotStatus();
                });
            });

            EventHub.Subscribe<EvOnPlayNextSong>((a) =>
            {
                Task.Run(async () =>
                {
                    Debug.Log("EvOnPlayNextSong! Updating status song playing");
                    if (_client == null) return;
                    await _client.SetCustomStatusAsync($"Playin '{a.Title}'");
                });
            });

            await Task.CompletedTask;
        }
        private ButtonStyle GetStyleByIndex(int index)
        {
            //             "\u2B05\uFE0F", // Left arrow (⬅️)
            // "\u2B06\uFE0F", // Up arrow (⬆️)
            // "\u27A1\uFE0F", // Right arrow (➡️)
            // "\u2B07\uFE0F", // Down arrow (⬇️)
            // "\uD83C\uDD70\uFE0F", // Regional indicator symbol letter A (🅰️) for A button
            // "\uD83C\uDD71\uFE0F", // Regional indicator symbol letter B (🅱️) for B button
            // "\uD83D\uDD1A", // Heavy plus sign for A button (➕)
            // "\uD83D\uDD1B", // Heavy minus sign for B button (➖)
            switch (index)
            {
                case 0:
                    return ButtonStyle.Primary;
                case 1:
                    return ButtonStyle.Primary;
                case 2:
                    return ButtonStyle.Primary;
                case 3:
                    return ButtonStyle.Primary;
                case 4:
                    return ButtonStyle.Danger;
                case 5:
                    return ButtonStyle.Danger;
                case 6:
                    return ButtonStyle.Secondary;
                case 7:
                    return ButtonStyle.Secondary;
            }
            return ButtonStyle.Premium;
        }
        private async Task UnsubscribeToEvents(Exception exception)
        {
            EventHub.Unsubscribe<EvOnFFmpegExit>((a) => { Debug.Log("Unsubscribed from event EvOnFFmpegExit"); });
            EventHub.Unsubscribe<EvOnPlayNextSong>((a) => { Debug.Log("Unsubscribed from event EvOnFFmpegExit"); });
            EventHub.Unsubscribe<EvForceReadyForUpdate>((a) => { Debug.Log("Unsubscribed from event EvForceReadyForUpdate"); });
            EventHub.Unsubscribe<EvOnTimerLoop>((a) => { Debug.Log("Unsubscribed from event EvOnTimerLoop"); });
            await Task.CompletedTask;
        }

        private async Task UpdateBotStatus()
        {
            // var currentTime = DateTime.Now.ToString("h:mmtt");
            // currentTime = currentTime.Replace(".", "");
            // await _client.SetCustomStatusAsync($"[{currentTime}] {GetRandomMotto(_botData)}");
            await _client.SetCustomStatusAsync($"{GetRandomMotto(_botData)}");
        }

        private string GetUnicodeCodePoints(string input)
        {
            StringInfo stringInfo = new StringInfo(input);
            string result = "";

            for (int i = 0; i < stringInfo.LengthInTextElements; i++)
            {
                string textElement = stringInfo.SubstringByTextElements(i, 1);
                foreach (char c in textElement)
                {
                    result += $"\\u{((int)c):X4}";
                }
            }

            return result;
        }

        private string GetRandomMotto(BotData botData)
        {
            var specialMotto = "";
            if (DateTime.Now.Month == 12) specialMotto = "Merry Christmas!"; //december
            if (DateTime.Now.Month == 1) specialMotto = "Happy new year!"; //january
            if (DateTime.Now.Month == 10) specialMotto = "Spooky scary skeletons!";  //october

            var motto = new string[botData.CustomStatus.Length + 1];
            for (var i = 0; i < motto.Length; i++)
            {
                if (i >= botData.CustomStatus.Length) break;
                motto[i] = botData.CustomStatus[i];
            }
            motto[motto.Length - 1] = specialMotto;
            return motto[Random.Shared.Next(motto.Length)];
        }

        private async Task SlashCommandClear(SocketGuild guild)
        {
            // Clear existing commands
            _ = Task.Run(async () =>
            {
                var commands = await guild.GetApplicationCommandsAsync();
                foreach (var command in commands)
                {
                    await command.DeleteAsync();
                }
            });
        }
    }
}
