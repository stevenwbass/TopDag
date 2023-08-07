# TopDag
An extensible library for Directed Acyclic Graphs targeting .NET Standard.

### Features
* Check for path existence
* Cycle detection
* Topological sorting
* Satisfiability graph
* Extensible -- inherit/override existing classes as you see fit

### Examples
* Basic directed acyclical graph: See [DagTests](https://github.com/stevenwbass/TopDag/blob/main/TopDag.Tests/DagTests.cs).
* Satisfiability graph: See [SatisfiabilityGraphTests](https://github.com/stevenwbass/TopDag/blob/main/TopDag.Tests/SatisfiabilityGraphTests.cs).

### Goals
* Provide algorithm for graph traversal with arbitrary operations at each node/edge visit (with cumulative state).
* Better Examples

Based on [ociaw's Dagger](https://github.com/ociaw/dagger).
