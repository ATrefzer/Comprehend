using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    /// Adds further information to FunctionInfo derived from the stack trace like call graph and recursion
    /// </summary>
    [DebuggerDisplay("Func: {FullName} Hidden={IsFiltered}")]
    public class FunctionCall
    {
        private readonly FunctionInfo _info;

        public FunctionCall(FunctionInfo info)
        {
            _info = info;
        }

        public HashSet<FunctionCall> Children { get; } = new HashSet<FunctionCall>();
        public HashSet<FunctionCall> Parents { get; } = new HashSet<FunctionCall>();

        public bool IsRecursive { get; internal set; }
        public bool TailCall { get; internal set; }

        public bool IsFiltered => _info.IsFiltered;

        public ulong Id => _info.Id;

        public string FullName => _info.FullName;

        public bool HasVisibleChildren { get; set; } = false;
        public bool IsPublic => _info.IsPublic;

        public override bool Equals(object obj)
        {
            return _info.Id == ((FunctionCall) obj)._info.Id;
        }

        public override int GetHashCode()
        {
            return _info.Id.GetHashCode();
        }

        /// <summary>
        /// excludeAncestorsWithVisibleChildren is an performance optimization. Parents that are already marked as parents
        /// of visible children don't need to be processed any further.
        /// </summary>
        /// <returns></returns>
        public List<FunctionCall> GetAncestorChain(bool excludeAncestorsWithVisibleChildren = false)
        {
            // Has to be empty by default otherwise it stops on first parent!
            var allAncestors = new HashSet<FunctionCall>();

            // Start with direct parents
            var toProcess = new Queue<FunctionCall>(Parents);

            while (toProcess.Any())
            {
                var parent = toProcess.Dequeue();

                bool addParent = !excludeAncestorsWithVisibleChildren || !parent.HasVisibleChildren;

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


        public string TypeName => _info.TypeName;

        public string Function => _info.Function;

        public bool IsCtor => _info.IsCtor;
    }
   
}