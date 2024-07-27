using System.Collections;

namespace ProjectNamespaceClassExtractor.Interfaces
{
    public interface INamespaceClassExtractor
    {
        Hashtable ExtractNamespacesAndClasses(string assemblyPath);
    }
}
