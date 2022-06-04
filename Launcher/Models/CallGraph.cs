using System.Collections.Generic;
using System.Linq;
using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    ///     Pre-filtered, but contains some banned functions calls that later call visible functions.
    /// </summary>
    internal class CallGraph
    {
        /// <summary>
        ///     All functions collected during processing of the profile.
        /// </summary>
        private readonly Dictionary<ulong, GraphCall> _functions = new Dictionary<ulong, GraphCall>();


        private readonly Dictionary<ulong, Stack<GraphCall>> _tidToStack = new Dictionary<ulong, Stack<GraphCall>>();

        public CallGraph(List<GraphCall> model)
        {
            AllFunctions = model;
        }

        private CallGraph()
        {
        }

        public List<GraphCall> AllFunctions { get; private set; }

        public static CallGraph FromEventStream(IEnumerable<ProfilerEvent> stream)
        {
            var model = new CallGraph();
            model.FromEventStream_(stream);
            return model;
        }

        /// <summary>
        ///     We need to mark (keep) all ancestors as soon as we have any visible child in the graph.
        ///     parent -> hidden -> visible shall be shown in the graph. We just skip the hidden stuff.
        /// </summary>
        private static void MarkAncestorsIfVisible(GraphCall enteredFunc)
        {
            if (!enteredFunc.IsBanned)
            {
                var ancestors = enteredFunc.GetAllAncestors(true);
                foreach (var ancestor in ancestors)
                {
                    ancestor.HasVisibleChildren = true;
                }
            }
        }


        private static void CleanupHiddenCalls(Dictionary<ulong, GraphCall> allFunctions, GraphCall exitFunc)
        {
            if (CanRemove(exitFunc))
            {
                //Debug.WriteLine("Removing " + exitFunc.FullName);
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
            return exitFunc.IsBanned && exitFunc.HasVisibleChildren == false;
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
                            activeFunc.IsRecursive = true;
                        }

                        activeFunc.Children.Add(enterFunc);
                        enterFunc.Parents.Add(activeFunc);

                        // We cant remove the ancestors of enterFunc because they contain at least
                        // one visible child.
                        MarkAncestorsIfVisible(enterFunc);
                    }

                    // This is the new active function 
                    // Stack tracks recursive calls. But they do not appear in the model later.
                    stack.Push(enterFunc);
                }
                else if (entry.Token == Tokens.TokenLeave)
                {
                    var stack = GetOrCreateStackByThreadId(entry.ThreadId);
                    var activeFunc = GetActiveFunction(stack);

                    if (activeFunc != null && activeFunc.FullName == entry.Func.FullName)
                    {
                        stack.Pop();

                        // Reduce memory by cleaning up while we process the event stream.
                        // We remove all functions that are hidden and only call hidden functions!
                        CleanupHiddenCalls(_functions, activeFunc);
                    }
                }
                else if (entry.Token == Tokens.TokenTailCall)
                {
                    if (!_functions.TryGetValue(entry.Func.Id, out var newFunc))
                    {
                        newFunc = CreateFunctionCall(entry);
                        newFunc.TailCall = true;
                        _functions.Add(newFunc.Id, newFunc);
                    }

                    newFunc.TailCall = true;
                }
                else if (entry.Token == Tokens.TokenDestroyThread)
                {
                    _tidToStack.Remove(entry.ThreadId);

                    // TODO close all open methods(!)
                }
            }

            AllFunctions = _functions.Values.ToList();
            Clear();
        }


        protected GraphCall GetEnteredFunction(ProfilerEvent entry)
        {
            GraphCall enterFunc;
            if (!_functions.TryGetValue(entry.Func.Id, out enterFunc))
            {
                enterFunc = CreateFunctionCall(entry);
                _functions.Add(enterFunc.Id, enterFunc);
            }

            return enterFunc;
        }

        protected void Clear()
        {
            _tidToStack.Clear();
            _functions.Clear();
        }

        protected GraphCall GetActiveFunction(Stack<GraphCall> stack)
        {
            if (stack == null)
            {
                return null;
            }

            // Find active function
            GraphCall activeFunc = null;
            if (stack.Count > 0)
            {
                activeFunc = stack.Peek();
            }

            return activeFunc;
        }

        protected Stack<GraphCall> GetOrCreateStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            if (!_tidToStack.TryGetValue(threadId, out var stack))
            {
                stack = new Stack<GraphCall>();
                _tidToStack.Add(threadId, stack);
            }

            return stack;
        }

        protected Stack<GraphCall> FindStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            _tidToStack.TryGetValue(threadId, out var stack);
            return stack;
        }

        protected GraphCall CreateFunctionCall(ProfilerEvent entry)
        {
            GraphCall newFunc;
            newFunc = new GraphCall(entry.Func);
            return newFunc;
        }
    }
}