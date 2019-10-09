using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

using Launcher.Profiler;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using Prism.Commands;

namespace Launcher
{
    internal class TracingViewModel : INotifyPropertyChanged
    {
        private string _target;

        private bool _isRunning;


        private string _outputDirectory;

        public TracingViewModel()
        {
            SelectOutputDirectoryCommand = new DelegateCommand(ExecuteSelectOutputDirectory);
            SelectTargetCommand = new DelegateCommand(ExecuteSelectTarget);
            RunTargetCommand = new DelegateCommand(ExecuteRunTarget);
        }

        public event EventHandler<TracesArg> TraceSourceChanged;


        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SelectOutputDirectoryCommand { get; set; }
        public ICommand SelectTargetCommand { get; set; }

        public ICommand RunTargetCommand { get; set; }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Target
        {
            get => _target;
            set
            {
                _target = value;
                OnPropertyChanged();
            }
        }

        public string OutputDirectory
        {
            get => _outputDirectory;
            set
            {
                _outputDirectory = value;
                OnPropertyChanged();
                TraceSourceChanged?.Invoke(this, new TracesArg { Path = OutputDirectory });
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private async void ExecuteRunTarget()
        {
            IsRunning = true;
            if (File.Exists(Target) && Directory.Exists(OutputDirectory))
            {
                await StartAsync();
            }

            IsRunning = false;
        }

        private async Task StartAsync()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            await Process.StartAsync(Target, directory, OutputDirectory);

            // Update trace list
            TraceSourceChanged?.Invoke(this, new TracesArg { Path = OutputDirectory });
        }

        private void ExecuteSelectTarget()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Executables (*.exe)|*.exe";
            if (dialog.ShowDialog() == true)
            {
                Target = dialog.FileName;
            }
        }

        private void ExecuteSelectOutputDirectory()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                OutputDirectory = dialog.FileName;
            }
        }
    }
}