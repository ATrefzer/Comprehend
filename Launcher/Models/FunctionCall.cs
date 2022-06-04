using GraphFormats.PlantUml;
using Launcher.Profiler;

namespace Launcher.Models
{
    public abstract class FunctionCall : IFunctionPresentation
    {
        protected readonly FunctionInfo Info;

        public FunctionCall(FunctionInfo info)
        {
            Info = info;

            // By default the pre-filtered functions are not included.
            IsIncluded = !info.IsBanned;
        }

        public bool IsRecursive { get; internal set; }
        public bool TailCall { get; internal set; }

        public bool IsBanned => Info.IsBanned;

        public ulong Id => Info.Id;

        public string FullName => Info.FullName;

        public bool HasVisibleChildren { get; set; } = false;
        public bool IsPublic => Info.IsPublic;

        public bool IsCtor => Info.IsCtor;

        /// <summary>
        ///     Custom filtering.
        ///     For sequence diagrams. Allow including banned functions!
        /// </summary>
        public bool IsIncluded { get; set; }

        public string TypeName => Info.TypeName;

        public string Function => Info.Function;

        public override bool Equals(object obj)
        {
            return Info.Id == ((FunctionCall)obj).Info.Id;
        }

        public override int GetHashCode()
        {
            return Info.Id.GetHashCode();
        }
    }
}