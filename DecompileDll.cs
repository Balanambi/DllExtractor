using System;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;

namespace DecompileDll
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the path to the DLL you want to decompile.");
                return;
            }

            string dllPath = args[0];
            string outputDir = args.Length > 1 ? args[1] : Path.GetDirectoryName(dllPath);

            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"File {dllPath} does not exist.");
                return;
            }

            try
            {
                DecompileAssembly(dllPath, outputDir);
                Console.WriteLine($"Decompilation complete. Files have been saved to {outputDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static void DecompileAssembly(string assemblyPath, string outputDir)
        {
            var fileName = Path.GetFileNameWithoutExtension(assemblyPath);
            var outputFilePath = Path.Combine(outputDir, fileName + ".cs");

            var module = new PEFile(assemblyPath);
            var resolver = new UniversalAssemblyResolver(assemblyPath, true, module.Reader.DetectTargetFrameworkId());
            var decompiler = new CSharpDecompiler(assemblyPath, resolver, new DecompilerSettings());

            var code = decompiler.DecompileWholeModuleAsString();
            File.WriteAllText(outputFilePath, code);
        }
    }
}
