using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace TransitiveDependencyFinder
{
    public class DependencyService : IDependencyService
    {
        private readonly HashSet<string> _visitedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, List<string>> GetTransitiveDependencies(string assemblyPath)
        {
            var dependencies = new Dictionary<string, List<string>>();

            try
            {
                var resolver = new UniversalAssemblyResolver(assemblyPath, true, null);
                var rootAssembly = resolver.Resolve(Path.GetFileName(assemblyPath));

                if (rootAssembly != null)
                {
                    GetDependenciesRecursive(rootAssembly, dependencies, resolver);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assembly: {ex.Message}");
            }

            return dependencies;
        }

        private void GetDependenciesRecursive(PEFile assembly, Dictionary<string, List<string>> dependencies, UniversalAssemblyResolver resolver)
        {
            if (_visitedAssemblies.Contains(assembly.FullName))
            {
                return;
            }

            _visitedAssemblies.Add(assembly.FullName);

            var assemblyPath = assembly.FileName;
            if (!dependencies.ContainsKey(assemblyPath))
            {
                dependencies[assemblyPath] = new List<string>();
            }

            foreach (var reference in assembly.AssemblyReferences)
            {
                try
                {
                    var referencedAssembly = resolver.Resolve(reference);
                    var referencedAssemblyPath = referencedAssembly.FileName;

                    if (!dependencies[assemblyPath].Contains(referencedAssemblyPath))
                    {
                        dependencies[assemblyPath].Add(referencedAssemblyPath);
                    }

                    GetDependenciesRecursive(referencedAssembly, dependencies, resolver);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading referenced assembly: {reference.FullName}. Exception: {ex.Message}");
                }
            }
        }

        public bool IsDllInTransitiveDependencies(string assemblyPath, string dllName)
        {
            var dependencies = GetTransitiveDependencies(assemblyPath);
            foreach (var deps in dependencies.Values)
            {
                foreach (var dep in deps)
                {
                    if (Path.GetFileName(dep).Equals(dllName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
