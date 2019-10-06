using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Launcher.Models
{
    internal class CallGraphModel
    {
        private CallGraphModel(List<FunctionCall> model)
        {
            AllFunctions = model;
        }

        public List<FunctionCall> AllFunctions { get; }

        private static Dictionary<ulong, Stack<FunctionCall>> _tidToStack = new Dictionary<ulong, Stack<FunctionCall>>();
        private static Dictionary<ulong, FunctionCall> _functions = new Dictionary<ulong, FunctionCall>();
        private static Filter _filter = Filter.Default();
        static FunctionCall GetActiveFunction(Stack<FunctionCall> stack)
        {          
            // Find active function
            FunctionCall activeFunc = null;
            if (stack.Count > 0)
            {
                activeFunc = stack.Peek();
            }

            return activeFunc;

        }

        private static Stack<FunctionCall> GetStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            if (!_tidToStack.TryGetValue(threadId, out var stack))
            {
                stack = new Stack<FunctionCall>();
                _tidToStack.Add(threadId, stack);
            }

            return stack;
        }

        private static FunctionCall GetEnteredFunction(ProfilerEvent entry)
        {
            FunctionCall enterFunc = null;
            if (!_functions.TryGetValue(entry.FunctioId, out enterFunc))
            {
                enterFunc = CreateFunctionCall(entry, _filter);
                _functions.Add(enterFunc.Id, enterFunc);
            }
            return enterFunc;
        }


        public static CallGraphModel FromEventStream(IEnumerable<ProfilerEvent> stream, Filter filter)
        {
            _filter = filter;
            _tidToStack.Clear();
            _functions.Clear();

            foreach (var entry in stream)
            {
                if (entry.Token == Tokens.TokenEnter)
                {
                    // 1. Mark as child of the active function
                    // 2. Push as active function

                    var stack = GetStackByThreadId(entry.ThreadId);
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

                        if (!enterFunc.IsHidden)
                        {
                            // We cant remove the ancestors of enterFunc becuase they contain at least
                            // one visible child.
                            MarkAsAncestorOfVisibleChild(activeFunc);
                        }
                    }

                    // This is the new active function 
                    // Stack tracks recursive calls. But they do not appear in the model later.
                    stack.Push(enterFunc);
                }
                else if (entry.Token == Tokens.TokenLeave)
                {
                    var stack = GetStackByThreadId(entry.ThreadId);
                    var activeFunc = GetActiveFunction(stack);

                    if (activeFunc != null && activeFunc.Name == entry.FunctionName)
                    {
                        stack.Pop();

                        // Reduce memory by cleaning up while we process the event stream.
                        // We remove all functions that are hidden and only call hidden functions!
                        CleanupHiddenCalls(_functions, activeFunc);
                    }
                    else
                    {
                        // Ignore. We did not start recording at the time.
                    }
                }
                else if (entry.Token == Tokens.TokenTailCall)
                {
                    FunctionCall newFunc = null;
                    if (!_functions.TryGetValue(entry.FunctioId, out newFunc))
                    {
                        newFunc = CreateFunctionCall(entry, filter);
                        newFunc.TailCall = true;
                        _functions.Add(newFunc.Id, newFunc);
                    }

                    newFunc.TailCall = true;
                }
                else if (entry.Token == Tokens.TokenDestroyThread)
                {
                    _tidToStack.Remove(entry.ThreadId);
                }
            }

            var model = new CallGraphModel(_functions.Values.ToList());
            _tidToStack.Clear();
            _functions.Clear();
            _filter = Filter.Default();
            return model;
        }

        private static void MarkAsAncestorOfVisibleChild(FunctionCall activeFunc)
        {
            HashSet<ulong> processedParent = new HashSet<ulong>();
            // Mark all parents that the have at least one visible child
            var parents = new Queue<FunctionCall>();
            parents.Enqueue(activeFunc);
            while (parents.Any())
            {
                var parent = parents.Dequeue();
                processedParent.Add(parent.Id);
                if (parent.HasVisibleChildren == false)
                {
                    parent.HasVisibleChildren = true;

                    foreach (var ancestor in parent.Parents)
                    {
                        if (!processedParent.Contains(ancestor.Id))
                        {
                            parents.Enqueue(ancestor);
                        }
                    }
                }
            }
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
            return exitFunc.IsHidden && exitFunc.HasVisibleChildren == false;
        }

        private static FunctionCall CreateFunctionCall(ProfilerEvent entry, Filter filter)
        {
            FunctionCall newFunc;
            newFunc = new FunctionCall();
            newFunc.Id = entry.FunctioId;
            newFunc.Name = entry.FunctionName;
            newFunc.IsHidden = filter.IsHidden(entry.FunctionName);
            return newFunc;
        }
    }
}