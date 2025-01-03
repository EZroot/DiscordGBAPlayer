using Discord;
using Discord.WebSocket;
using DiscordGamePlayer.Events;
using DiscordGamePlayer.Events.EventArgs;
using DiscordGamePlayer.GamePlayer.Interfaces;
using DiscordGamePlayer.Models;
using DiscordGamePlayer.Services.Interfaces;
using DiscordGamePlayer.Utils;
using System.Globalization;

namespace DiscordGamePlayer.Services.Managers
{
    internal class BotManager : IServiceBotManager
    {
        private const double BOT_LOOP_TIMER_MS = 10000;
        Dictionary<string,string> _keyToButtonMap = new Dictionary<string,string>
        {
            { "left","\u2B05\uFE0F" }, // Left arrow (⬅️)
            { "up","\u2B06\uFE0F" }, // Up arrow (⬅️)
            { "right","\u27A1\uFE0F" }, // Right arrow (➡️)
            { "down","\u2B07\uFE0F" }, // Down arrow (⬇️)
            { "a","\uD83C\uDD70\uFE0F" }, // Regional indicator symbol letter A (🅰️) for A button
            { "b","\uD83C\uDD71\uFE0F" }, // Regional indicator symbol letter B (🅱️) for B button
            { "start","\uD83D\uDD1A" }, // Heavy plus sign for A button (➕)
            { "select","\uD83D\uDD1B" }, // Heavy minus sign for B button (➖)
        };

        private DiscordSocketClient? _client;
        private BotData _botData;
        private SocketGuild? _guild;
        private IUserMessage? _lastMessage;
        public DiscordSocketClient? Client => _client;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task Initialize()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildVoiceStates
            });

            _client.Log += Ev_Log;
            _client.Ready += Ev_ClientReady;
            _client.Connected += SubscribeToEvents;
            _client.Disconnected += UnsubscribeToEvents;
            _client.ButtonExecuted += Ev_ButtonExecutedAsync;

            _botData = Service.Get<IServiceDataManager>().LoadConfig();

            await Service.Get<IServiceAnalytics>().InitializeAsync();
            await Service.Get<IServiceDirtyHooker>().InitializeAsync(_botData.EmulatorName);
            await Service.Get<IServiceKeyMapper>().InitializeAsync(_botData.KeyMapper);

            var loopTimer = new BotTimer(BOT_LOOP_TIMER_MS);

            await _client.LoginAsync(TokenType.Bot, _botData.ApiKey);
            await _client.StartAsync();

            UpdateBotStatus();

            // Block this task
            await Task.Delay(-1);
        }

        private async Task SubscribeToEvents()
        {
            EventHub.Subscribe<EvForceReadyForUpdate>((a) => SendScreenshotToDiscord());
            EventHub.Subscribe<EvOnTimerLoop>((a) => UpdateBotStatus(ignoreSongPlaying: false));
            EventHub.Subscribe<EvOnFFmpegExit>((a) => UpdateBotStatus(ignoreSongPlaying: false));

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

        private async Task UnsubscribeToEvents(Exception exception)
        {
            EventHub.Unsubscribe<EvOnFFmpegExit>((a) => { Debug.Log("Unsubscribed from event EvOnFFmpegExit"); });
            EventHub.Unsubscribe<EvOnPlayNextSong>((a) => { Debug.Log("Unsubscribed from event EvOnFFmpegExit"); });
            EventHub.Unsubscribe<EvForceReadyForUpdate>((a) => { Debug.Log("Unsubscribed from event EvForceReadyForUpdate"); });
            EventHub.Unsubscribe<EvOnTimerLoop>((a) => { Debug.Log("Unsubscribed from event EvOnTimerLoop"); });
            await Task.CompletedTask;
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

        private void SendScreenshotToDiscord()
        {
            Task.Run(async () =>
            {
                //Exit if were already processing this task
                if (!await _semaphore.WaitAsync(0))
                    return;

                try
                {
                    var path = await Service.Get<IServiceDirtyHooker>().GetEmulatorScreenshot();
                    var channel = _guild.GetTextChannel(ulong.Parse(_botData.ChannelId));
                    if (_lastMessage != null)
                    {
                        await _lastMessage.DeleteAsync();
                        _lastMessage = null;
                    }

                    var buttonBuilder = new ComponentBuilder();
                    foreach (var entry in _keyToButtonMap)
                    {
                        var key = entry.Key;
                        var value = entry.Value;
                        var customid = key;
                        var emoji = value;
                        ButtonStyle style = StyleHelper.GetStyleByKey(key);
                        buttonBuilder.WithButton(customId: customid, style: style, emote: new Emoji(emoji));
                    }

                    var msg = await channel.SendFileAsync(path, components: buttonBuilder.Build());
                    if (_lastMessage == null)
                    {
                        _lastMessage = msg;
                    }
                }
                catch (Exception e)
                {
                    Utils.Debug.Log($"<color=red>ERROR: {e.Message}");
                }
                finally
                {
                    _semaphore.Release();
                }

            });
        }

        private void UpdateBotStatus(bool ignoreSongPlaying = true)
        {
            Task.Run(async () =>
            {
                // Debug.Log($"Timer called! Rdy:{_readyForUpdate}");
                if (_client == null) return;
                if (!ignoreSongPlaying && Service.Get<IServiceFFmpeg>().IsSongPlaying) return;
                await _client.SetCustomStatusAsync($"{GetRandomMotto(_botData)}");
            });
        }

        private async Task Ev_ButtonExecutedAsync(SocketMessageComponent component)
        {
            await component.DeferAsync();

            var key = component.Data.CustomId;
            Debug.Log($"Trying to execute: {key}");
            var keyCode = Service.Get<IServiceKeyMapper>().ParseKeyCode(component.Data.CustomId);
            await Service.Get<IServiceDirtyHooker>().SendKeyPressToEmulator(keyCode);

            Debug.Log($"<color=magenta>{component.User.Username}</color> <color=white>:></color> Button Sent: <color=cyan>{key}</color>");
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
            await Task.CompletedTask;
        }

        private static Task Ev_Log(LogMessage msg)
        {
            var colorTag = msg.Severity == LogSeverity.Error || msg.Severity == LogSeverity.Critical ? "red" : "white";
            colorTag = msg.Severity == LogSeverity.Warning ? "yellow" : colorTag;
            Debug.Log($"<color={colorTag}>{msg.ToString()}</color>");
            return Task.CompletedTask;
        }
    }
}
