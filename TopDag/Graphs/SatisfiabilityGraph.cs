using TopDag.Graphs.Nodes;

namespace TopDag.Graphs
{
    /// <summary>
    /// For use with a user-provided class that extends <see cref="Nodes.SatisfiabilityNode"/>.<br />
    /// Will throw exceptions if <typeparamref name="TData"/> cannot be cast to <see cref="SatisfiabilityNode"/>! This is by design.<br />
    /// <br />
    /// Given a DAG with nodes that can evaluate whether they are "satisfied", the SatisfiabilityGraph can find paths through the DAG<br />
    /// that are completely satisfied (all nodes return true from <see cref="Nodes.SatisfiabilityNode.IsSatisfied"/>).<br />
    /// <br />
    /// Has methods to find all satisfied paths (<see cref="FindSatisfiedPaths"/>)<br />
    /// or to find out if any satisfied paths exist (<see cref="IsSatisfied"/>).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public class SatisfiabilityGraph<TKey, TData> : Dag<TKey, TData> where TData : class
    {
        public SatisfiabilityGraph()
        { }

        public bool IsSatisfied()
        {
            return FindSatisfiedPaths().Any();
        }

        /// <summary>
        /// Topologically sorts the graph, then visits nodes in each layer, executing the user-supplied satisfiability check.<br />
        /// Nodes in subsequent layers will only be visited if they have an outgoing path to a satisfied node in the previous layer.
        /// </summary>
        /// <returns>
        /// Completely satisfied paths through the graph. If no such paths exist, returns an empty list.<br />
        /// Paths are returned "top down" i.e. furthest-ancestor nodes are first and leaf nodes are last.
        /// </returns>
        public List<TKey[]> FindSatisfiedPaths()
        {
            var result = new List<TKey[]>();

            var (layers, detached) = Sorting.TopologicalSort(this);
            AddSatPathsForDetachedKeys(result, detached);
            AddSatPathsForLayers(result, layers);

            return result;
        }

        private void AddSatPathsForLayers(List<TKey[]> result, List<List<TKey>> layers)
        {
            // hashset has O(1) lookups so we don't have to traverse the result object to know if the current node has an outgoing edge to a satisfied node
            var satNodeKeys = new HashSet<TKey>();

            foreach (var layer in layers)
            {
                foreach (var key in layer)
                {
                    AddKeyIfSat(satNodeKeys, key);
                }
            }

            var satPaths = FindSatPathsForLayers(satNodeKeys);

            // append sat-paths to result
            result.AddRange(satPaths.Select(x => x.ToArray()));
        }

        private List<List<TKey>> FindSatPathsForLayers(HashSet<TKey> satNodeKeys)
        {
            // find sat-paths in the layers
            var satPaths = new List<List<TKey>>();

            var rootNodes = satNodeKeys.Where(key => !this.IncomingEdges.ContainsKey(key) || !this.IncomingEdges[key].Any());

            foreach(var rootNode in rootNodes)
            {
                satPaths.AddRange(FindSatPathsForRootNode(rootNode, satNodeKeys));
            }

            return satPaths;
        }

        private List<List<TKey>> FindSatPathsForRootNode(TKey? rootNode, HashSet<TKey> satNodeKeys)
        {
            // this should never happen, but handle it gracefully just in case
            if (rootNode == null) return new List<List<TKey>>();

            var satPathsForRootNode = new List<List<TKey>>();
            var pathStack = new Stack<TKey>();
            pathStack.Push(rootNode);
            var visitedChildNodes = new Dictionary<TKey, List<TKey>>();
            
            while (pathStack.Count() > 0)
            {
                var currentNode = pathStack.Peek();
                var (anyNextNodes, nextSatNode) = GetNextSatNode(satNodeKeys, visitedChildNodes, currentNode);

                if (nextSatNode != null)
                {
                    pathStack.Push(nextSatNode);
                }
                else
                {
                    if (!anyNextNodes)
                    {
                        var satPath = pathStack.ToList();
                        satPath.Reverse(); // so that sat-paths can be traced through the graph without reading them backwards
                        satPathsForRootNode.Add(satPath);
                    }
                    var childNode = pathStack.Pop();
                    var parentNode = pathStack.Count > 0 ? pathStack.Peek() : default;
                    if (parentNode != null)
                    {
                        if (visitedChildNodes.ContainsKey(parentNode))
                            visitedChildNodes[parentNode].Add(childNode);
                        else
                            visitedChildNodes.Add(parentNode, new List<TKey> { childNode });
                    }
                }
            }

            return satPathsForRootNode;
        }

        private (bool, TKey?) GetNextSatNode(HashSet<TKey> satNodeKeys, Dictionary<TKey, List<TKey>> visitedChildNodes, TKey? currentNode)
        {
            var anyNextNodes = this.OutgoingEdges[currentNode].Any();
            var outGoingEdgesToIgnore = visitedChildNodes.ContainsKey(currentNode) ? visitedChildNodes[currentNode] : new List<TKey>();
            var nextSatNode = this.OutgoingEdges[currentNode].FirstOrDefault(
                key => satNodeKeys.Contains(key) && !outGoingEdgesToIgnore.Contains(key));

            return (anyNextNodes, nextSatNode);
        }

        private void AddKeyIfSat(HashSet<TKey> satNodeKeys, TKey? key)
        {
            // a node in any layer (except the first) must have at last one outgoing edge to a satisfied node to be part of a sat-path
            if (this.OutgoingEdges[key].Any(childNodeKey => satNodeKeys.Contains(childNodeKey)) ||
                // first-layer nodes have no outgoing edges and get a free pass to get sat-checked
                !this.OutgoingEdges[key].Any())
            {
                if ((this[key] as SatisfiabilityNode).IsSatisfied())
                {
                    satNodeKeys.Add(key);
                }
            }
        }

        private void AddSatPathsForDetachedKeys(List<TKey[]> result, List<TKey> detached)
        {
            foreach (var detachedKey in detached)
            {
                if ((this[detachedKey] as SatisfiabilityNode).IsSatisfied())
                {
                    result.Add(new TKey[1] { detachedKey });
                }
            }
        }

        private SatisfiabilityNode GetSatNodeForKey(TKey key)
        {
            var satNode = this[key] as SatisfiabilityNode;

            if (satNode == null)
                throw new InvalidCastException($"{nameof(TKey)} must extend {nameof(SatisfiabilityNode)}!");

            return satNode;
        }
    }
}