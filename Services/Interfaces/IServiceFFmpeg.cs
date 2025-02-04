﻿using Discord.Audio;
using System.Diagnostics;

namespace DiscordGamePlayer.Services.Interfaces
{
    internal interface IServiceFFmpeg : IService
    {
        bool IsSongPlaying { get; }
        Process CreateStream(string url);
        Task StreamToDiscord(IAudioClient client, string url);
        bool ForceClose();
        Task SetVolume(float newVolume);

    }
}
