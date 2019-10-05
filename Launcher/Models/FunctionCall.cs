using System.Collections.Generic;
using System.Diagnostics;

namespace Launcher.Models
{
    [DebuggerDisplay("Func: {Name} Hidden={IsHidden}")]
    public class FunctionCall
    {
        public ulong Id { get; set; }
        public string Name { get; set; }

        public HashSet<FunctionCall> Children { get; } = new HashSet<FunctionCall>();
        public HashSet<FunctionCall> Parents { get; } = new HashSet<FunctionCall>();

        public bool Recursive { get; internal set; }
        public bool TailCall { get; internal set; }
        public bool IsHidden { get; set; }
        public bool HasVisibleChildren { get; set; } = false;

        public override bool Equals(object obj)
        {
            return Id == ((FunctionCall) obj).Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}