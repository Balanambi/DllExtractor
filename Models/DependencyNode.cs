using System.Collections.Generic;

namespace TransitiveDependencyFinder
{
    public class DependencyNode
    {
        public string AssemblyPath { get; set; }
        public List<DependencyNode> Dependencies { get; set; } = new List<DependencyNode>();

        public DependencyNode(string assemblyPath)
        {
            AssemblyPath = assemblyPath;
        }

        public override string ToString()
        {
            return AssemblyPath;
        }
    }
}
