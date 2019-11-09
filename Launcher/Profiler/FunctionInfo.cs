﻿using GraphFormats;

namespace Launcher.Profiler
{
    public class FunctionInfo 
    {
        public FunctionInfo(ulong id, string fullName, bool isPublic, bool isFiltered)
        {
            Id = id;
            FullName = fullName;
            IsPublic = isPublic;
            IsFiltered = isFiltered;

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

        public ulong Id { get; set; }
        public string FullName { get; set; }
        public bool IsFiltered { get; set; }
        public bool IsPublic { get; set; }

        public override string ToString()
        {
            return FullName;
        }


        public Parts SplitFullName(string name)
        {
            var parts = new Parts();

            return parts;
        }

        private bool IsCtor()
        {
            return Function.EndsWith("ctor");
        }

        public class Parts
        {
            public string Module;
            public string TypeName;
            public string Function;
        }
    }
}