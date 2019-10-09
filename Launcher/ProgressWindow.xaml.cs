using System;
using System.Windows;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window, IParserProgress
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void Progress(int percent, int numEvent)
        {
            Dispatcher?.BeginInvoke(new Action(() =>
                                               {
                                                   //System.Diagnostics.Trace.WriteLine("Percent = " + currentProgress + ", Event Nr: " + numEvent);
                                                   _progressBar.Value = percent;
                                               }));
        }
    }
}