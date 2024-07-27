using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectNamespaceClassExtractor.Interfaces;
using ProjectNamespaceClassExtractor.Logging;

namespace ProjectNamespaceClassExtractor.Services
{
    public class AssemblyProcessor : IAssemblyProcessor
    {
        private readonly INamespaceClassExtractor _namespaceClassExtractor;
        private readonly Hashtable _namespaceClassMap;

        public AssemblyProcessor(INamespaceClassExtractor namespaceClassExtractor)
        {
            _namespaceClassExtractor = namespaceClassExtractor;
            _namespaceClassMap = new Hashtable();
        }

        public void ProcessAssemblies(string projectPath)
        {
            try
            {
                var dllFiles = Directory.GetFiles(projectPath, "*.dll", SearchOption.AllDirectories);
                foreach (var dll in dllFiles)
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

                Log4NetConfig.Logger.Info("Successfully processed assemblies and extracted namespaces and classes.");
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
    }
}
