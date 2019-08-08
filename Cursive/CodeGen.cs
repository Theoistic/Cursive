using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursive
{
    public enum CodeGenType { Library, Executable }

    public class CompilationError
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }

    public static class CodeGen
    {
        private static readonly IEnumerable<string> DefaultNamespaces = new[] {
            "System",
            "System.IO",
            "System.Net",
            "System.Linq",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Collections.Generic",
            "Microsoft.CSharp.RuntimeBinder"
        };

        internal static string CompilerDirectory => Path.GetDirectoryName(typeof(CodeGen).Assembly.Location);

        public static IList<MetadataReference> References(IEnumerable<string> refs)
        {
            List<MetadataReference> references = new List<MetadataReference>();
            references.AddRange(Directory.GetFiles(OutputDirectory, "*.dll").Select(x => MetadataReference.CreateFromFile(x)));
            return references;
        }

        public static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text, Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
        }

        public static string OutputDirectory => Path.Combine(Directory.GetCurrentDirectory(), "output");

        public static async Task<IEnumerable<CompilationError>> Compile(Config config, string[] source, bool emitpdb = false)
        {
            Directory.CreateDirectory(OutputDirectory);

            // resolve dependencies
            await config.ResolveDependencies();

            CSharpCompilationOptions DefaultCompilationOptions =
            new CSharpCompilationOptions(config.Name.EndsWith("dll") ? OutputKind.DynamicallyLinkedLibrary : OutputKind.ConsoleApplication)
                    .WithOverflowChecks(true)
                    .WithOptimizationLevel(OptimizationLevel.Release)
                    .WithUsings(DefaultNamespaces);

            List<SyntaxTree> syntaxTrees = source.Select(x => Parse(x)).ToList();
            var compilation = CSharpCompilation.Create(config.NameWithoutExtension, syntaxTrees, References(null), DefaultCompilationOptions);

            try {
                var result = compilation.Emit(Path.Combine(OutputDirectory, config.Name), emitpdb ? Path.Combine(OutputDirectory, $"{config.NameWithoutExtension}.pdb") : null);
                return result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => new CompilationError { Type = "Error", Message = x.GetMessage() });
            } catch (Exception ex) {
                return new List<CompilationError> {
                    new CompilationError
                    {
                        Type = "Error",
                        Message = ex.Message
                    },
                    new CompilationError {
                        Type = "Fatal",
                        Message = "Unable to run the compilation. CODE #7"
                    }
                };
            }
        }
    }
}
