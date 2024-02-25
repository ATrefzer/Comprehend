using System.Collections.Generic;
using System.Linq;

using GraphFormats;


namespace Launcher.Models
{
    /// <summary>
    /// Shows the selected functions and compacts all hidden calls in between.
    /// 
    /// Note:
    /// The pre-filter was applied already when processing the profiler events.
    /// When a hidden function has only hidden children it was removed immediately.
    /// </summary>
    internal class CallGraphExport
    {
        private readonly HashSet<(ulong, ulong)> _processed = new HashSet<(ulong, ulong)>();

        /// <summary>
        /// Selected by the user in the function picker dialog.
        /// </summary>
        private HashSet<ulong> _included;

        internal void Export(CallGraph model, HashSet<ulong> included, IGraphBuilder builder)
        {
            _included = included;
            _processed.Clear();
            builder.AddCategory("indirect", "StrokeDashArray", "1 1");
            Build(builder, model);
        }


        private bool IsIncluded(GraphCall call)
        {
            return _included.Contains(call.Id);
        }

        /// <summary>
        /// Algorithm.
        /// Start with the selected functions. (Recursively) iterate all children. Regardless
        /// if hidden or not. But we remember the last visible ancestor when we walk down the
        /// call graph. We only draw edges from the last visible ancestor to visible functions.
        /// </summary>
        private void Build(IGraphBuilder builder, CallGraph model)
        {
            _processed.Clear();

            // User visible functions
            var selection = new HashSet<GraphCall>();
            var starting = model.AllFunctions.Where(IsIncluded).ToList();
            selection.UnionWith(starting);

            //Include children
            foreach (var func in selection)
            {
                Build(builder, null, func);
            }
        }
       
        private void Build(IGraphBuilder builder, GraphCall lastVisibleAncestor, GraphCall target)
        {
            // Assumption: We start with the first visible parent. Anything hidden above is ignored.

            var toProcess = new Queue<(GraphCall, GraphCall)>();
            toProcess.Enqueue((lastVisibleAncestor, target));

            while (toProcess.Any())
            {
                (lastVisibleAncestor, target) = toProcess.Dequeue();

                // Avoid processing the same link twice. We have to use the link to decide, not the nodes(!)
                if (lastVisibleAncestor != null)
                {
                    var link = (lastVisibleAncestor.Id, target.Id);
                    if (_processed.Contains(link))
                    {
                        continue;
                    }

                    _processed.Add(link);
                }

                if (lastVisibleAncestor == null && IsIncluded(target))
                {
                    // A single visible node.
                    builder.AddNode(target.FullName);
                }         

                if (lastVisibleAncestor != null && IsIncluded(target))
                {
                    if (lastVisibleAncestor.Children.Contains(target))
                    {
                        // Direct call
                        builder.AddEdge(lastVisibleAncestor.FullName, target.FullName);
                    }
                    else
                    {
                        // Indirect call (mark as dashed line)
                        builder.AddEdge(lastVisibleAncestor.FullName, target.FullName, "indirect");
                    }
                }

                if (IsIncluded(target))
                {
                    // New visible parent for the children
                    lastVisibleAncestor = target;
                }

                foreach (var call in target.Children)
                {
                    toProcess.Enqueue((lastVisibleAncestor, call));
                }
            }
        }
    }
}