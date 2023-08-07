using System;
using System.Collections.Generic;
using System.Linq;

/*
 * Mostly copied from ociaw's Dagger.
 * Changed to a static, parameterized method to accomodate arbitrary graph types (provided they extend AbstractDag).
 */

namespace TopDag.Graphs
{
    public static class Sorting
    {
        /// <summary>
        /// Returns the nodes topologically sorted into layers. Nodes with no outgoing edges are in the first layer,
        /// while nodes that only point to nodes in the first layer are in the second layer, and so on. Any node that
        /// points to a node that has not been added to the graph is considered detached.
        /// </summary>
        /// <returns>A tuple containing a list of layers and a list of detached keys.</returns>
        public static (List<List<TKey>> layers, List<TKey> detached) TopologicalSort<TKey, TData>(Dag<TKey, TData> graph)
        {
            // This method could probably be optimized significantly.
            // Now that I know it's called a topological sort, could use Kahn's algorithm.
            // However, we'll still need to take into account detached nodes where the nodes they point
            // to don't actually exist.

            List<List<TKey>> layers = new List<List<TKey>> { new List<TKey>() };
            List<TKey> detached = new List<TKey>();
            foreach (var kvp in graph.OutgoingEdges)
            {
                TKey key = kvp.Key;
                HashSet<TKey> destinations = graph.OutgoingEdges[key];
                if (graph.OutgoingEdges[key].Count == 0)
                    layers[0].Add(key); // If a key has no outgoing edges, it's added to the first layer
                else if (destinations.Any(dest => !graph.OutgoingEdges.ContainsKey(dest)))
                    detached.Add(key); // If a key has any outgoing edges that are not in the graph, it is considered detached.
            }

            HashSet<TKey> satisfiedKeys = new HashSet<TKey>(layers[0]);
            HashSet<TKey> unsatisfiedKeys = new HashSet<TKey>();

            while (layers[layers.Count - 1].Count > 0)
            {
                IEnumerable<TKey> candidates =
                    layers[layers.Count - 1]
                    .SelectMany(previous => graph.IncomingEdges.ContainsKey(previous) ? graph.IncomingEdges[previous] : new HashSet<TKey>())
                    .Where(key => graph.OutgoingEdges.ContainsKey(key))
                    .Concat(unsatisfiedKeys)
                    .Distinct();

                unsatisfiedKeys.Clear();

                List<TKey> currentLevel = new List<TKey>();
                foreach (TKey candidate in candidates)
                {
                    Boolean satisfied = true;
                    foreach (TKey outgoing in graph.OutgoingEdges[candidate])
                    {
                        // Check if each of the outgoing keys have been set already.
                        if (satisfiedKeys.Contains(outgoing))
                            continue;

                        // If not, then the candidate gets bumped up to the next level.
                        satisfied = false;
                        break;
                    }

                    if (!satisfied)
                    {
                        unsatisfiedKeys.Add(candidate);
                        continue;
                    }

                    currentLevel.Add(candidate);
                }

                layers.Add(currentLevel);
                foreach (var key in currentLevel)
                    satisfiedKeys.Add(key);
            }

            layers.RemoveAt(layers.Count - 1);
            detached.AddRange(unsatisfiedKeys);
            return (layers, detached);
        }
    }
}
