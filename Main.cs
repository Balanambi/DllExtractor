using System;
using Microsoft.Extensions.DependencyInjection;
using ProjectNamespaceClassExtractor.Interfaces;
using ProjectNamespaceClassExtractor.Logging;

namespace ProjectNamespaceClassExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            var assemblyProcessor = serviceProvider.GetService<IAssemblyProcessor>();
            var searchService = serviceProvider.GetService<ISearchService>();

            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the path to the project folder and the class file name to search.");
                return;
            }

            string projectPath = args[0];
            string classFilePath = args.Length > 1 ? args[1] : string.Empty;

            try
            {
                assemblyProcessor.ProcessAssemblies(projectPath);
                if (!string.IsNullOrEmpty(classFilePath))
                {
                    var result = searchService.SearchInClassFile(classFilePath);
                    Console.WriteLine(result ? "Class or namespace found in the file." : "No matching class or namespace found.");
                }
            }
            catch (Exception ex)
            {
                Log4NetConfig.Logger.Error("An error occurred.", ex);
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IAssemblyProcessor, AssemblyProcessor>();
            services.AddSingleton<INamespaceClassExtractor, NamespaceClassExtractor>();
            services.AddSingleton<ISearchService, SearchService>();
            Log4NetConfig.Configure();
            return services.BuildServiceProvider();
        }
    }
}
