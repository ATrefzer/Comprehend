using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphFormats.PlantUml;
using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    ///     Adds further information to FunctionInfo derived from the stack trace like the called children
    ///     Note: For a call tree each time a function is called it gets a new FunctionCall instance.
    ///     For a call graph there is a 1:1 relation between a FunctionInfo and a FunctionCall object.
    ///     This class also stores user information like the IsIncluded flag.
    /// </summary>
    [DebuggerDisplay("Func: {FullName} Banned={IsBanned}")]
    public class FunctionCall : IFunctionPresentation
    {
        private static readonly Actor _actor;
        public readonly FunctionInfo Info;
        private bool _isIncluded;

        static FunctionCall()
        {
            _actor = new Actor();
        }

        public FunctionCall(FunctionInfo info)
        {
            Info = info;

            // By default the pre-filtered functions are not included.
            IsIncluded = !info.IsBanned;
        }

        public HashSet<FunctionCall> Children { get; } = new HashSet<FunctionCall>();
        public HashSet<FunctionCall> Parents { get; } = new HashSet<FunctionCall>();

        public bool IsRecursive { get; internal set; }
        public bool TailCall { get; internal set; }

        public bool IsBanned => Info.IsBanned;

        public ulong Id => Info.Id;

        public string FullName => Info.FullName;

        public bool HasVisibleChildren { get; set; } = false;
        public bool IsPublic => Info.IsPublic;

        public bool IsCtor => Info.IsCtor;

        /// <summary>
        ///     Custom filtering.
        ///     For sequence diagrams. Allow including banned functions!
        /// </summary>
        public bool IsIncluded
        {
            get => _isIncluded;
            set => _isIncluded = value;
        }

        public string TypeName => Info.TypeName;

        public string Function => Info.Function;

        public override bool Equals(object obj)
        {
            return Info.Id == ((FunctionCall)obj).Info.Id;
        }

        public override int GetHashCode()
        {
            return Info.Id.GetHashCode();
        }

        /// <summary>
        ///     excludeAncestorsWithVisibleChildren is an performance optimization. Parents that are already marked as parents
        ///     of visible children don't need to be processed any further.
        /// </summary>
        public List<FunctionCall> GetAncestorChain(bool excludeAncestorsWithVisibleChildren = false)
        {
            // Has to be empty by default otherwise it stops on first parent!
            var allAncestors = new HashSet<FunctionCall>();

            // Start with direct parents
            var toProcess = new Queue<FunctionCall>(Parents);

            while (toProcess.Any())
            {
                var parent = toProcess.Dequeue();

                var addParent = !excludeAncestorsWithVisibleChildren || !parent.HasVisibleChildren;

                if (addParent && allAncestors.Add(parent))
                {
                    foreach (var ancestor in parent.Parents)
                    {
                        if (!allAncestors.Contains(ancestor))
                        {
                            toProcess.Enqueue(ancestor);
                        }
                    }
                }
            }

            return allAncestors.ToList();
        }

        public static FunctionCall GetActor()
        {
            return _actor;
        }

        private class Actor : FunctionCall
        {
            public Actor() : base(new FunctionInfo(0, "Actor.Caller", true, false))
            {
                IsIncluded = true;
            }
        }
    }
}