using System.Windows;

using Launcher.Profiler;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_RunTests(object sender, RoutedEventArgs e)
        {
            ProfilerExports.RunTests();
        }
    }
}