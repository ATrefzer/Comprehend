using System.Linq;
using Launcher.Models;

namespace Launcher
{
    /// <summary>
    ///     View model for a tree item in the call tree explorer
    /// </summary>
    public class TreeCallViewModel : TreeItemViewModel
    {
        private readonly SpecialNode _specialNode;

        /// <summary>
        ///     Root entry
        /// </summary>
        public TreeCallViewModel(TreeCall call) : base(null, call.Children.Any())
        {
            Call = call;
            _specialNode = SpecialNode.None;
        }

        private TreeCallViewModel(TreeCall call, TreeCallViewModel parent, SpecialNode specialNode) : base(parent,
            specialNode == SpecialNode.None && call.Children.Any())
        {
            Call = call;
            _specialNode = specialNode;
        }

        public TreeCall Call { get; }

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
                Children.Add(new TreeCallViewModel(child, this, SpecialNode.None));
            }
        }

        private enum SpecialNode
        {
            None,
            Recursion
        }
    }
}