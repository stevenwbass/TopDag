using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TopDag.Graphs
{
    public abstract class AbstractDag<TKey, TData> : IGraph<TKey, TData>
    {
        public Dictionary<TKey, TData> Data { get; protected set; } = new Dictionary<TKey, TData>();

        public Dictionary<TKey, HashSet<TKey>> IncomingEdges { get; protected set; } = new Dictionary<TKey, HashSet<TKey>>();

        public Dictionary<TKey, HashSet<TKey>> OutgoingEdges { get; protected set; } = new Dictionary<TKey, HashSet<TKey>>();

        public Int32 Count => OutgoingEdges.Count;

        public TData this[TKey key] => Data[key];

        public IEnumerator<KeyValuePair<TKey, TData>> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void ShallowCopy(AbstractDag<TKey, TData> target)
        {
            target.Data = new Dictionary<TKey, TData>(this.Data);
            target.IncomingEdges = this.IncomingEdges.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TKey>(kvp.Value));
            target.OutgoingEdges = this.OutgoingEdges.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TKey>(kvp.Value));
        }

        public void AddNode(TKey key, TData data, IList<TKey> outgoing)
        {
            if (OutgoingEdges.ContainsKey(key))
                throw new ArgumentException("Node with the provided key already exists.");
            if (CausesCycle(key, outgoing))
                throw new ArgumentException("Adding this node causes a cycle.");

            Data.Add(key, data);
            AddEdges(key, outgoing);
        }

        protected void AddEdges(TKey key, IList<TKey> outgoing)
        {
            OutgoingEdges.Add(key, new HashSet<TKey>(outgoing));
            foreach (var dest in outgoing)
            {
                if (!IncomingEdges.TryGetValue(dest, out HashSet<TKey> incoming))
                    IncomingEdges[dest] = new HashSet<TKey> { key };
                else
                    incoming.Add(key);
            }
        }

        public bool CausesCycle(TKey key, IList<TKey> outgoing)
        {
            if (outgoing.Contains(key))
                return true; // Self cycle
            if (!IncomingEdges.TryGetValue(key, out HashSet<TKey> incoming) || incoming.Count == 0)
                return false; // No incoming edges, so we can't have a cycle.

            // If a path exists from any outgoing edge to any incoming edge, adding the node will cause a cycle.
            foreach (TKey start in outgoing)
            {
                foreach (TKey end in incoming)
                {
                    if (PathExists(start, end))
                        return true;
                }
            }

            return false;
        }

        public void RemoveNode(TKey key)
        {
            if (!OutgoingEdges.ContainsKey(key))
                throw new ArgumentException("Node does not exist.");

            Data.Remove(key);
            foreach (var edge in OutgoingEdges[key])
            {
                IncomingEdges[edge].Remove(key);
            }
            OutgoingEdges.Remove(key);
        }

        public void Trim(IEnumerable<TKey> topKeys)
        {
            if (topKeys == null)
                throw new ArgumentNullException(nameof(topKeys));

            Queue<TKey> unsearched = new Queue<TKey>(topKeys);
            HashSet<TKey> unconnected = new HashSet<TKey>(OutgoingEdges.Keys);
            while (unsearched.Count != 0)
            {
                TKey key = unsearched.Dequeue();
                if (!unconnected.Contains(key))
                    continue; // We have already found this key.

                // We've found the key, so we can remove it from the unconnected list.
                unconnected.Remove(key);
                foreach (TKey outgoing in OutgoingEdges[key])
                    unsearched.Enqueue(outgoing);
            }

            foreach (TKey key in unconnected)
            {
                RemoveNode(key);
            }
        }

        /// <summary>
        /// Checks if a path exists between the given start and end nodes.
        /// </summary>
        public bool PathExists(TKey start, TKey end)
        {
            HashSet<TKey> tested = new HashSet<TKey> { start };
            Queue<TKey> queued = new Queue<TKey>(tested);

            while (queued.Count > 0)
            {
                TKey current = queued.Dequeue();
                if (current.Equals(end)) // A path exists
                    return true;


                if (!OutgoingEdges.TryGetValue(current, out HashSet<TKey> destinations))
                    continue; // No out edges for current

                foreach (TKey destination in destinations)
                {
                    if (tested.Contains(destination))
                        continue;

                    tested.Add(destination);
                    queued.Enqueue(destination);
                }
            }

            return false;
        }

        public bool Contains(TKey key) => OutgoingEdges.ContainsKey(key);

        public List<TKey> GetIncoming(TKey key)
        {
            if (!IncomingEdges.TryGetValue(key, out HashSet<TKey> edges))
                return new List<TKey>();
            return edges.ToList();
        }

        /// <summary>
        /// Gets the incoming eges to a key, regardless if the node has been added or not.
        /// </summary>
        public List<TKey> GetOutgoing(TKey key)
        {
            if (!OutgoingEdges.TryGetValue(key, out HashSet<TKey> edges))
                return new List<TKey>();
            return edges.ToList();
        }
    }
}
