using System;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher.Execution
{
    internal sealed class BackgroundExecutionService
    {
        private readonly Window _wnd;

        public BackgroundExecutionService(Window wnd)
        {
            _wnd = wnd;
        }

        public async Task RunWithProgress(Action<IProgress> action)
        {
            var progressWindow = new ProgressWindow();
            progressWindow.Owner = _wnd;
            progressWindow._progressBar.Value = 0;

            var t1 = Task.Run(() => action(progressWindow));
            var t2 = t1.ContinueWith(task => CloseProgressWindow(progressWindow));

            progressWindow.ShowDialog();

            // Task is completed here (RanToCompletion). But unpack stored exception.
            await t1;

            // Continuation (Ensure no exception is lost)
            await t2;
        }

        private static void CloseProgressWindow(ProgressWindow progressWindow)
        {
            progressWindow.Dispatcher?.Invoke(() =>
                                              {
                                                  progressWindow._progressBar.Value = 100;
                                                  progressWindow.Close();
                                              });
        }
    }
}