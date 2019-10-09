namespace Launcher.Execution
{
    interface IProgress
    {
        void Progress(string message, int percent);
    }
}