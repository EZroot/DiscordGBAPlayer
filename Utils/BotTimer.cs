
using System.Timers;
using DiscordGamePlayer.Events;
using DiscordGamePlayer.Events.Events;

namespace DiscordGamePlayer.Utils
{
    public class BotTimer
    {
        private System.Timers.Timer _timer;
        public BotTimer(double interval = 15000)
        {
            _timer = new System.Timers.Timer(interval);
            _timer.Elapsed += OnTimedEvent; 
            _timer.AutoReset = true; 
            _timer.Enabled = true; 
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            EventHub.Raise(new EvOnTimerLoop());
        }
    }
}