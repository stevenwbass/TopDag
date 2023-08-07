using TopDag.Graphs;

namespace TopDag.Tests
{
    /// <summary>
    /// This is basically copied from ociaw's graph tests, updated to reflect refactors: https://github.com/ociaw/dagger/blob/master/Tests/GraphTests.cs
    /// </summary>
    [TestClass]
    public class DagTests
    {
        /// <summary>
        /// TData is an object to allow other test classes to extend this one, ensuring all base cases are covered by inheriting classes when relevant.
        /// </summary>
        protected Dag<int, object> Graph;
        
        [TestInitialize] public void Initialize() 
        {
            Graph = new Dag<int, object>();
        }

        [TestMethod]
        public void TestMultiple()
        {
            Graph.AddNode(1, 1, new[] { 2, 3, 4 });
            Graph.AddNode(2, 4, new[] { 4 });
            Graph.AddNode(3, 9, new[] { 2, 4 });
            Graph.AddNode(4, 16, new int[0]);

            var (layers, detached) = Sorting.TopologicalSort(Graph);

            Assert.AreEqual(4, layers.Count);
            Assert.AreEqual(0, detached.Count);
            Assert.AreEqual(16, Graph[layers[0][0]]);
            Assert.AreEqual(4, Graph[layers[1][0]]);
            Assert.AreEqual(9, Graph[layers[2][0]]);
            Assert.AreEqual(1, Graph[layers[3][0]]);
        }

        [TestMethod]
        public void TestBranch()
        {
            Graph.AddNode(7, 1, new[] { 4, 6 });
            Graph.AddNode(5, 1, new[] { 1, 4 });
            Graph.AddNode(4, 1, new[] { 2, 3 });
            Graph.AddNode(3, 1, new[] { 1, 2 });
            Graph.AddNode(2, 1, new[] { 1 });
            Graph.AddNode(1, 1, new int[0]);
            Graph.AddNode(6, 1, new[] { 5, 4 });

            var incoming = Graph.GetIncoming(1);
            var outgoing = Graph.GetOutgoing(3);

            Assert.AreEqual(1, Graph[1]);
            Assert.AreEqual(3, incoming.Count);
            Assert.AreEqual(3, incoming.Distinct().Count());
            Assert.AreEqual(2, outgoing.Count);
            Assert.AreEqual(2, outgoing.Distinct().Count());
        }

        [TestMethod]
        public void TestFlat()
        {
            Graph.AddNode(1, 1, new int[0]);
            Graph.AddNode(2, 1, new int[0]);
            Graph.AddNode(3, 1, new int[0]);
            Graph.AddNode(4, 1, new int[0]);

            var (layers, detached) = Sorting.TopologicalSort(Graph);
            var incoming = Graph.GetIncoming(1);
            var outgoing = Graph.GetOutgoing(0);

            Assert.AreEqual(1, layers.Count);
            Assert.AreEqual(0, detached.Count);
            Assert.AreEqual(4, layers[0].Count);
            Assert.AreEqual(0, incoming.Count);
            Assert.AreEqual(0, outgoing.Count);
        }

        [TestMethod]
        public void TestDuplicate()
        {
            Graph.AddNode(1, 1, new[] { 2 });
            Graph.AddNode(2, 1, new[] { 3 });
            Graph.AddNode(3, 1, new[] { 4 });
            Graph.AddNode(4, 1, new[] { 5 });

            Assert.ThrowsException<ArgumentException>(() => Graph.AddNode(1, 1, new[] { 5 }));
        }

        [TestMethod]
        public void TestCycle()
        {
            Graph.AddNode(1, 1, new[] { 2 });
            Graph.AddNode(2, 1, new[] { 3 });
            Graph.AddNode(3, 1, new[] { 4 });
            Graph.AddNode(4, 1, new[] { 5 });

            Assert.ThrowsException<ArgumentException>(() => Graph.AddNode(5, 1, new[] { 1 }));
            Assert.ThrowsException<ArgumentException>(() => Graph.AddNode(5, 1, new[] { 5 }));
        }

        [TestMethod]
        public void TestDetached()
        {
            Graph.AddNode(1, 1, new int[0]);
            Graph.AddNode(2, 1, new[] { 1 });
            Graph.AddNode(3, 1, new[] { 2 });
            Graph.AddNode(4, 1, new[] { 10 });
            Graph.AddNode(5, 1, new[] { 3, 4 });

            var (layers, detached) = Sorting.TopologicalSort(Graph);

            Assert.AreEqual(3, layers.Count);
            Assert.AreEqual(2, detached.Count);
        }

        [TestMethod]
        public void TestAllDetached()
        {
            Graph.AddNode(1, 1, new[] { 10 });
            Graph.AddNode(2, 1, new[] { 11 });
            Graph.AddNode(3, 1, new[] { 12 });
            Graph.AddNode(4, 1, new[] { 13 });

            var (layers, detached) = Sorting.TopologicalSort(Graph);

            Assert.AreEqual(0, layers.Count);
            Assert.AreEqual(4, detached.Count);
        }

        [TestMethod]
        public void TestTrim()
        {
            Graph.AddNode(1, 1, new[] { 2, 3, 4 });
            Graph.AddNode(2, 4, new[] { 4 });
            Graph.AddNode(3, 9, new[] { 2, 4 });
            Graph.AddNode(4, 16, new int[0]);

            var clone = new Dag<int, object>();
            Graph.ShallowCopy(clone);
            clone.Trim(new[] { 3 });

            var (originalLayers, _) = Sorting.TopologicalSort(Graph);
            var (layers, detached) = Sorting.TopologicalSort(clone);

            Assert.AreEqual(4, Graph.Count);
            Assert.AreEqual(4, originalLayers.Count);
            Assert.AreEqual(3, clone.Count);
            Assert.AreEqual(3, layers.Count);
            Assert.AreEqual(0, detached.Count);
            Assert.ThrowsException<ArgumentNullException>(() => clone.Trim(null));
            Assert.ThrowsException<ArgumentException>(() => clone.RemoveNode(100));
        }
    }
}