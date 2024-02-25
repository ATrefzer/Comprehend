using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Launcher.Models;
using Prism.Commands;

namespace Launcher
{
    public sealed class CallTreeExplorerViewModel : INotifyPropertyChanged
    {
        private List<TreeCallViewModel> _all;

        public CallTreeExplorerViewModel()
        {
            SearchCommand = new DelegateCommand<string>(DoSearch);
        }

        public ObservableCollection<TreeCallViewModel> Roots { get; } =
            new ObservableCollection<TreeCallViewModel>();

        public ICommand SearchCommand { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        private void Flatten(TreeCallViewModel vm, ICollection<TreeCallViewModel> result)
        {
            vm.Load();
            result.Add(vm);
            foreach (var child in vm.Children)
            {
                if (child is TreeCallViewModel childVm)
                {
                    Flatten(childVm, result);
                }
            }
        }

        private void DoSearch(string text)
        {
            var searchFor = text.ToUpperInvariant();
            if (_all == null)
            {
                _all = new List<TreeCallViewModel>();
                foreach (var root in Roots)
                {
                    Flatten(root, _all);
                }
            }


            if (string.IsNullOrEmpty(text.Trim()))
            {
                // Make all items visible again and remove the highlighting
                foreach (var item in _all)
                {
                    item.IsVisible = true;
                    item.IsHighlighted = false;
                }

                return;
            }

            if (text == "!")
            {
                // Make all items visible again but keep the highlighting
                foreach (var item in _all)
                {
                    item.IsVisible = true;
                }

                return;
            }


            foreach (var item in _all)
            {
                item.IsVisible = false;
            }


            for (var index = 0; index < _all.Count; index++)
            {
                if (!_all[index].Name.ToUpperInvariant().Contains(searchFor))
                {
                    continue;
                }

                // Note: Setting a node expanded or visible will also set all parents
                // expanded or visible.
                _all[index].IsExpanded = true;
                _all[index].IsVisible = true;
                _all[index].IsHighlighted = true;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RemoveBannedBranches()
        {
            // Rebuild the tree
            var tmp = new List<TreeCall>();
            foreach (var root in Roots)
            {
                var simplifiedClone = root.Call.Clone(true);
                tmp.Add(simplifiedClone);
            }

            // Add roots to explorer
            Roots.Clear();
            foreach (var child in tmp)
            {
                Roots.Add(new TreeCallViewModel(child));
            }
        }

        public void Unfold(TreeCallViewModel vm)
        {
            vm.Load();
            foreach (var child in vm.Children)
            {
                Unfold((TreeCallViewModel)child);
            }

            vm.IsExpanded = true;
        }
    }
}