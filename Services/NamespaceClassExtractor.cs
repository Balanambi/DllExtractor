using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
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
                var resolver = new UniversalAssemblyResolver(assemblyPath, true, GetTargetFrameworkId(assemblyPath));
                resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
                AddFrameworkSearchDirectories(resolver);

                var decompiler = new CSharpDecompiler(assemblyPath, resolver, new DecompilerSettings());
                var typeSystem = decompiler.TypeSystem;

                foreach (var type in typeSystem.MainModule.TypeDefinitions)
                {
                    if (type.Accessibility != Accessibility.Public)
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
            var frameworkDirectories = new List<string>
            {
                @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL",
                @"C:\Windows\Microsoft.NET\assembly\GAC_32",
                @"C:\Windows\Microsoft.NET\assembly\GAC_64",
                @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework",
                @"C:\Program Files\dotnet\shared"
            };

            // Add .NET Core directories dynamically
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var netCoreDir = Path.Combine(programFiles, "dotnet", "shared");
            if (Directory.Exists(netCoreDir))
            {
                foreach (var dir in Directory.GetDirectories(netCoreDir))
                {
                    frameworkDirectories.Add(dir);
                }
            }

            foreach (var dir in frameworkDirectories)
            {
                if (Directory.Exists(dir))
                {
                    resolver.AddSearchDirectory(dir);
                }
            }
        }

        private string GetTargetFrameworkId(string assemblyPath)
        {
            var resolver = new UniversalAssemblyResolver(assemblyPath, true, null);
            var module = new PEFile(assemblyPath, PEStreamOptions.PrefetchEntireImage, metadataOptions: MetadataReaderOptions.ApplyWindowsRuntimeProjections);
            var metadataReader = module.Metadata;

            var assemblyDefinition = metadataReader.GetAssemblyDefinition();
            var customAttributes = assemblyDefinition.GetCustomAttributes().Select(handle => metadataReader.GetCustomAttribute(handle));

            foreach (var attribute in customAttributes)
            {
                var ctorHandle = attribute.Constructor;
                StringHandle nameHandle = default;

                switch (ctorHandle.Kind)
                {
                    case HandleKind.MethodDefinition:
                        var methodDef = metadataReader.GetMethodDefinition((MethodDefinitionHandle)ctorHandle);
                        nameHandle = methodDef.Name;
                        break;
                    case HandleKind.MemberReference:
                        var memberRef = metadataReader.GetMemberReference((MemberReferenceHandle)ctorHandle);
                        nameHandle = memberRef.Name;
                        break;
                    default:
                        continue;
                }

                var attributeName = metadataReader.GetString(nameHandle);
                if (attributeName == "TargetFrameworkAttribute")
                {
                    var value = metadataReader.GetBlobReader(attribute.Value).ReadSerializedString();
                    return value;
                }
            }

            // Default to .NETCoreApp if the target framework attribute is not found
            return ".NETCoreApp,Version=v2.0";
        }
    }
}
