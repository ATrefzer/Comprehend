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
        public readonly FunctionInfo Info;

        public FunctionCall(FunctionInfo info)
        {
            Info = info;
        }

        public HashSet<FunctionCall> Children { get; } = new HashSet<FunctionCall>();
        public HashSet<FunctionCall> Parents { get; } = new HashSet<FunctionCall>();

        public bool IsRecursive { get; internal set; }
        public bool TailCall { get; internal set; }

        public bool IsFiltered => Info.IsFiltered;

        public ulong Id => Info.Id;

        public string FullName => Info.FullName;

        public bool HasVisibleChildren { get; set; } = false;
        public bool IsPublic => Info.IsPublic;

        public override bool Equals(object obj)
        {
            return Info.Id == ((FunctionCall) obj).Info.Id;
        }

        public override int GetHashCode()
        {
            return Info.Id.GetHashCode();
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


        public string TypeName => Info.TypeName;

        public string Function => Info.Function;

        public bool IsCtor => Info.IsCtor;
    }
   
}