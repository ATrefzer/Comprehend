namespace Launcher.Execution
{
    internal interface IProgress
    {
        void Progress(string message, int percent);
    }
}