using System;
using System.Windows;

namespace Launcher.Execution
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window, IProgress
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void Progress(string message, int percent)
        {
            Dispatcher?.BeginInvoke(new Action(() =>
                                               {
                                                   // TODO write text
                                                   _progressBar.Value = percent;
                                               }));
        }
    }
}