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


        public static CallGraphModel FromEventStream(IEnumerable<ProfilerEvent> stream, Filter filter)
        {
            var tidToStack = new Dictionary<ulong, Stack<FunctionCall>>();
            var functions = new Dictionary<ulong, FunctionCall>();

            foreach (var entry in stream)
            {
                if (entry.Token == Tokens.TokenEnter)
                {
                    // 1. Note as child of the active function
                    // 2. Push as active function

                    FunctionCall enterFunc = null;
                    if (!functions.TryGetValue(entry.FunctioId, out enterFunc))
                    {
                        enterFunc = CreateFunctionCall(entry, filter);
                        functions.Add(enterFunc.Id, enterFunc);
                    }

                    // Find the correct thread such that we find the correct parent function.
                    if (!tidToStack.TryGetValue(entry.ThreadId, out var stack))
                    {
                        stack = new Stack<FunctionCall>();
                        tidToStack.Add(entry.ThreadId, stack);
                    }

                    Debug.Assert(enterFunc != null);

                    // Find active function
                    FunctionCall activeFunc = null;
                    if (stack.Count > 0)
                    {
                        activeFunc = stack.Peek();
                    }

                    if (ReferenceEquals(activeFunc, enterFunc)) // reference
                    {
                        activeFunc.Recursive = true;
                    }
                    else if (activeFunc != null)
                    {
                        activeFunc.Children.Add(enterFunc);
                        enterFunc.Parents.Add(activeFunc);
                    }

                    // This is the new active function 
                    // Stack tracks recursive calls. But they do not appear in the model later.
                    stack.Push(enterFunc);
                }
                else if (entry.Token == Tokens.TokenLeave)
                {
                    // Find the correct thread such that we find the correct parent functions.
                    Stack<FunctionCall> stack;
                    if (!tidToStack.TryGetValue(entry.ThreadId, out stack))
                    {
                        stack = new Stack<FunctionCall>();
                        tidToStack.Add(entry.ThreadId, stack);
                    }

                    // Find active function
                    FunctionCall activeFunc = null;
                    if (stack.Count > 0)
                    {
                        activeFunc = stack.Peek();
                    }

                    if (activeFunc != null && activeFunc.Name == entry.FunctionName)
                    {
                        stack.Pop();

                        // Reduce memory by cleaning up while be process the event stream.
                        // We remove all functions that are hidden and only call hidden functions!
                        CleanupHiddenCalls(functions, activeFunc);
                    }
                    else
                    {
                        // Ignore. We did not start recording at the time.
                    }
                }
                else if (entry.Token == Tokens.TokenTailCall)
                {
                    FunctionCall newFunc = null;
                    if (!functions.TryGetValue(entry.FunctioId, out newFunc))
                    {
                        newFunc = CreateFunctionCall(entry, filter);
                        newFunc.TailCall = true;
                        functions.Add(newFunc.Id, newFunc);
                    }

                    newFunc.TailCall = true;
                }
                else if (entry.Token == Tokens.TokenDestroyThread)
                {
                    tidToStack.Remove(entry.ThreadId);
                }
            }

            return new CallGraphModel(functions.Values.ToList());
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
            return CanRemove(exitFunc, new HashSet<ulong>());
        }

        /// <summary>
        /// Are there only hidden functions called
        /// </summary>
        private static bool CanRemove(FunctionCall exitFunc, HashSet<ulong> visited)
        {
            if (!exitFunc.IsHidden)
            {
                return false;
            }

            foreach (var child in exitFunc.Children)
            {
                if (visited.Contains(child.Id))
                {
                    continue;
                }

                // Avoid cycles
                visited.Add(child.Id);

                if (!CanRemove(child, visited))
                {
                    // At lease one child cannot be removed.
                    return false;
                }
            }

            // Nothing found that keeps us from removing this hidden function with only hidden children.
            return true;
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