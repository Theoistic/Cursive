using Antlr4.Runtime;
using CursiveLanguage;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

[assembly: CLSCompliant(false)]
namespace Cursive
{

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                PrintInfo();

                if (args.Length > 0 && args[0].ToLower() == "config.json")
                {
                    Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(args[0]));
                    config.FileLocation = args[0];
                    await CompileProject(config);
                } else if(args.Length > 0 && Directory.Exists(args[0])) {
                    Config config = null;
                    if (File.Exists(Path.Combine(args[0], "config.json"))) {
                        config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(args[0], "config.json")));
                        config.FileLocation = Path.Combine(args[0], "config.json");
                    } else {
                        New(args[0]);
                    }
                    await CompileProject(config);
                }
                else if (args.Length > 0)
                {
                    await NCMD.Parse(args);
                } else
                {
                    Logger.Write("No arguments where given.. ", ConsoleColor.Red);
                }
            }
            catch (AggregateException ex)
            {
                Logger.Write($"Internal error: {ex.Message}", ConsoleColor.Red);
            }

            /*if (Debugger.IsAttached)
                Console.ReadLine();*/
        }

        private static void PrintInfo()
        {
            Logger.Write($"** Cursive Compiler [Version: {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion}] - https://Cursive.Dev  **");
        }

        public static async Task CompileProject(Config config, bool debug = false)
        {
            var timer = Stopwatch.StartNew();
            var tasks = new List<Task<string>>();
            string[] localSources = Directory.GetFiles(Path.GetDirectoryName(config.FileLocation), "*.lewd");
            foreach (var filename in localSources)
            {
                // Silly closure semantics
                var actual = filename;
                tasks.Add(Task.Run(() =>
                {
                    using (var file = File.OpenRead(actual))
                    {
                        var input = new AntlrInputStream(file);
                        var lexer = new CursiveLexer(input);
                        var tokens = new CommonTokenStream(lexer);
                        var parser = new CursiveParser(tokens);

                        Logger.Write($"Parsed: {actual}");

                        parser.RemoveErrorListeners();
                        parser.AddErrorListener(new ErrorListener());
                        var visitor = new CursiveTranslator(actual);
                        var text = visitor.Visit(parser.prog());

                        /*using (var translated = File.Open(actual.Replace(".lewd", ".cs"), FileMode.Create))
                        using (var stream = new StreamWriter(translated))
                            await stream.WriteAsync(text);*/

                        return text;
                    }
                }));
            }
            Logger.Write("Parsing Files ...");
            var sources = await Task.WhenAll(tasks);
            Logger.Write($"Parsed all files {timer.Elapsed.TotalMilliseconds / 60}");
            Logger.Write("Compiling ...");
            var errors = await CodeGen.Compile(config, sources, debug);
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Logger.Write(error.Message, ConsoleColor.Red);
                }
            }
            else
            {
                Logger.Write($"Compiled {timer.Elapsed.TotalMilliseconds / 60}");
                Logger.Write("Done.", ConsoleColor.Green);
            }
        }

        [CMD]
        public static async Task Resolve(string name, string version, string targetVersion = null)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "output");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            await PackageManager.Resolve(name, version, ".NETCoreApp2.0", path);
        }

        [CMD]
        public static void New(string name)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), name);
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var config = new Config
            {
                FileLocation = Path.Combine(name, "config.json"), // todo not tested.
                Name = name + ".exe"
            };
            File.WriteAllText(Path.Combine(path, "config.json"), JsonConvert.SerializeObject(config, Formatting.Indented));
            File.WriteAllText(Path.Combine(path, "program.lewd"), "\n\ncum drop Program\n\tMain is a static cum stain\n\t\tbitching \"Hello World!\"!");
            Logger.Write($"Created New Project {name}");
            Open(path);
        }

        [CMD]
        public static void Open(string path = null)
        {
            string proc = Utils.GetToolFullPath("code");
            if (proc != null)
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = Directory.GetCurrentDirectory();
                }
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = "code", Arguments = path, UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden });
                }
                catch(Exception ex)
                {
                    Logger.Write("Visual Studio Code is not installed (Recommended)", ConsoleColor.Yellow);
                }
            }
        }

        [CMD]
        public static async Task Dbg(string project)
        {
            Config config = null;
            if (!File.Exists(Path.Combine(project, "config.json")))
            {
                Logger.Error("No Cursive project exists here.");
                return;
            }
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(project, "config.json")));
            config.FileLocation = Path.Combine(project, "config.json");
            await CompileProject(config, true);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Path.Combine(project, "output", config.Output);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Logger.Write($"Starting {config.Output}");
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    //Debugging.Debugger.Debug(config.Output);
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        [CMD]
        public static async Task Run()
        {
            string path = Directory.GetCurrentDirectory();
            await Dbg(path);
        }

        [CMD]
        public static void Help()
        {
            Logger.Write("usage: ");
            Logger.Write(" Compile:   Cursive \"c:\\projectfolder\\\"");
            Logger.Write(" Compile:   Cursive \"c:\\projectfolder\\config.json\"");
            Logger.Write(" New:       Cursive new -name \"MyProject\"");
            Logger.Write(" Open:      Cursive open");
            Logger.Write(" Run:       Cursive run");
            Logger.Write(" Resolve:   Cursive Resolve -name \"SomeNugetPackage\" -version \"1.2.3.4\"    (this will resolve the package to the current directory)");
        }
    }
}
