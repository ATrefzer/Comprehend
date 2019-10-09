using System.Collections.Generic;
using System.Diagnostics;

using Launcher.Profiler;

namespace Launcher.Models
{
    [DebuggerDisplay("Func: {Name} Hidden={IsHidden}")]
    public class FunctionCall
    {
        private readonly FunctionInfo _info;

        public FunctionCall(FunctionInfo info)
        {
            _info = info;
        }

        public HashSet<FunctionCall> Children { get; } = new HashSet<FunctionCall>();
        public HashSet<FunctionCall> Parents { get; } = new HashSet<FunctionCall>();

        public bool Recursive { get; internal set; }
        public bool TailCall { get; internal set; }

        public bool IsHidden => _info.IsHidden;

        /// <summary>
        /// Returns true if the functions is an entry function for the following analysis.
        /// </summary>
        public bool IsEntry => _info.IsEntry;

        public ulong Id => _info.Id;

        public string Name => _info.Name;

        public bool HasVisibleChildren { get; set; } = false;

        public override bool Equals(object obj)
        {
            return _info.Id == ((FunctionCall) obj)._info.Id;
        }

        public override int GetHashCode()
        {
            return _info.Id.GetHashCode();
        }
    }
}