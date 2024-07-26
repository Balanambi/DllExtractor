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
                var decompiler = new WholeProjectDecompiler();
                decompiler.DecompileProject(dllPath, outputDir);
                Console.WriteLine($"Decompilation complete. Files have been saved to {outputDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
