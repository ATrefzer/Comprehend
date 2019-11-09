using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GraphFormats;

using Launcher.Profiler;

namespace Launcher.Models
{
    /// <summary>
    /// Adds further information to FunctionInfo derived from the stack trace like call graph and recursion
    /// </summary>
    [DebuggerDisplay("Func: {FullName} Hidden={IsFiltered}")]
    public class FunctionCall : IFunction
    {
        private readonly FunctionInfo _info;

        public FunctionCall(FunctionInfo info)
        {
            _info = info;
        }

        public HashSet<FunctionCall> Children { get; } = new HashSet<FunctionCall>();
        public HashSet<FunctionCall> Parents { get; } = new HashSet<FunctionCall>();

        public bool IsRecursive { get; internal set; }
        public bool TailCall { get; internal set; }

        public bool IsFiltered => _info.IsFiltered;

        public ulong Id => _info.Id;

        public string FullName => _info.FullName;

        public bool HasVisibleChildren { get; set; } = false;
        public bool IsPublic => _info.IsPublic;

        public override bool Equals(object obj)
        {
            return _info.Id == ((FunctionCall) obj)._info.Id;
        }

        public override int GetHashCode()
        {
            return _info.Id.GetHashCode();
        }

        public List<FunctionCall> GetAncestorChain()
        {
            var allAncestors = new HashSet<FunctionCall>(Parents);

            // Start with direct parents
            var toProcess = new Queue<FunctionCall>(Parents);

            while (toProcess.Any())
            {
                var parent = toProcess.Dequeue();
                if (allAncestors.Add(parent))
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

        public string TypeName => _info.TypeName;

        public string Function => _info.Function;

        public bool IsCtor => _info.IsCtor;
    }
   
}