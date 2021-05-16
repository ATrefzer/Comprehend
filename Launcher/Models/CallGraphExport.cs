using System.Collections.Generic;
using System.Linq;

using GraphFormats;


namespace Launcher.Models
{
    /// <summary>
    /// Selects the entry function and compacts all hidden calls.
    /// 
    /// Note:
    /// The filter was applied already when processing the profiler events.
    /// When a hidden function has only hidden children it was removed immediately.
    /// </summary>
    internal class CallGraphExport
    {
        private readonly HashSet<(ulong, ulong)> _processed = new HashSet<(ulong, ulong)>();
        private HashSet<ulong> _included;

        internal void Export(CallGraph model, HashSet<ulong> included, IGraphBuilder builder)
        {
            _included = included;
            _processed.Clear();
            builder.AddCategory("indirect", "StrokeDashArray", "1 1");
            Build(builder, model);
        }


        private bool IsIncluded(FunctionCall call)
        {
            return _included.Contains(call.Id);
        }

        private void Build(IGraphBuilder builder, CallGraph model)
        {
            _processed.Clear();

            // User visible functions
            var selection = new HashSet<FunctionCall>();
            var starting = model.AllFunctions.Where(IsIncluded).ToList();
            selection.UnionWith(starting);

            // Visible parents are already in the list of included functions
            // Include (visible) parents
            //foreach (var func in starting)
            //{
            //    selection.UnionWith(func.GetAncestorChain().Where(f => IsIncluded(f)));
            //}

            //Include children
            foreach (var func in selection)
            {
                Build(builder, null, func);
            }
        }

        /// <summary>
        /// Algorithm.
        /// Start with all visible functions. (Recursively) iterate all children. Regardless
        /// if hidden or not. But we remember the last visible ancestor when we walk down the
        /// call tree. We only draw edges from the last visible ancestor to visible functions.
        /// </summary>
        private void Build(IGraphBuilder builder, FunctionCall lastVisibleAncestor, FunctionCall target)
        {
            // Assumption: We start with the first visible parent. Anything hidden above is ignored.

            var toProcess = new Queue<(FunctionCall, FunctionCall)>();
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

                // Display recursion
                //if (lastVisibleAncestor != null && lastVisibleAncestor.Recursive)
                //{
                //    builder.AddEdge(lastVisibleAncestor.Name, lastVisibleAncestor.Name);
                //}

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