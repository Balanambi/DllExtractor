using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup dependency injection
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IDllAnalyzer, DllAnalyzer>()
            .BuildServiceProvider();

        var dllAnalyzer = serviceProvider.GetService<IDllAnalyzer>();

        string folderPath = @"C:\Path\To\Your\Dlls"; // Change this to your folder path

        Logger.Info("Starting analysis of DLLs.");

        try
        {
            var namespaceClasses = await dllAnalyzer.GetNamespacesAndClassesAsync(folderPath);
            var dllDependencies = await dllAnalyzer.GetDllDependenciesAsync(folderPath);

            // Display the namespaces and classes
            Logger.Info("Displaying namespaces and classes.");
            foreach (DictionaryEntry entry in namespaceClasses)
            {
                Console.WriteLine($"Namespace: {entry.Key}");
                foreach (var className in (List<string>)entry.Value)
                {
                    Console.WriteLine($"\tClass: {className}");
                }
            }

            // Display the DLL dependencies
            Logger.Info("Displaying DLL dependencies.");
            Console.WriteLine("\nDLL Dependencies:");
            foreach (var entry in dllDependencies)
            {
                Console.WriteLine($"DLL: {entry.Key}");
                foreach (var dependency in entry.Value)
                {
                    Console.WriteLine($"\tDependency: {dependency}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("An error occurred during the DLL analysis process.", ex);
        }

        Logger.Info("Completed analysis of DLLs.");
    }
}
