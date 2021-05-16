using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Launcher.Profiler;
using Prism.Commands;

namespace Launcher
{
    internal class FunctionPickerConfig
    {
        public FunctionPickerConfig()
        {
            ExecuteButtonText = "Ok";
            HasStartFunction = true;
        }

        public string Instructions { get; set; }

        public Func<FunctionPickerViewModel, Task> DoFunc { get; set; }

        public string ExecuteButtonText { get; set; }

        public bool HasStartFunction { get; set; }

        public bool ShowIncludeColumn { get; set; }
    }

    internal sealed class FunctionPickerViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private FunctionInfo _startFunction;

        /// <summary>
        ///     Performs an action on execute command.
        /// </summary>
        public FunctionPickerViewModel(FunctionPickerConfig config)
        {
            async void ExecuteMethod()
            {
                await config.DoFunc(this);
            }

            GenerateCommand = new DelegateCommand(ExecuteMethod);
            SelectStartFunctionCommand = new DelegateCommand<FunctionInfoViewModel>(SelectStartFunction);
            IncludeCommand = new DelegateCommand<object>(Include);
            ExcludeCommand = new DelegateCommand<object>(Exclude);
            StartFunction = null;

            Instructions = config.Instructions;
            ExecuteButtonText = config.ExecuteButtonText;
            HasStartFunction = config.HasStartFunction;
            ShowIncludeColumn = config.ShowIncludeColumn;
        }

        public bool ShowIncludeColumn { get; }

        public string ExecuteButtonText { get; }

        public ICommand ExcludeCommand { get; }

        public ICommand IncludeCommand { get; }

        public ICommand GenerateCommand { get; }

        public ICommand SelectStartFunctionCommand { get; }
        public ICommand SelectOnlyStartFunctionCommand { get; set; }

        /// <summary>
        ///     References the FunctionsInfo objects in the model. So we can edit them directly.
        /// </summary>
        public ObservableCollection<FunctionInfoViewModel> AllPreFilteredFunctions { get; set; }

        public bool HasStartFunction { get; }

        public FunctionInfo StartFunction
        {
            get => _startFunction;
            set
            {
                if (value == _startFunction)
                {
                    return;
                }

                _startFunction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRender));
                OnErrorsChanged(new DataErrorsChangedEventArgs(nameof(StartFunction)));
            }
        }

        public bool CanRender => !HasStartFunction || StartFunction != null;

        public bool ShowInstructions => Instructions != null;
        public string Instructions { get; }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;


        public bool HasErrors => StartFunction == null;


        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == nameof(StartFunction))
            {
                if (StartFunction == null && HasStartFunction)
                {
                    return new List<string> { "You have to select a function to generate a sequence diagram!" };
                }
            }

            return Enumerable.Empty<string>();
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public HashSet<ulong> GetIncludedFunctionIds()
        {
            return AllPreFilteredFunctions.Where(f => f.Included && !f.Info.IsBanned).Select(f => f.Info.Id).ToHashSet();
        }


        public void Initialize(IEnumerable<FunctionInfoViewModel> preFiltered)
        {
            AllPreFilteredFunctions = new ObservableCollection<FunctionInfoViewModel>(preFiltered);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnErrorsChanged(DataErrorsChangedEventArgs e)
        {
            ErrorsChanged?.Invoke(this, e);
        }

        private void Exclude(object param)
        {
            var exclude = param as IList;
            if (exclude == null)
            {
                return;
            }

            foreach (FunctionInfoViewModel func in exclude)
            {
                func.Included = false;
            }
        }

        private void Include(object param)
        {
            var include = param as IList;
            if (include == null)
            {
                return;
            }

            foreach (FunctionInfoViewModel func in include)
            {
                func.Included = true;
            }
        }

        private void SelectStartFunction(FunctionInfoViewModel startFunction)
        {
            if (startFunction != null)
            {
                var vm = startFunction;
                StartFunction = vm.Model;
            }
        }
    }
}