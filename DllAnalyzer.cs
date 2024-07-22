using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class DllAnalyzer : MarshalByRefObject
{
    public async Task<Hashtable> GetNamespacesAndClassesAsync(string folderPath)
    {
        Hashtable namespaceClasses = new Hashtable();

        Logger.Info($"Scanning DLLs in folder: {folderPath}");

        var tasks = new List<Task>();

        foreach (string dllPath in Directory.GetFiles(folderPath, "*.dll"))
        {
            tasks.Add(Task.Run(async () =>
            {
                Logger.Info($"Processing DLL: {dllPath}");
                try
                {
                    var assembly = await LoadAssemblyAsync(dllPath);
                    if (assembly != null)
                    {
                        await ProcessAssemblyTypesAsync(assembly, namespaceClasses);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    await HandleReflectionTypeLoadExceptionAsync(ex, dllPath, namespaceClasses);
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

        var tasks = new List<Task>();

        foreach (string dllPath in Directory.GetFiles(folderPath, "*.dll"))
        {
            tasks.Add(Task.Run(async () =>
            {
                Logger.Info($"Processing DLL: {dllPath}");
                try
                {
                    var assembly = await LoadAssemblyAsync(dllPath);
                    if (assembly != null)
                    {
                        List<string> dependencies = new List<string>();
                        foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                        {
                            dependencies.Add(referencedAssembly.FullName);
                        }

                        dllDependencies[Path.GetFileName(dllPath)] = dependencies;
                    }
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

    private async Task<Assembly> LoadAssemblyAsync(string dllPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                return Assembly.LoadFrom(dllPath);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load assembly {dllPath} using LoadFrom. Attempting to load using ReflectionOnlyLoad. Exception: {ex.Message}");
                try
                {
                    return Assembly.ReflectionOnlyLoadFrom(dllPath);
                }
                catch (Exception reflectionEx)
                {
                    Logger.Error($"Failed to load assembly {dllPath} using ReflectionOnlyLoadFrom. Exception: {reflectionEx.Message}");
                    return null;
                }
            }
        });
    }

    private async Task ProcessAssemblyTypesAsync(Assembly assembly, Hashtable namespaceClasses)
    {
        await Task.Run(() =>
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!namespaceClasses.ContainsKey(type.Namespace))
                {
                    namespaceClasses[type.Namespace] = new List<string>();
                }

                ((List<string>)namespaceClasses[type.Namespace]).Add(type.Name);
            }
        });
    }

    private async Task HandleReflectionTypeLoadExceptionAsync(ReflectionTypeLoadException ex, string dllPath, Hashtable namespaceClasses)
    {
        Logger.Error($"Unable to load one or more of the requested types from {dllPath}.", ex);
        foreach (var loaderException in ex.LoaderExceptions)
        {
            Logger.Error(loaderException.Message, loaderException);
            // Attempt to resolve missing assemblies or dependencies here
            await TryResolveAndLoadMissingAssemblyAsync(loaderException as FileNotFoundException);
        }

        // Retry loading types from the assembly
        try
        {
            var assembly = await LoadAssemblyAsync(dllPath);
            if (assembly != null)
            {
                await ProcessAssemblyTypesAsync(assembly, namespaceClasses);
            }
        }
        catch (Exception retryEx)
        {
            Logger.Error($"Retrying to load assembly {dllPath} failed.", retryEx);
        }
    }

    private async Task TryResolveAndLoadMissingAssemblyAsync(FileNotFoundException fileNotFoundException)
    {
        if (fileNotFoundException != null)
        {
            var missingAssemblyName = new AssemblyName(fileNotFoundException.FileName);
            Logger.Warn($"Attempting to resolve missing assembly: {missingAssemblyName.FullName}");

            // Search and load the missing assembly
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var dllPath = FindAssemblyInBaseDirectory(missingAssemblyName.Name, baseDirectory);
            if (!string.IsNullOrEmpty(dllPath))
            {
                await LoadAssemblyAsync(dllPath);
            }
        }
    }

    private string FindAssemblyInBaseDirectory(string assemblyName, string baseDirectory)
    {
        var files = Directory.GetFiles(baseDirectory, "*.dll", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (Path.GetFileNameWithoutExtension(file).Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
        }
        return null;
    }
}
