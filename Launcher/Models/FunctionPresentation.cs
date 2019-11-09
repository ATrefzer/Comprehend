using System.Diagnostics;

using GraphFormats;

namespace Launcher.Models
{
    [DebuggerDisplay("Type: {TypeName} Func={Function} Ctor={IsCtor}")]
    internal class FunctionPresentation : IFunctionPresentation
    {
        private readonly FunctionCall _call;

        public FunctionPresentation(FunctionCall call)
        {
            _call = call;

            if (call == null)
            {
                IsNull = true;
                IsCtor = false;
                return;
            }

            IsNull = false;
            TypeName = call.TypeName;
            Function = call.Function;
            IsCtor = call.IsCtor;
            IsRecursive = call.IsRecursive;
            FullName = call.FullName;

            // Underlying object is modified by the filter UI
            IsFiltered = call.IsFiltered;
        }

        public FunctionPresentation()
        {
        }

        public string FullName { get; }

        public bool IsRecursive { get; }

        public string TypeName { get; set; }
        public string Function { get; }

        public bool IsCtor { get; set; }

        public bool IsNull { get; }


        public bool IsFiltered { get; set; }
    }
}