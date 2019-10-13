using System.Collections.Generic;

using Launcher.Profiler;

namespace Launcher.Models
{
    public class BaseModel
    {
        protected static readonly Dictionary<ulong, Stack<FunctionCall>> _tidToStack = new Dictionary<ulong, Stack<FunctionCall>>();

        // All functions resolved
        protected static readonly Dictionary<ulong, FunctionCall> _functions = new Dictionary<ulong, FunctionCall>();


        protected static FunctionCall GetEnteredFunction(ProfilerEvent entry)
        {
            FunctionCall enterFunc = null;
            if (!_functions.TryGetValue(entry.Func.Id, out enterFunc))
            {
                enterFunc = CreateFunctionCall(entry);
                _functions.Add(enterFunc.Id, enterFunc);
            }

            return enterFunc;
        }

        protected static void Clear()
        {
            _tidToStack.Clear();
            _functions.Clear();
        }

        protected static FunctionCall GetActiveFunction(Stack<FunctionCall> stack)
        {
            if (stack == null)
            {
                return null;
            }

            // Find active function
            FunctionCall activeFunc = null;
            if (stack.Count > 0)
            {
                activeFunc = stack.Peek();
            }

            return activeFunc;
        }

        protected static Stack<FunctionCall> GetOrCreateStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            if (!_tidToStack.TryGetValue(threadId, out var stack))
            {
                stack = new Stack<FunctionCall>();
                _tidToStack.Add(threadId, stack);
            }

            return stack;
        }

        protected static Stack<FunctionCall> FindStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            _tidToStack.TryGetValue(threadId, out var stack);
            return stack;
        }

        protected static FunctionCall CreateFunctionCall(ProfilerEvent entry)
        {
            FunctionCall newFunc;
            newFunc = new FunctionCall(entry.Func);
            return newFunc;
        }


        private BaseModel CreateFromEventStream()
        {
            return null;
        }
    }
}