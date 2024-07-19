using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class DllAnalyzer : IDllAnalyzer
{
    public async Task<Hashtable> GetNamespacesAndClassesAsync(string folderPath)
    {
        Hashtable namespaceClasses = new Hashtable();

        Logger.Info($"Scanning DLLs in folder: {folderPath}");

        var dllFiles = Directory.GetFiles(folderPath, "*.dll");
        var tasks = new List<Task>();

        foreach (string dllPath in dllFiles)
        {
            tasks.Add(Task.Run(async () =>
            {
                Logger.Info($"Processing DLL: {dllPath}");
                try
                {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(dllPath);
                    await LoadReferencedAssembliesAsync(assembly, folderPath);

                    foreach (var type in assembly.GetTypes())
                    {
                        if (!namespaceClasses.ContainsKey(type.Namespace))
                        {
                            namespaceClasses[type.Namespace] = new List<string>();
                        }

                        ((List<string>)namespaceClasses[type.Namespace]).Add(type.Name);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Logger.Error($"Unable to load one or more of the requested types from {dllPath}.", ex);
                    foreach (var loaderException in ex.LoaderExceptions)
                    {
                        Logger.Error(loaderException.Message, loaderException);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"An error occurred while processing {dllPath}.", ex);
                }
            }));
        }

        await Task.WhenAll(tasks);

        Logger.Info("Completed scanning namespaces and classes.");
        return namespaceClasses;
    }

    public async Task<Dictionary<string, List<string>>> GetDllDependenciesAsync(string folderPath)
    {
        Dictionary<string, List<string>> dllDependencies = new Dictionary<string, List<string>>();

        Logger.Info($"Scanning DLL dependencies in folder: {folderPath}");

        var dllFiles = Directory.GetFiles(folderPath, "*.dll");
        var tasks = new List<Task>();

        foreach (string dllPath in dllFiles)
        {
            tasks.Add(Task.Run(async () =>
            {
                Logger.Info($"Processing DLL: {dllPath}");
                try
                {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(dllPath);
                    await LoadReferencedAssembliesAsync(assembly, folderPath);

                    List<string> dependencies = new List<string>();
                    foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                    {
                        dependencies.Add(referencedAssembly.FullName);
                    }

                    dllDependencies[Path.GetFileName(dllPath)] = dependencies;
                }
                catch (Exception ex)
                {
                    Logger.Error($"An error occurred while processing {dllPath}.", ex);
                }
            }));
        }

        await Task.WhenAll(tasks);

        Logger.Info("Completed scanning DLL dependencies.");
        return dllDependencies;
    }

    private async Task LoadReferencedAssembliesAsync(Assembly assembly, string baseDirectory)
    {
        var tasks = new List<Task>();

        foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    Assembly.ReflectionOnlyLoad(referencedAssemblyName.FullName);
                }
                catch (FileNotFoundException)
                {
                    Logger.Error($"Referenced assembly {referencedAssemblyName.FullName} could not be found. Attempting to locate it.");

                    // Try to find the assembly in the base directory and subdirectories
                    string assemblyPath = await FindAssemblyInBaseDirectoryAsync(baseDirectory, referencedAssemblyName.Name);
                    if (assemblyPath != null)
                    {
                        try
                        {
                            var loadedAssembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
                            await LoadReferencedAssembliesAsync(loadedAssembly, baseDirectory); // Recursively load dependencies
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to load assembly from path: {assemblyPath}.", ex);
                        }
                    }
                    else
                    {
                        Logger.Error($"Assembly {referencedAssemblyName.Name} could not be found in base directory {baseDirectory}.");
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    private async Task<string> FindAssemblyInBaseDirectoryAsync(string baseDirectory, string assemblyName)
    {
        return await Task.Run(() =>
        {
            foreach (var file in Directory.GetFiles(baseDirectory, $"{assemblyName}.dll", SearchOption.AllDirectories))
            {
                return file;
            }
            return null;
        });
    }
}
