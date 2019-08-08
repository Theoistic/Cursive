using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cursive
{
    public class Config
    {
        internal string FileLocation { get; set; }

        public string Name { get; set; }
        public string TargetFramework { get; set; } = ".NETCoreApp2.0";
        public Dictionary<string, string> Dependencies { get; set; } = new Dictionary<string, string>() { { "Microsoft.NETCore.App", "2.0.0"} };

        internal string Extension => Path.GetExtension(Name);
        internal string NameWithoutExtension => Path.GetFileNameWithoutExtension(Name);
        internal string Output => string.IsNullOrEmpty(Extension) ? $"{Name}.exe" : $"{Name}";

        internal async Task ResolveDependencies()
        {
            Logger.Write("Resolving Dependencies ...");
            foreach(var dep in Dependencies)
            {
                await PackageManager.Resolve(dep.Key, dep.Value, TargetFramework, CodeGen.OutputDirectory);
            }
        }
    }
}
