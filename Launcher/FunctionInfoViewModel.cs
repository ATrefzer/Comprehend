using System.ComponentModel;
using System.Runtime.CompilerServices;

using Launcher.Profiler;

namespace Launcher
{
    internal class FunctionInfoViewModel : INotifyPropertyChanged
    {
        private readonly FunctionInfo _info;

        public FunctionInfoViewModel(FunctionInfo info)
        {
            _info = info;
            Included = true;
            Public = info.IsPublic;
            FullName = info.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Included
        {
            get => !_info.IsFiltered;
            set
            {
                _info.IsFiltered = !value;
                OnPropertyChanged();
            }
        }

        public string FullName { get; set; }

        public bool Public { get; set; }

        public FunctionInfo Model => _info;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}