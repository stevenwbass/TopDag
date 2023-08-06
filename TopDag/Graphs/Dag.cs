namespace TopDag.Graphs
{
    /// <summary>
    /// A directed acyclical graph. Basically, ociaw's unaltered Dagger Graph (https://github.com/ociaw/dagger/blob/master/Dagger/Graph.cs).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public class Dag<TKey, TData> : AbstractDag<TKey, TData>
    {
        public Dag() { }
    }
}
