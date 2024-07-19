using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDllAnalyzer
{
    Task<Hashtable> GetNamespacesAndClassesAsync(string folderPath);
    Task<Dictionary<string, List<string>>> GetDllDependenciesAsync(string folderPath);
}
