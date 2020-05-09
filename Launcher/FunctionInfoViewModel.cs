using System.ComponentModel;
using System.Runtime.CompilerServices;

using Launcher.Profiler;

namespace Launcher
{
    internal class FunctionInfoViewModel : INotifyPropertyChanged
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

        public bool Included
        {
            get => !Info.IsFiltered;
            set
            {
                Info.IsFiltered = !value;
                OnPropertyChanged();
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}