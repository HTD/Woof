namespace Woof.Internals;

/// <summary>
/// Derived class represents a directed graph vertex and provides methods for traversing and checking for recursion.
/// </summary>
/// <typeparam name="TVertex">Derived vertex type.</typeparam>
public abstract class GraphVertex<TVertex> where TVertex : GraphVertex<TVertex> {

    /// <summary>
    /// Derived class must get the collection of vertices that directly belong to the this vertex.
    /// </summary>
    public abstract IEnumerable<TVertex> Vertices { get; }

    /// <summary>
    /// Gets a value indicating whether this vertex is a sink (has no outgoing edges).
    /// </summary>
    public bool IsSink => Vertices is IList<TVertex> l ? l.Count < 1 : !Vertices.Any();

    /// <summary>
    /// Gets the collection of vertices in Breadth First Search order.
    /// </summary>
    public IEnumerable<TVertex> BreadthFirst {
        get {
            Queue<TVertex> queue = Vertices is IList<TVertex> l1 ? new(l1.Count) : new();
            queue.Enqueue((this as TVertex)!); // Start with the current vertex
            while (queue.Count > 0) {
                var vertex = queue.Dequeue();
                yield return vertex;
                if (vertex.IsSink) continue;
                foreach (var child in vertex.Vertices) {
                    queue.Enqueue(child);
                }
            }
        }
    }

    /// <summary>
    /// Gets the collection of vertices in Depth First Search order.
    /// </summary>
    public IEnumerable<TVertex> DepthFirst {
        get {
            Stack<TVertex> stack = Vertices is IList<TVertex> l1 ? new(l1.Count) : new();
            stack.Push((this as TVertex)!);
            while (stack.Count > 0) {
                var vertex = stack.Pop();
                yield return vertex;
                if (vertex.IsSink) continue;
                foreach (var child in vertex.Vertices.Reverse()) stack.Push(child);
            }
        }
    }

    /// <summary>
    /// Tests the graph for cycles using DFS and returns all cyclic edges.
    /// </summary>
    /// <param name="graph">A collection of all graph vertices.</param>
    /// <returns>A collection of cyclic edges (pairs of vertices).</returns>
    public static IEnumerable<Tuple<TVertex, TVertex>> DetectCycles(IEnumerable<TVertex> graph) {
        var visited = new HashSet<TVertex>(); // Tracks all visited nodes
        var currentPath = new HashSet<TVertex>(); // Tracks the current DFS path
        var cycles = new List<Tuple<TVertex, TVertex>>(); // Stores cyclic edges
        foreach (var vertex in graph) {
            if (!visited.Contains(vertex)) {
                FindCycles(vertex, visited, currentPath, cycles);
            }
        }
        return cycles;
    }
    
    /// <summary>
    /// Helper method to perform DFS and detect cycles.
    /// </summary>
    /// <param name="vertex">The current vertex being visited.</param>
    /// <param name="visited">Set of all visited vertices.</param>
    /// <param name="currentPath">Set of vertices in the current DFS path.</param>
    /// <param name="cycles">List to store cyclic edges.</param>
    private static void FindCycles(
        TVertex vertex,
        HashSet<TVertex> visited,
        HashSet<TVertex> currentPath,
        List<Tuple<TVertex, TVertex>> cycles
    ) {
        visited.Add(vertex);
        currentPath.Add(vertex);
        foreach (var child in vertex.Vertices) {
            if (currentPath.Contains(child)) {
                // Cycle detected: edge from vertex to child
                cycles.Add(new Tuple<TVertex, TVertex>(vertex, child));
            }
            else if (!visited.Contains(child)) {
                // Continue DFS
                FindCycles(child, visited, currentPath, cycles);
            }
        }
        // Backtrack: remove the current vertex from the current path
        currentPath.Remove(vertex);
    }

}

/// <summary>
/// Provides static methods to traverse over directed graphs of any kind.
/// </summary>
public static class GraphNode {

    /// <summary>
    /// Traverses the graph nodes in DFS order.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="node">Tree node.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All node leaves in DFS order, this node included.</returns>
    public static IEnumerable<TNode> TraverseDepthFirst<TNode>(TNode node, Func<TNode, IEnumerable<TNode>?> children) {
        Stack<TNode> stack = [];
        stack.Push(node);
        while (stack.Count > 0) {
            var current = stack.Pop();
            yield return current;
            var items = children(current);
            if (!Any(items)) continue;
            foreach (var item in items!.Reverse()) stack.Push(item);
        }
    }

    /// <summary>
    /// Traverses the graph nodes in DFS order.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="graph">Graph to traverse.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All graph leaves in DFS order.</returns>
    public static IEnumerable<TNode> TraverseDepthFirst<TNode>(this IEnumerable<TNode> graph, Func<TNode, IEnumerable<TNode>?> children) {
        foreach (var node in graph) {
            foreach (var result in TraverseDepthFirst(node, children)) {
                yield return result;
            }
        }
    }

    /// <summary>
    /// Traverses the graph nodes in BFS order.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="node">Tree node.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All graph sinks in BFS order.</returns>
    public static IEnumerable<TNode> TraverseBreadthFirst<TNode>(TNode node, Func<TNode, IEnumerable<TNode>?> children) {
        var queue = new Queue<TNode>();
        queue.Enqueue(node);
        while (queue.Count > 0) {
            var current = queue.Dequeue();
            yield return current;
            var items = children(current);
            if (!Any(items)) continue;
            foreach (var item in items!) queue.Enqueue(item);
        }
    }

    /// <summary>
    /// Traverses the graph nodes in BFS order.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="graph">Tree to traverse.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All graph sinks in BFS order.</returns>
    public static IEnumerable<TNode> TraverseBreadthFirst<TNode>(this IEnumerable<TNode> graph, Func<TNode, IEnumerable<TNode>?> children) {
        foreach (var node in graph) {
            foreach (var result in TraverseBreadthFirst(node, children)) {
                yield return result;
            }
        }
    }

    /// <summary>
    /// Tests if the optional collection is set at all, and if it contains any items.
    /// </summary>
    /// <typeparam name="TElement">Element type.</typeparam>
    /// <param name="elements">Optional collection.</param>
    /// <returns>True if collection is set and it contains items.</returns>
    private static bool Any<TElement>(IEnumerable<TElement>? elements) =>
        elements is IList<TElement> l ? l.Count > 0 : elements?.Any() == true;

}
