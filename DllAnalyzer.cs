using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class DllAnalyzer : IDllAnalyzer
{
    public Hashtable GetNamespacesAndClasses(string folderPath)
    {
        Hashtable namespaceClasses = new Hashtable();

        Logger.Info($"Scanning DLLs in folder: {folderPath}");

        foreach (string dllPath in Directory.GetFiles(folderPath, "*.dll"))
        {
            Logger.Info($"Processing DLL: {dllPath}");
            try
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(dllPath);
                LoadReferencedAssemblies(assembly, folderPath);

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
        }

        Logger.Info("Completed scanning namespaces and classes.");
        return namespaceClasses;
    }

    public Dictionary<string, List<string>> GetDllDependencies(string folderPath)
    {
        Dictionary<string, List<string>> dllDependencies = new Dictionary<string, List<string>>();

        Logger.Info($"Scanning DLL dependencies in folder: {folderPath}");

        foreach (string dllPath in Directory.GetFiles(folderPath, "*.dll"))
        {
            Logger.Info($"Processing DLL: {dllPath}");
            try
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(dllPath);
                LoadReferencedAssemblies(assembly, folderPath);

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
        }

        Logger.Info("Completed scanning DLL dependencies.");
        return dllDependencies;
    }

    private void LoadReferencedAssemblies(Assembly assembly, string baseDirectory)
    {
        foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
        {
            try
            {
                Assembly.ReflectionOnlyLoad(referencedAssemblyName.FullName);
            }
            catch (FileNotFoundException)
            {
                Logger.Error($"Referenced assembly {referencedAssemblyName.FullName} could not be found. Attempting to locate it.");

                // Try to find the assembly in the base directory and subdirectories
                string assemblyPath = FindAssemblyInBaseDirectory(baseDirectory, referencedAssemblyName.Name);
                if (assemblyPath != null)
                {
                    try
                    {
                        var loadedAssembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
                        LoadReferencedAssemblies(loadedAssembly, baseDirectory); // Recursively load dependencies
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
        }
    }

    private string FindAssemblyInBaseDirectory(string baseDirectory, string assemblyName)
    {
        foreach (var file in Directory.GetFiles(baseDirectory, $"{assemblyName}.dll", SearchOption.AllDirectories))
        {
            return file;
        }
        return null;
    }
}
