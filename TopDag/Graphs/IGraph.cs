using System.Xml.Linq;

namespace TopDag.Graphs
{
    public interface IGraph<TKey, TData> : IEnumerable<KeyValuePair<TKey, TData>>
    {
        /// <summary>
        /// Adds a node with the specified key, data, and outgoing edges to the graph.
        /// </summary>
        public void AddNode(TKey key, TData data, IList<TKey> outgoing);

        /// <summary>
        /// Checks if adding the node causes a cycle.
        /// </summary>
        public bool CausesCycle(TKey key, IList<TKey> outgoing);

        /// <summary>
        /// Removes all nodes that are not reachable from any of the given nodes.
        /// </summary>
        /// <param name="topKeys">Keys to keep.</param>
        public void Trim(IEnumerable<TKey> topKeys);

        /// <summary>
        /// Removes a node and its outgoing edges.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveNode(TKey key);

        /// <summary>
        /// Checks if a path exists between the given start and end nodes.
        /// </summary>
        public bool PathExists(TKey start, TKey end);

        /// <summary>
        /// Checks if the graph contains a node with the given key.
        /// </summary>
        /// <param name="key"></param>
        public bool Contains(TKey key);

        /// <summary>
        /// Gets the incoming edges to a key, regardless if the node has been added or not.
        /// </summary>
        public List<TKey> GetIncoming(TKey key);

        /// <summary>
        /// Gets the incoming eges to a key, regardless if the node has been added or not.
        /// </summary>
        public List<TKey> GetOutgoing(TKey key);

        /// <summary>
        /// Creates a shallow copy of the graph, referencing the same keys and data as the original.
        /// </summary>
        public void ShallowCopy(AbstractDag<TKey, TData> target);
    }
}
