using Discord.Audio;
using DiscordGamePlayer.Models;

namespace DiscordGamePlayer.Services.Interfaces
{
    internal interface IServiceYtdlp : IService
    {
        List<SongData> SearchResults { get; }
        Task<List<SongData>>? SearchYoutube(string query, int maxResults = 5);
        Task StreamToDiscord(IAudioClient client, string videoUrl);
        Task<string> GetSongTitle(string videoUrl);
        bool IsYouTubeUrl(string url);
    }
}
