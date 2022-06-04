using System;

namespace Launcher.Execution
{
    /// <summary>
    ///     Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : IProgress
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void Progress(string message, int percent)
        {
            Dispatcher?.BeginInvoke(new Action(() => { _progressBar.Value = percent; }));
        }
    }
}