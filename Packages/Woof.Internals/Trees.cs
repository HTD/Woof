namespace Woof.Internals;

/// <summary>
/// Contains universal tree traversing methods for any collection.
/// </summary>
public static class Trees {

    /// <summary>
    /// Traverses the tree nodes in semi-DFS order, returning parent containers before child nodes.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="tree">Tree to traverse.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All tree leaves in DFS order.</returns>
    public static IEnumerable<TNode> Traverse<TNode>(this IEnumerable<TNode> tree, Func<TNode, IEnumerable<TNode>?> children) {
        Stack<TNode> stack = tree is IList list ? new(list.Count) : new();
        foreach (var node in tree.Reverse()) stack.Push(node);
        while (stack.Count > 0) {
            var current = stack.Pop();
            yield return current;
            var items = children(current);
            if (items is null) continue;
            foreach (var item in items.Reverse()) stack.Push(item);
        }
    }

    /// <summary>
    /// Traverses the tree nodes in semi-DFS order, returning parent containers before child nodes.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="node">Tree node.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All node leaves in DFS order, this node included.</returns>
    public static IEnumerable<TNode> Traverse<TNode>(TNode node, Func<TNode, IEnumerable<TNode>?> children) {
        var stack = new Stack<TNode>();
        stack.Push(node);
        while (stack.Count > 0) {
            var current = stack.Pop();
            yield return current;
            var items = children(current);
            if (items is null) continue;
            foreach (var item in items.Reverse()) stack.Push(item);
        }
    }

    /// <summary>
    /// Traverses the tree nodes in BFS order.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="tree">Tree to traverse.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All tree leaves in BFS order.</returns>
    public static IEnumerable<TNode> TraverseBFS<TNode>(this IEnumerable<TNode> tree, Func<TNode, IEnumerable<TNode>?> children) {
        Queue<TNode> queue = tree is IList list ? new(list.Count) : new();
        foreach (var node in tree) queue.Enqueue(node);
        while (queue.Count > 0) {
            var current = queue.Dequeue();
            yield return current;
            var items = children(current);
            if (items is null) continue;
            foreach (var item in items) queue.Enqueue(item);
        }
    }

    /// <summary>
    /// Traverses the tree nodes in BFS order.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="node">Tree node.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All tree leaves in BFS order.</returns>
    public static IEnumerable<TNode> TraverseBFS<TNode>(TNode node, Func<TNode, IEnumerable<TNode>?> children) {
        var queue = new Queue<TNode>();
        queue.Enqueue(node);
        while (queue.Count > 0) {
            var current = queue.Dequeue();
            yield return current;
            var items = children(current);
            if (items is null) continue;
            foreach (var item in items) queue.Enqueue(item);
        }
    }

    /// <summary>
    /// Traverses the tree nodes in DFS order.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="tree">Tree to traverse.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All tree leaves in DFS order.</returns>
    public static IEnumerable<TNode> TraverseDFS<TNode>(this IEnumerable<TNode> tree, Func<TNode, IEnumerable<TNode>?> children) {
        Stack<(TNode node, bool isVisited)> stack = tree is IList list ? new(list.Count) : new();
        foreach (var node in tree.Reverse()) stack.Push((node, false));
        while (stack.Count > 0) {
            var (node, isVisited) = stack.Pop();
            if (isVisited) {
                yield return node;
                continue;
            }
            var items = children(node);
            if (items is null) {
                yield return node;
                continue;
            }
            stack.Push((node, true));
            foreach (var item in items.Reverse()) stack.Push((item, false));
        }
    }

    /// <summary>
    /// Traverses the tree nodes in DFS order.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <param name="node">Tree node.</param>
    /// <param name="children">A function returning child nodes of a node or null if the node doesn't contain child nodes.</param>
    /// <returns>All tree leaves in DFS order.</returns>
    public static IEnumerable<TNode> TraverseDFS<TNode>(TNode node, Func<TNode, IEnumerable<TNode>?> children) {
        Stack<(TNode node, bool isVisited)> stack = new();
        stack.Push((node, false));
        while (stack.Count > 0) {
            var current = stack.Pop();
            if (current.isVisited) {
                yield return current.node;
                continue;
            }
            var items = children(current.node);
            if (items is null) {
                yield return current.node;
                continue;
            }
            stack.Push((current.node, true));
            foreach (var item in items.Reverse()) stack.Push((item, false));
        }
    }

}