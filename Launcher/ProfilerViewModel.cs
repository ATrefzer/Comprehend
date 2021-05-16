using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Launcher.Profiler;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using Prism.Commands;

namespace Launcher
{
    internal class ProfilerViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly EventWaitHandle _recordingStateChanged;
        private readonly EventWaitHandle _recordingState;
        private string _target;

        private bool _isRunning;
        private string _outputDirectory;

        private bool _isProfilingEnabled;

        public ProfilerViewModel()
        {
            SelectOutputDirectoryCommand = new DelegateCommand(ExecuteSelectOutputDirectory);
            SelectTargetCommand = new DelegateCommand(ExecuteSelectTarget);
            RunTargetCommand = new DelegateCommand(ExecuteRunTarget);

            _recordingStateChanged = new EventWaitHandle(false, EventResetMode.AutoReset, "MiniProfiler_RecordingStateChanged_Event");
            _recordingState = new EventWaitHandle(false, EventResetMode.ManualReset, "MiniProfiler_RecordingState_Event");

            _isProfilingEnabled = true;
            SetProfilingEnabled(_isProfilingEnabled, false); // Avoid controller thread to wake up initially
        }

        public event EventHandler<TracesArg> AvailableTracesChanged;


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
                AvailableTracesChanged?.Invoke(this, new TracesArg { Path = OutputDirectory });
            }
        }

        public bool IsProfilingEnabled
        {
            get => _isProfilingEnabled;
            set
            {
                SetProfilingEnabled(value, true);

                OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            _recordingStateChanged?.Dispose();
            _recordingState?.Dispose();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetProfilingEnabled(bool value, bool notify)
        {
            _isProfilingEnabled = value;
            if (value)
            {
                // Manual
                _recordingState.Set();
            }
            else
            {
                _recordingState.Reset();
            }

            if (notify)
            {
                // Auto reset
                _recordingStateChanged.Set();
            }
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
            AvailableTracesChanged?.Invoke(this, new TracesArg { Path = OutputDirectory });
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