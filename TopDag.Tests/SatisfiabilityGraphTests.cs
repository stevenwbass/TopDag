using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDag.Graphs;
using TopDag.Graphs.Nodes;

namespace TopDag.Tests
{
    public class SatisfiabilityGraphTests
    {
        public class DumbSatNode : SatisfiabilityNode
        {
            private int _val;
            public DumbSatNode(int satWhen1) 
            {
                _val = satWhen1;
            }

            public override bool IsSatisfied()
            {
                return _val == 1;
            }
        }

        /// <summary>
        /// Base class tests. Ensures SatisfiabilityGraph still works as a basic Dag.
        /// </summary>
        [TestClass]
        public class SatDagTests: DagTests
        {
            
            [TestInitialize]
            public new void Initialize()
            {
                Graph = new SatisfiabilityGraph<int, object>();
            }
        }

        public class SatGraphBase
        {
            public SatisfiabilityGraph<string, DumbSatNode> Graph;

            [TestInitialize]
            public void Initialize()
            {
                Graph = new SatisfiabilityGraph<string, DumbSatNode>();
            }

            public void CheckSatPath(string[] expected, List<string[]> satPaths)
            {
                var foundPath = satPaths.FirstOrDefault(path => path.SequenceEqual(expected));
                Assert.IsNotNull(foundPath);
            }
        }

        /// <summary>
        /// Tests for a top-down graph that looks like this:<br />
        /// <code>
        ///     A
        ///    / \
        ///   B   C
        ///    \ /
        ///     D
        /// </code>
        /// </summary>
        public class BasicGraphTests : SatGraphBase
        {
            [TestClass]
            public class IsSatisfiedTests : BasicGraphTests
            {
                [TestMethod]
                public void AllSatisfiedReturnsTrue()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C" });
                    Graph.AddNode("B", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("D", new DumbSatNode(1), new string[0]);

                    var sat = Graph.IsSatisfied();

                    Assert.AreEqual(true, sat);
                }

                [TestMethod]
                public void WhenCIsUnsatisfiedReturnsTrue()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C" });
                    Graph.AddNode("B", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("C", new DumbSatNode(0), new[] { "D" }); // not satisfied!
                    Graph.AddNode("D", new DumbSatNode(1), new string[0]);

                    var sat = Graph.IsSatisfied();

                    Assert.AreEqual(true, sat);
                }

                [TestMethod]
                public void WhenBIsUnsatisfiedReturnsTrue()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C" });
                    Graph.AddNode("B", new DumbSatNode(0), new[] { "D" }); // not satisfied!
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("D", new DumbSatNode(1), new string[0]);

                    var paths = Graph.FindSatisfiedPaths();

                    var sat = Graph.IsSatisfied();

                    Assert.AreEqual(true, sat);
                }

                [TestMethod]
                public void WhenDIsUnsatisfiedReturnsFalse()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C" });
                    Graph.AddNode("B", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("D", new DumbSatNode(0), new string[0]); // not satisfied!

                    var sat = Graph.IsSatisfied();

                    Assert.AreEqual(false, sat);
                }

                [TestMethod]
                public void WhenAIsUnsatisfiedReturnsFalse()
                {
                    Graph.AddNode("A", new DumbSatNode(0), new[] { "B", "C" }); // not satisfied!
                    Graph.AddNode("B", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("D", new DumbSatNode(1), new string[0]); 

                    var sat = Graph.IsSatisfied();

                    Assert.AreEqual(false, sat);
                }
            }

            [TestClass]
            public class FindSatisfiedPathsTests : BasicGraphTests
            {
                [TestMethod]
                public void AllSatisfiedReturnsTwoPaths()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C" });
                    Graph.AddNode("B", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("D", new DumbSatNode(1), new string[0]);

                    var paths = Graph.FindSatisfiedPaths();

                    Assert.AreEqual(2, paths.Count);
                    CheckSatPath(new[] { "A", "B", "D" }, paths);
                    CheckSatPath(new[] { "A", "C", "D" }, paths);
                }

                [TestMethod]
                public void WhenCIsUnsatisfiedReturnsOnePath()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C" });
                    Graph.AddNode("B", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("C", new DumbSatNode(0), new[] { "D" }); // not satisfied!
                    Graph.AddNode("D", new DumbSatNode(1), new string[0]);

                    var paths = Graph.FindSatisfiedPaths();

                    Assert.AreEqual(1, paths.Count);
                    CheckSatPath(new[] { "A", "B", "D" }, paths);
                }

                [TestMethod]
                public void WhenBIsUnsatisfiedReturnsOnePath()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C" });
                    Graph.AddNode("B", new DumbSatNode(0), new[] { "D" }); // not satisfied!
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("D", new DumbSatNode(1), new string[0]);

                    var paths = Graph.FindSatisfiedPaths();

                    Assert.AreEqual(1, paths.Count);
                    CheckSatPath(new[] { "A", "C", "D" }, paths);
                }

                [TestMethod]
                public void WhenDIsUnsatisfiedReturnsNoPaths()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C" });
                    Graph.AddNode("B", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("D", new DumbSatNode(0), new string[0]); // not satisfied!

                    var paths = Graph.FindSatisfiedPaths();

                    Assert.AreEqual(0, paths.Count);
                }

