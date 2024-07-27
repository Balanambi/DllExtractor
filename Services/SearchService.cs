using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProjectNamespaceClassExtractor.Interfaces;
using ProjectNamespaceClassExtractor.Logging;

namespace ProjectNamespaceClassExtractor.Services
{
    public class SearchService : ISearchService
    {
        private readonly IAssemblyProcessor _assemblyProcessor;

        public SearchService(IAssemblyProcessor assemblyProcessor)
        {
            _assemblyProcessor = assemblyProcessor;
        }

        public bool SearchInClassFile(string classFilePath)
        {
            try
            {
                var namespaceClassMap = _assemblyProcessor.GetNamespaceClassMap();
                var classContent = File.ReadAllText(classFilePath);

                foreach (DictionaryEntry entry in namespaceClassMap)
                {
                    var namespaceName = entry.Key.ToString();
                    var classList = (List<string>)entry.Value;

                    if (classContent.Contains(namespaceName) || classList.Exists(c => classContent.Contains(c)))
                    {
                        Log4NetConfig.Logger.Info($"Found namespace or class match in {classFilePath}.");
                        return true;
                    }
                }

                Log4NetConfig.Logger.Info($"No namespace or class match found in {classFilePath}.");
            }
            catch (Exception ex)
            {
                Log4NetConfig.Logger.Error($"Error searching in class file {classFilePath}.", ex);
                throw;
            }

            return false;
        }
    }
}
