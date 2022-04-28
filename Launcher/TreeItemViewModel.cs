using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Launcher
{
    public class TreeItemViewModel : INotifyPropertyChanged
    {
        private readonly TreeItemViewModel _parent;
        private ObservableCollection<TreeItemViewModel> _children = new ObservableCollection<TreeItemViewModel>();
        private bool _isExpanded;
        private bool _isHighlighted;

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

        private bool _isVisible = true;

        public bool IsVisible
        {
            get => _isVisible;

            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }

                // Expand all the way up to the root.
                if (_isVisible && _parent != null)
                {
                    _parent.IsVisible = true;
                }
            }
        }

        public bool HasPlaceholder => _children.Count == 1 && _children[0] is PlaceHolderTreeViewItem;

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (value != _isHighlighted)
                {
                    _isHighlighted = value;
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
}