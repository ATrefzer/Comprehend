using System.ComponentModel;
using System.Runtime.CompilerServices;

using Launcher.Profiler;

namespace Launcher
{
    internal sealed class FunctionInfoViewModel : INotifyPropertyChanged
    {
        public readonly FunctionInfo Info;
        private bool _hidden = false;

        public FunctionInfoViewModel(FunctionInfo info)
        {
            Info = info;
            Included = true;
            Public = info.IsPublic;
            FullName = info.FullName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isIncluded = true;
        public bool Included
        {
            get => _isIncluded;
            set
            {
                if ( value != _isIncluded)
                {
                    _isIncluded = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Function is not reachable or contained in the model.
        /// </summary>
        public bool Hidden
        {
            get => _hidden;
            set
            {
                _hidden = value;
                OnPropertyChanged();
            }
        }

        public string FullName { get; set; }

        public bool Public { get; set; }

        public FunctionInfo Model => Info;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}