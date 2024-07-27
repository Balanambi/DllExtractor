using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectNamespaceClassExtractor.Interfaces;
using ProjectNamespaceClassExtractor.Logging;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;

namespace ProjectNamespaceClassExtractor.Services
{
    public class AssemblyProcessor : IAssemblyProcessor
    {
        private readonly INamespaceClassExtractor _namespaceClassExtractor;
        private readonly Hashtable _namespaceClassMap;
        private readonly List<string> _failedAssemblies;

        public AssemblyProcessor(INamespaceClassExtractor namespaceClassExtractor)
        {
            _namespaceClassExtractor = namespaceClassExtractor;
            _namespaceClassMap = new Hashtable();
            _failedAssemblies = new List<string>();
        }

        public void ProcessAssemblies(string projectPath)
        {
            try
            {
                var dllFiles = Directory.GetFiles(projectPath, "*.dll", SearchOption.AllDirectories);
                foreach (var dll in dllFiles)
                {
                    try
                    {
                        var map = _namespaceClassExtractor.ExtractNamespacesAndClasses(dll);
                        foreach (DictionaryEntry entry in map)
                        {
                            if (!_namespaceClassMap.ContainsKey(entry.Key))
                            {
                                _namespaceClassMap[entry.Key] = entry.Value;
                            }
                            else
                            {
                                var existingList = (List<string>)_namespaceClassMap[entry.Key];
                                var newList = (List<string>)entry.Value;
                                existingList.AddRange(newList);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _failedAssemblies.Add(dll);
                        Log4NetConfig.Logger.Error($"Error processing assembly {dll}.", ex);
                    }
                }

                Log4NetConfig.Logger.Info("Successfully processed assemblies and extracted namespaces and classes.");
                LogFailedAssemblies();
            }
            catch (Exception ex)
            {
                Log4NetConfig.Logger.Error("Error processing assemblies.", ex);
                throw;
            }
        }

        public Hashtable GetNamespaceClassMap()
        {
            return _namespaceClassMap;
        }

        private void LogFailedAssemblies()
        {
            if (_failedAssemblies.Any())
            {
                Log4NetConfig.Logger.Info("The following assemblies failed to decompile:");
                foreach (var failedAssembly in _failedAssemblies)
                {
                    Log4NetConfig.Logger.Info(failedAssembly);
                }
            }
        }
    }
}
