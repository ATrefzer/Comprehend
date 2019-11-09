using System.Collections.Generic;
using System.Linq;

using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    /// Pre-filtered, but contains all hidden calls that contain visible targets.
    /// </summary>
    internal class CallGraphModel : BaseModel
    {
        public CallGraphModel(List<FunctionCall> model)
        {
            AllFunctions = model;
        }

        private CallGraphModel()
        {
        }

        public List<FunctionCall> AllFunctions { get; private set; }

        public static CallGraphModel FromEventStream(IEnumerable<ProfilerEvent> stream)
        {
            var model = new CallGraphModel();
            model.FromEventStream_(stream);
            return model;
        }


        private static void MarkAllAncestorOfVisibleChild(FunctionCall activeFunc)
        {
            var ancestors = activeFunc.GetAncestorChain();
            foreach (var ancestor in ancestors)
            {
                ancestor.HasVisibleChildren = true;
            }

            //var processedParent = new HashSet<ulong>();

            //// Mark all parents that the have at least one visible child
            //var parents = new Queue<FunctionCall>();
            //parents.Enqueue(activeFunc);
            //while (parents.Any())
            //{
            //    var parent = parents.Dequeue();
            //    processedParent.Add(parent.Id);
            //    if (parent.HasVisibleChildren == false)
            //    {
            //        parent.HasVisibleChildren = true;

            //        foreach (var ancestor in parent.Parents)
            //        {
            //            if (!processedParent.Contains(ancestor.Id))
            //            {
            //                parents.Enqueue(ancestor);
            //            }
            //        }
            //    }
            //}
        }

        private static void CleanupHiddenCalls(Dictionary<ulong, FunctionCall> allFunctions, FunctionCall exitFunc)
        {
            if (CanRemove(exitFunc))
            {
                allFunctions.Remove(exitFunc.Id);

                // Cleanup all calls to this function. There is nothing worth down there.
                foreach (var parent in exitFunc.Parents)
                {
                    parent.Children.Remove(exitFunc);
                }

                exitFunc.Parents.Clear();
            }
        }

        private static bool CanRemove(FunctionCall exitFunc)
        {
            return exitFunc.IsFiltered && exitFunc.HasVisibleChildren == false;
        }

        private void FromEventStream_(IEnumerable<ProfilerEvent> stream)
        {
            Clear();

            foreach (var entry in stream)
            {
                if (entry.Token == Tokens.TokenEnter)
                {
                    // 1. Mark as child of the active function
                    // 2. Push as active function

                    var stack = GetOrCreateStackByThreadId(entry.ThreadId);
                    var enterFunc = GetEnteredFunction(entry);
                    var activeFunc = GetActiveFunction(stack);

                    if (activeFunc != null)
                    {
                        if (ReferenceEquals(activeFunc, enterFunc))
                        {
                            activeFunc.Recursive = true;
                        }

                        activeFunc.Children.Add(enterFunc);
                        enterFunc.Parents.Add(activeFunc);

                        if (!enterFunc.IsFiltered)
                        {
                            // We cant remove the ancestors of enterFunc because they contain at least
                            // one visible child.
                            MarkAllAncestorOfVisibleChild(activeFunc);
                        }
                    }

                    // This is the new active function 
                    // Stack tracks recursive calls. But they do not appear in the model later.
                    stack.Push(enterFunc);
                }
                else if (entry.Token == Tokens.TokenLeave)
                {
                    var stack = GetOrCreateStackByThreadId(entry.ThreadId);
                    var activeFunc = GetActiveFunction(stack);

                    if (activeFunc != null && activeFunc.Name == entry.Func.FullName)
                    {
                        stack.Pop();

                        // Reduce memory by cleaning up while we process the event stream.
                        // We remove all functions that are hidden and only call hidden functions!
                        CleanupHiddenCalls(Functions, activeFunc);
                    }
                    else
                    {
                        // Ignore. We did not start recording at the time.
                    }
                }
                else if (entry.Token == Tokens.TokenTailCall)
                {
                    if (!Functions.TryGetValue(entry.Func.Id, out var newFunc))
                    {
                        newFunc = CreateFunctionCall(entry);
                        newFunc.TailCall = true;
                        Functions.Add(newFunc.Id, newFunc);
                    }

                    newFunc.TailCall = true;
                }
                else if (entry.Token == Tokens.TokenDestroyThread)
                {
                    TidToStack.Remove(entry.ThreadId);

                    // TODO close all open methods(!)
                }
            }

            AllFunctions = Functions.Values.ToList();
            Clear();
        }
    }
}