using System.IO;

namespace Launcher.Profiler
{
    internal class Profile
    {

        public Profile(string directory, string baseFile)
        {
            Directory = directory;
            BaseFile = baseFile;
        }

        public string Directory { get; set; }

        public string BaseFile { get; }

        public string IndexFile => Path.Combine(Directory, BaseFile + ".index");
        public string EventFile => Path.Combine(Directory, BaseFile + ".profile");

        public override string ToString()
        {
            return BaseFile;
        }
    }
}