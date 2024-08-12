using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;

namespace TransitiveDependencyFinder
{
    public class DependencyService : IDependencyService
    {
        private readonly HashSet<string> _visitedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public DependencyNode GetTransitiveDependencies(string assemblyPath)
        {
            DependencyNode rootNode = null;

            try
            {
                var resolver = new UniversalAssemblyResolver(assemblyPath, true, null);
                var rootAssembly = resolver.Resolve(Path.GetFileName(assemblyPath));

                if (rootAssembly != null)
                {
                    rootNode = new DependencyNode(assemblyPath);
                    GetDependenciesRecursive(rootAssembly, rootNode, resolver);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assembly: {ex.Message}");
            }

            return rootNode;
        }

        private void GetDependenciesRecursive(PEFile assembly, DependencyNode currentNode, UniversalAssemblyResolver resolver)
        {
            if (_visitedAssemblies.Contains(assembly.FullName))
            {
                return;
            }

            _visitedAssemblies.Add(assembly.FullName);

            foreach (var reference in assembly.AssemblyReferences)
            {
                try
                {
                    var referencedAssembly = resolver.Resolve(reference);
                    var referencedAssemblyPath = referencedAssembly.FileName;

                    var childNode = new DependencyNode(referencedAssemblyPath);
                    currentNode.Dependencies.Add(childNode);

                    GetDependenciesRecursive(referencedAssembly, childNode, resolver);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading referenced assembly: {reference.FullName}. Exception: {ex.Message}");
                }
            }
        }

        public bool IsDllInTransitiveDependencies(string assemblyPath, string dllName)
        {
            var rootNode = GetTransitiveDependencies(assemblyPath);
            return SearchNode(rootNode, dllName);
        }

        private bool SearchNode(DependencyNode node, string dllName)
        {
            if (node == null)
            {
                return false;
            }

            if (Path.GetFileName(node.AssemblyPath).Equals(dllName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (var child in node.Dependencies)
            {
                if (SearchNode(child, dllName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}