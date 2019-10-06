using GraphLibrary.Dgml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Models
{
    // TODO inject export format.
    class CallGraphExporter
    {
        private readonly HashSet<(ulong, ulong)> _processed = new HashSet<(ulong, ulong)>();

        internal void Export(CallGraphModel model, string path)
        {
            var builder = new DgmlFileBuilder();
            builder.AddCategory("indirect", "StrokeDashArray", "1 1");
            Build(builder, model);

            builder.WriteOutput(path);
        }

        private void Build(DgmlFileBuilder builder, CallGraphModel model)
        {
            _processed.Clear();
            foreach (var func in model.AllFunctions.Where(f => !f.IsHidden))
            {
                // Start with all visible functions and add them to the graph
                Build(builder, null, func);
            }
        }

        /// <summary>
        /// Algorithm.
        /// Start with all visible functions. (Recursively) iterate all children. Regardless
        /// if hidden or not. But we remember the last visible ancestor when we walk down the 
        /// call tree. We only draw edges from the last visible ancestor to visible functions.
        /// </summary>
        private void Build(DgmlFileBuilder builder, FunctionCall lastVisibleAncestor, FunctionCall target)
        {
            // Assumption: We start with the first visible parent. Anything hidden above is ignored.

            var toProcess = new Queue<(FunctionCall, FunctionCall)>();
            toProcess.Enqueue((lastVisibleAncestor, target));

            while (toProcess.Any())
            {
                (lastVisibleAncestor, target) = toProcess.Dequeue();

                // Avoid processsing the same link twice. We have to use the link to decide, not the nodes(!)
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
                if (lastVisibleAncestor != null && lastVisibleAncestor.Recursive)
                {
                    builder.AddEdge(lastVisibleAncestor.Name, lastVisibleAncestor.Name);
                }

                if (lastVisibleAncestor != null && !target.IsHidden)
                {
                    if (lastVisibleAncestor.Children.Contains(target))
                    {
                        // Direct call
                        builder.AddEdge(lastVisibleAncestor.Name, target.Name);
                    }
                    else
                    {
                        // Indirect call (mark as dashed line)
                        builder.AddEdge(lastVisibleAncestor.Name, target.Name, "indirect");
                    }
                }

                if (!target.IsHidden)
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
