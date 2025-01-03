namespace DiscordGamePlayer.Models
{
    [System.Serializable]
    public struct BotData
    {
        public string ApiKey;
        public string GuildId; 
        public string ChannelId; 
        public string EmulatorName;
        public string EnvPath;
        public bool DebugMode;
        public string[] CustomStatus;
        public Dictionary<string,string> KeyMapper;
        // public Dictionary<string,string> CustomButtonLayout;
    }
}
