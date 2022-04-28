using System.Linq;
using System.Windows;
using Launcher.Models;

namespace Launcher
{
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