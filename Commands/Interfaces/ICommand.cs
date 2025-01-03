namespace DiscordGamePlayer.Commands.Interfaces
{
    internal interface ICommand
    {
        // void Execute();
        Task ExecuteAsync();
        Task Undo();
        Task Redo();
    }
}