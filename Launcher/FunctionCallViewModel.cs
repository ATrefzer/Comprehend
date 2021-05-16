using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Launcher.Models;

namespace Launcher
{
    public class CallTreeExplorerViewModel
    {
        public ObservableCollection<FunctionCallViewModel> Roots { get; } =
            new ObservableCollection<FunctionCallViewModel>();
    }

    internal class PlaceHolderTreeViewItem : TreeItemViewModel
    {
        public PlaceHolderTreeViewItem(TreeItemViewModel parent) : base(parent, false)
        {
        }
    }

    public class TreeItemViewModel : INotifyPropertyChanged
    {
        private readonly TreeItemViewModel _parent;
        private ObservableCollection<TreeItemViewModel> _children = new ObservableCollection<TreeItemViewModel>();
        private bool _isExpanded;
        private bool _isSelected;

        public TreeItemViewModel(TreeItemViewModel parent, bool loadLazy)
        {
            _parent = parent;


            if (loadLazy)
            {
                AddPlaceHolder();
            }
        }


        public ObservableCollection<TreeItemViewModel> Children
        {
            get => _children;
            set
            {
                _children = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                {
                    _parent.IsExpanded = true;
                }

                Load();
            }
        }

        public bool HasPlaceholder => _children.Count == 1 && _children[0] is PlaceHolderTreeViewItem;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Load()
        {
            // Lazy load the child items, if necessary.
            if (HasPlaceholder)
            {
                Children.RemoveAt(0);
                LoadChildren();
            }
        }

        private void AddPlaceHolder()
        {
            _children.Add(new PlaceHolderTreeViewItem(this));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void LoadChildren()
        {
        }
    }

    /// <summary>
    ///     View model for a tree item in the call tree explorer
    /// </summary>
    public class FunctionCallViewModel : TreeItemViewModel
    {
        private readonly SpecialNode _specialNode;

        /// <summary>
        ///     Root entry
        /// </summary>
        public FunctionCallViewModel(FunctionCall call) : base(null, call.Children.Any())
        {
            Call = call;
            _specialNode = SpecialNode.None;
        }

        private FunctionCallViewModel(FunctionCall call, FunctionCallViewModel parent, SpecialNode specialNode) : base(parent,
            specialNode == SpecialNode.None && call.Children.Any())
        {
            Call = call;
            _specialNode = specialNode;
        }

        public FunctionCall Call { get; }

        public string Name => Call.FullName;

        public bool IsRecursion => _specialNode == SpecialNode.Recursion;

        public bool IsIncluded
        {
            get => Call.IsIncluded;

            set
            {
                if (Call.IsIncluded != value)
                {
                    Call.IsIncluded = value;
                    OnPropertyChanged();
                }
            }
        }


        protected override void LoadChildren()
        {
            if (_specialNode != SpecialNode.None)
            {
                return;
            }

            foreach (var child in Call.Children)
            {
                Children.Add(new FunctionCallViewModel(child, this, SpecialNode.None));
            }
        }

        private enum SpecialNode
        {
            None,
            Recursion
        }
    }
}