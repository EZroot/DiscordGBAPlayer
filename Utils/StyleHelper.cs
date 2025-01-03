
using Discord;

namespace DiscordGamePlayer.Utils
{
    public static class StyleHelper
    {
        public static ButtonStyle GetStyleByKey(string key)
        {
            switch (key)
            {
                case "left":
                case "up":
                case "right":
                case "down":
                    return ButtonStyle.Primary;
                case "a":
                case "b":
                    return ButtonStyle.Danger;
                case "start":
                case "select":
                    return ButtonStyle.Secondary;
            }
            return ButtonStyle.Primary;
        }
    }
}