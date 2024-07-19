Run the Application
1. Make sure your project targets .NET Framework (e.g., .NET Framework 4.7.2).
2. Add Microsoft.Extensions.DependencyInjection via NuGet.
3. Add the log4net NuGet package


1. Logger.cs: Handles logging using log4net.
2. DllAnalyzer.cs: Uses Assembly.ReflectionOnlyLoadFrom to load assemblies in reflection-only mode. Loads referenced assemblies recursively.
3. Program.cs: Main entry point that initializes the DllAnalyzer and logs the results.
