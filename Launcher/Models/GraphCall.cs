using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    ///     Adds further information to FunctionInfo derived from the stack trace like the called children
    ///     For a call graph there is a 1:1 relation between a FunctionInfo and a FunctionCall object.
    ///     This class also stores user information like the IsIncluded flag.
    /// </summary>
    [DebuggerDisplay("Func: {FullName} Banned={IsBanned}")]
    public class GraphCall : FunctionCall
    {
        public GraphCall(FunctionInfo info) : base(info)
        {
        }

        public HashSet<GraphCall> Children { get; } = new HashSet<GraphCall>();
        public HashSet<GraphCall> Parents { get; } = new HashSet<GraphCall>();


        // TODO move to graph algo
        /// <summary>
        ///     excludeAncestorsWithVisibleChildren is an performance optimization. Parents that are already marked as parents
        ///     of visible children don't need to be processed any further.
        /// </summary>
        public List<GraphCall> GetAllAncestors(bool excludeAncestorsWithVisibleChildren = false)
        {
            // Has to be empty by default otherwise it stops on first parent!
            var allAncestors = new HashSet<GraphCall>();

            // Start with direct parents
            var toProcess = new Queue<GraphCall>(Parents);

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
    }
}