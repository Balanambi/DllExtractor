using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ProjectNamespaceClassExtractor.Interfaces;
using ProjectNamespaceClassExtractor.Logging;

namespace ProjectNamespaceClassExtractor.Services
{
    public class NamespaceClassExtractor : INamespaceClassExtractor
    {
        public Hashtable ExtractNamespacesAndClasses(string assemblyPath)
        {
            var namespaceClassMap = new Hashtable();

            try
            {
                var module = new PEFile(assemblyPath);
                var resolver = new UniversalAssemblyResolver(assemblyPath, true, ".NETFramework,Version=v4.7.2");
                resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
                AddFrameworkSearchDirectories(resolver);

                var decompiler = new CSharpDecompiler(assemblyPath, resolver, new DecompilerSettings());
                var typeSystem = decompiler.TypeSystem;

                foreach (var type in typeSystem.MainModule.TypeDefinitions)
                {
                    if (!type.IsPublic)
                        continue;

                    var namespaceName = type.Namespace;
                    var className = type.Name;

                    if (!namespaceClassMap.ContainsKey(namespaceName))
                    {
                        namespaceClassMap[namespaceName] = new List<string>();
                    }

                    ((List<string>)namespaceClassMap[namespaceName]).Add(className);
                }

                Log4NetConfig.Logger.Info($"Extracted namespaces and classes from {assemblyPath}.");
            }
            catch (Exception ex)
            {
                Log4NetConfig.Logger.Error($"Error extracting namespaces and classes from {assemblyPath}.", ex);
                throw;
            }

            return namespaceClassMap;
        }

        private void AddFrameworkSearchDirectories(UniversalAssemblyResolver resolver)
        {
            var frameworkDirectories = new[]
            {
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL",
                @"C:\Windows\Microsoft.NET\assembly\GAC_32",
                @"C:\Windows\Microsoft.NET\assembly\GAC_64",
                @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2",
                @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.2.0",
            };

            foreach (var dir in frameworkDirectories)
            {
                if (Directory.Exists(dir))
                {
                    resolver.AddSearchDirectory(dir);
                }
            }
        }
    }
}
