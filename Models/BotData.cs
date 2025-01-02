namespace DiscordGamePlayer.Models
{
    [System.Serializable]
    public struct BotData
    {
        public string ApiKey;
        public string EnvPath;
        public string GuildId; 
        public string ChannelId; 
        public string[] CustomStatus;
        public bool DebugMode;
    }
}
