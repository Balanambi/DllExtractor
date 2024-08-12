using System.Collections.Generic;

namespace TransitiveDependencyFinder
{
    public interface IDependencyService
    {
        Dictionary<string, List<string>> GetTransitiveDependencies(string assemblyPath);
        bool IsDllInTransitiveDependencies(string assemblyPath, string dllName);
    }
}
