namespace HawkEye.Commands
{
    public interface ICommand
    {
        string Name { get; }
        string[] Alias { get; }
        string Description { get; }

        void OnTrigger(string[] args);
    }
}