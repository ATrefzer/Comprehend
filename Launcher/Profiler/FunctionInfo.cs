namespace Launcher.Profiler
{
    /// <summary>
    /// Function info from the index file. This info was collected during the trace.
    /// Additionally we can pass if the function shall be filtered.
    /// This means not relevant at all and therefore not considered in some models later)
    /// </summary>
    public class FunctionInfo
    {
        public FunctionInfo(ulong id, string fullName, bool isPublic, bool isBanned)
        {
            Id = id;
            FullName = fullName;
            IsPublic = isPublic;
            IsBanned = isBanned;

            var lastDot = fullName.LastIndexOf('.');
            Function = fullName.Substring(lastDot + 1).Trim('.');

            var bang = fullName.IndexOf('!');
            if (bang >= 0)
            {
                Module = fullName.Substring(0, bang).Trim('.');
                TypeName = fullName.Substring(bang + 1, lastDot - bang - 1).Trim('.');
            }
            else
            {
                Module = "unknown";
                TypeName = fullName.Substring(0, lastDot).Trim('.');
            }
        }

        /// <summary>
        /// Derived from FullName
        /// </summary>
        public string Function { get; }

        /// <summary>
        /// Derived from FullName
        /// </summary>
        public string Module { get; }

        /// <summary>
        /// Derived from FullName
        /// </summary>
        public string TypeName { get; }

        public ulong Id { get; }
        public string FullName { get; }

        /// <summary>
        /// Cannot be changed afterwards. Reduce data.
        /// </summary>
        public bool IsBanned { get; }
        public bool IsPublic { get; }


        public bool IsCtor => Function == ".ctor" || Function == "ctor";

        public override string ToString()
        {
            return FullName;
        }

        public class Parts
        {
            public string Module;
            public string TypeName;
            public string Function;
        }
    }
}