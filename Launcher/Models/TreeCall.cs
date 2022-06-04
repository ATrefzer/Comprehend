﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    ///     Adds further information to FunctionInfo derived from the stack trace like the called children
    ///     For a call tree each time a function is called it gets a new FunctionCall instance.///
    ///     This class also stores user information like the IsIncluded flag.
    /// </summary>
    [DebuggerDisplay("Func: {FullName} Banned={IsBanned}")]
    public class TreeCall : FunctionCall
    {
        private static readonly Actor _actor;

        static TreeCall()
        {
            _actor = new Actor();
        }

        public TreeCall(FunctionInfo info) : base(info)
        {
        }

        public List<TreeCall> Children { get; } = new List<TreeCall>();


        public static TreeCall GetActor()
        {
            return _actor;
        }

        public TreeCall Clone(bool removeBannedBranches)
        {
            var clone = new TreeCall(Info);
            Clone(this, clone);
            if (removeBannedBranches)
            {
                RemoveBannedBranches(clone);
            }

            return clone;
        }

        private void Clone(TreeCall original, TreeCall clone)
        {
            foreach (var child in original.Children)
            {
                var newChild = new TreeCall(child.Info);
                clone.Children.Add(newChild);
                Clone(child, newChild);
            }
        }

        private void RemoveBannedBranches(TreeCall root)
        {
            foreach (var child in root.Children)
            {
                RemoveBannedBranches(child);
            }

            // Remove all banned children without further children
            // If root is banned itself and all children are removed we get rid of it in the next level.
            root.Children.RemoveAll(call => call.IsBanned && call.Children.Any() is false);
        }

        private class Actor : TreeCall
        {
            public Actor() : base(new FunctionInfo(0, "Actor.Caller", true, false))
            {
                IsIncluded = true;
            }
        }
    }
}