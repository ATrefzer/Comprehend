using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

using Launcher.Execution;
using Launcher.Profiler;


using Prism.Commands;

using Process = System.Diagnostics.Process;

namespace Launcher
{
    interface IGenerator
    {
        Task ExecuteGenerate(FunctionInfo startFunction);
    }

    internal class MethodChooserViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        // TODO atr remove
        private readonly BackgroundExecutionService _backgroundService;
        private readonly string _workingDirectory;
      
       


        private FunctionInfo _startFunction;

        public MethodChooserViewModel(BackgroundExecutionService backgroundService, string workingDirectory, IGenerator generator)
        {
            _backgroundService = backgroundService;
            _workingDirectory = workingDirectory;
            GenerateCommand = new DelegateCommand(async () => await generator.ExecuteGenerate(StartFunction));
            SelectStartFunctionCommand = new DelegateCommand<FunctionInfoViewModel>(SelectStartFunction);
            SelectOnlyStartFunctionCommand = new DelegateCommand<FunctionInfoViewModel>(SelectOnlyStartFunction);
            IncludeCommand = new DelegateCommand<object>(Include);
            ExcludeCommand = new DelegateCommand<object>(Exclude);
            StartFunction = null;
        }

      

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ICommand ExcludeCommand { get; set; }

        public ICommand IncludeCommand { get; set; }

        public ICommand GenerateCommand { get; }

        /// <summary>
        /// References the FunctionsInfo objects in the model. So we can edit them directly.
        /// </summary>
        public ObservableCollection<FunctionInfoViewModel> AllPreFilteredFunctions { get; set; }

        public bool HasStartFunction { get; } = true;

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

        public bool CanRender => StartFunction != null;

        public ICommand SelectStartFunctionCommand { get; set; }
        public ICommand SelectOnlyStartFunctionCommand { get; set; }

        public bool HasErrors => StartFunction == null;


        public void Initialize(IEnumerable<FunctionInfoViewModel> preFiltered)
        {
            AllPreFilteredFunctions = new ObservableCollection<FunctionInfoViewModel>(preFiltered);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == nameof(StartFunction))
            {
                if (StartFunction == null)
                {
                    return new List<string> { "You have to select a function to generate a sequence diagram!" };
                }
            }

            return Enumerable.Empty<string>();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
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
                var vm = startFunction as FunctionInfoViewModel;
                StartFunction = vm.Model;
            }
        }
        private void SelectOnlyStartFunction(object startFunction)
        {
            if (startFunction != null)
            {
                var vm = startFunction as FunctionInfoViewModel;
                StartFunction = vm.Model;

                foreach (var func in AllPreFilteredFunctions)
                {
                    var included = func == startFunction;
                   func.Included = included;
                }
            }
        }


       
    }
}