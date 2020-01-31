using System.Collections.Generic;

using Launcher.Profiler;

namespace Launcher.Models
{
    public class BaseModel
    {
        protected readonly Dictionary<ulong, Stack<FunctionCall>> TidToStack = new Dictionary<ulong, Stack<FunctionCall>>();

        // All functions resolved
        protected readonly Dictionary<ulong, FunctionCall> Functions = new Dictionary<ulong, FunctionCall>();


        protected FunctionCall GetEnteredFunction(ProfilerEvent entry)
        {
            FunctionCall enterFunc;
            if (!Functions.TryGetValue(entry.Func.Id, out enterFunc))
            {
                enterFunc = CreateFunctionCall(entry);
                Functions.Add(enterFunc.Id, enterFunc);
            }

            return enterFunc;
        }

        protected void Clear()
        {
            TidToStack.Clear();
            Functions.Clear();
        }

        protected FunctionCall GetActiveFunction(Stack<FunctionCall> stack)
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

        protected Stack<FunctionCall> GetOrCreateStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            if (!TidToStack.TryGetValue(threadId, out var stack))
            {
                stack = new Stack<FunctionCall>();
                TidToStack.Add(threadId, stack);
            }

            return stack;
        }

        protected Stack<FunctionCall> FindStackByThreadId(ulong threadId)
        {
            // Find the correct thread such that we find the correct parent function.
            TidToStack.TryGetValue(threadId, out var stack);
            return stack;
        }

        protected FunctionCall CreateFunctionCall(ProfilerEvent entry)
        {
            FunctionCall newFunc;
            newFunc = new FunctionCall(entry.Func);
            return newFunc;
        }
    }
}