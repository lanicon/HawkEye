namespace HawkEye.Commands
{
    internal class CommandExit : ICommand
    {
        public string Name => "Exit";

        public string[] Alias => new string[] { "Quit", "Stop", "Bye" };

        public string Description => "Exits HawkEye";

        public void OnTrigger(string[] args) => Program.Stop();
    }
}