                [TestMethod]
                public void WhenAIsUnsatisfiedReturnsNoPaths()
                {
                    Graph.AddNode("A", new DumbSatNode(0), new[] { "B", "C" }); // not satisfied!
                    Graph.AddNode("B", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("D", new DumbSatNode(1), new string[0]);

                    var paths = Graph.FindSatisfiedPaths();

                    Assert.AreEqual(0, paths.Count);
                }
            }
        }

        /// <summary>
        /// Attempting to put this class through the ringer... See individual test comments for their graph layout.
        /// </summary>
        public class ComplexGraphTests : SatGraphBase
        {
            /// <summary>
            /// Tests for a top-down graph that looks like this:<br />
            /// <code>
            ///                         A
            ///                        /|\
            ///                       / | \
            ///                      /  |  \
            ///                     /   |   \   J
            ///        M   N(->K)  /    |    \ /
            ///         \  |      B     C     D
            ///          \ |     /|     |     |\
            ///            L    / |     |  K  | \
            ///             \  /  |     | /   |  \
            ///               E   F     G     H   I(->M)
            ///               
            /// Not pictured well: N has an outgoing edge to K, and I has an outgoing edge to M.
            /// </code>
            /// See individual tests for which nodes are satisfied/unsatisfied in the given test.
            /// </summary>
            [TestClass]
            public class FindSatisfiedPathsTests : ComplexGraphTests 
            {
                /// <summary>
                /// All nodes are satisfied.
                /// </summary>
                [TestMethod]
                public void ScenarioAReturnsExpectedPaths()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C", "D" });
                    Graph.AddNode("B", new DumbSatNode(1), new[] { "E", "F" });
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "G" });
                    Graph.AddNode("D", new DumbSatNode(1), new[] { "H", "I" });
                    Graph.AddNode("E", new DumbSatNode(1), new string[0]);
                    Graph.AddNode("F", new DumbSatNode(1), new string[0]);
                    Graph.AddNode("G", new DumbSatNode(1), new string[0]);
                    Graph.AddNode("H", new DumbSatNode(1), new string[0]);
                    Graph.AddNode("I", new DumbSatNode(1), new[] { "M" });
                    Graph.AddNode("J", new DumbSatNode(1), new[] { "D" });
                    Graph.AddNode("K", new DumbSatNode(1), new[] { "G" });
                    Graph.AddNode("L", new DumbSatNode(1), new[] { "E" });
                    Graph.AddNode("M", new DumbSatNode(1), new[] { "L" });
                    Graph.AddNode("N", new DumbSatNode(1), new[] { "L", "K" });

                    var satPaths = Graph.FindSatisfiedPaths();

                    Assert.AreEqual(9, satPaths.Count);
                    CheckSatPath(new[] { "A", "B", "E" }, satPaths);
                    CheckSatPath(new[] { "A", "B", "F" }, satPaths);
                    CheckSatPath(new[] { "A", "C", "G" }, satPaths);
                    CheckSatPath(new[] { "A", "D", "H" }, satPaths);
                    CheckSatPath(new[] { "A", "D", "I", "M", "L", "E" }, satPaths);
                    CheckSatPath(new[] { "J", "D", "H" }, satPaths);
                    CheckSatPath(new[] { "J", "D", "I", "M", "L", "E" }, satPaths);
                    CheckSatPath(new[] { "N", "L", "E" }, satPaths);
                    CheckSatPath(new[] { "N", "K", "G" }, satPaths);
                }

                /// <summary>
                /// All nodes are satisfied EXCEPT B, I, and J.
                /// </summary>
                [TestMethod]
                public void ScenarioBReturnsExpectedPaths()
                {
                    Graph.AddNode("A", new DumbSatNode(1), new[] { "B", "C", "D" });
                    Graph.AddNode("B", new DumbSatNode(0), new[] { "E", "F" });
                    Graph.AddNode("C", new DumbSatNode(1), new[] { "G" });
                    Graph.AddNode("D", new DumbSatNode(1), new[] { "H", "I" });
                    Graph.AddNode("E", new DumbSatNode(1), new string[0]);
                    Graph.AddNode("F", new DumbSatNode(1), new string[0]);
                    Graph.AddNode("G", new DumbSatNode(1), new string[0]);
                    Graph.AddNode("H", new DumbSatNode(1), new string[0]);
                    Graph.AddNode("I", new DumbSatNode(0), new[] { "M" });
                    Graph.AddNode("J", new DumbSatNode(0), new[] { "D" });
                    Graph.AddNode("K", new DumbSatNode(1), new[] { "G" });
                    Graph.AddNode("L", new DumbSatNode(1), new[] { "E" });
                    Graph.AddNode("M", new DumbSatNode(1), new[] { "L" });
                    Graph.AddNode("N", new DumbSatNode(1), new[] { "L", "K" });

                    var satPaths = Graph.FindSatisfiedPaths();

                    Assert.AreEqual(4, satPaths.Count);
                    CheckSatPath(new[] { "A", "C", "G" }, satPaths);
                    CheckSatPath(new[] { "A", "D", "H" }, satPaths);
                    CheckSatPath(new[] { "N", "L", "E" }, satPaths);
                    CheckSatPath(new[] { "N", "K", "G" }, satPaths);
                }
            }
        }
    }
}
