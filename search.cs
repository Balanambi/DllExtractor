using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NamespaceClassSearch
{
    class Search
    {
        static Dictionary<string, bool> execute(Hashtable namespacesAndClasses,string projectDirectory)
        {
            // Sample hashtable data
           // var namespacesAndClasses = new Dictionary<string, string>
         //   {
         //       { "MyNamespace", "MyClass" },
         //       { "AnotherNamespace", "AnotherClass" }
          //  };

            // Project directory
           // string projectDirectory = @"C:\Path\To\Your\Project";

            // Initialize results dictionary
            var results = new Dictionary<string, bool>();

            foreach (var entry in namespacesAndClasses)
            {
                results[entry.Key + "." + entry.Value] = false;
            }

            try
            {
                // Traverse all files in the project directory with parallel processing
                ParallelTraverseFiles(projectDirectory, namespacesAndClasses, results);

                // Output results
                foreach (var result in results)
                {
                    Console.WriteLine($"{result.Key} is used: {result.Value}");
                }
              return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ParallelTraverseFiles(string directory, Dictionary<string, string> namespacesAndClasses, Dictionary<string, bool> results)
        {
            var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            Parallel.ForEach(files, file =>
            {
                SearchFile(file, namespacesAndClasses, results);
            });
        }

        static void SearchFile(string file, Dictionary<string, string> namespacesAndClasses, Dictionary<string, bool> results)
        {
            try
            {
                using (var reader = new StreamReader(file))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        foreach (var entry in namespacesAndClasses)
                        {
                            string namespacePattern = $@"\bnamespace\s+{entry.Key}\b";
                            string classPattern = $@"\bclass\s+{entry.Value}\b";

                            var namespaceRegex = new Regex(namespacePattern, RegexOptions.Compiled);
                            var classRegex = new Regex(classPattern, RegexOptions.Compiled);

                            if (namespaceRegex.IsMatch(line) || classRegex.IsMatch(line))
                            {
                                lock (results)
                                {
                                    results[entry.Key + "." + entry.Value] = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(file, ex);
            }
        }

        static void LogError(string file, Exception ex)
        {
            string logFile = "error_log.txt";
            string message = $"Error processing file {file}: {ex.Message}{Environment.NewLine}";
            File.AppendAllText(logFile, message);
        }
    }
}
