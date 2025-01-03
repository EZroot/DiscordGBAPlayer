using DiscordGamePlayer.Commands.Interfaces;
using DiscordGamePlayer.Models;

namespace DiscordGamePlayer.Commands.CommandArgs
{
    public class CmdForceReadyUpdate : ICommand
    {
        private bool _forceUpdate;

        public CmdForceReadyUpdate(bool forceUpdate)
        {
            _forceUpdate = forceUpdate;
        }

        public async Task ExecuteAsync()
        {
            await Task.CompletedTask;
        }

        public async Task Redo()
        {
            Utils.Debug.Log($"<color=red>Error: Redo unavailable in {this}");
            await Task.CompletedTask;
        }

        public async Task Undo()
        {
            Utils.Debug.Log($"<color=red>Error: Undo unavailable in {this}");
            await Task.CompletedTask;
        }
    }
}