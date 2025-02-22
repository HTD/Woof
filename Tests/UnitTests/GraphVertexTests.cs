namespace UnitTests;

public class GraphVertexTests {

    /*=================================
	Graph:
	-----------------------------------
	0
	|--1
	|  |--2
	|  |  |--3
	|  |  |--4
	|  |
	|  |--5
	|     |--6
	|
	|--7
	   |--8
	   |--9
	   
	(Collection = Root.Children)

	Root BFS:
	[0, 1, 7, 2, 5, 8, 9, 3, 4, 6]

	Collection BFS:
	[1, 2, 5, 3, 4, 6, 7, 8, 9]

	Root DFS:
	[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]

	Collection DFS:
	[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
	
	Root Reverse DFS:
	[3, 4, 2, 6, 5, 1, 8, 9, 7, 0]
	
	Collection Reverse DFS:
	[3, 4, 2, 6, 5, 1, 8, 9, 7]
	=================================*/

    private static IntVertex VertexExample() {
        IntVertex root = new();
        root.Add(1);
        root.At(1)?.Add(2);
        root.At(1)?.At(2)?.Add(3);
        root.At(1)?.At(2)?.Add(4);
        root.At(1)?.Add(5);
        root.At(1)?.At(5)?.Add(6);
        root.Add(7);
        root.At(7)?.Add(8);
        root.At(7)?.Add(9);
        return root;
    }

    private static IntNode NodeExample() {
        IntNode root = new();
        root.Add(1);
        root.At(1)?.Add(2);
        root.At(1)?.At(2)?.Add(3);
        root.At(1)?.At(2)?.Add(4);
        root.At(1)?.Add(5);
        root.At(1)?.At(5)?.Add(6);
        root.Add(7);
        root.At(7)?.Add(8);
        root.At(7)?.Add(9);
        return root;
    }

    [Fact]
    public void A010_GraphVertex_BreadthFirstSearch() {
        var root = VertexExample();
        var result = root.BreadthFirst.Select(n => n.Value).ToArray();
        int[] expected = [0, 1, 7, 2, 5, 8, 9, 3, 4, 6];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A020_GraphVertex_DepthFirstSearch() {
        var root = VertexExample();
        var result = root.DepthFirst.Select(n => n.Value).ToArray();
        int[] expected = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A030_GraphVertex_PostOrder() {
        var root = VertexExample();
        var result = root.PostOrder.Select(n => n.Value).ToArray();
        int[] expected = [3, 4, 2, 6, 5, 1, 8, 9, 7, 0];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A040_GraphNode_RootBFS() {
        var root = NodeExample();
        var result = GraphNode.TraverseBreadthFirst(root, n => n.Children).Select(n => n.Value).ToArray();
        int[] expected = [0, 1, 7, 2, 5, 8, 9, 3, 4, 6];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A050_GraphNode_CollectionBFS() {
        var root = NodeExample();
        var result = root.Children.TraverseBreadthFirst(n => n.Children).Select(n => n.Value).ToArray();
        int[] expected = [1, 2, 5, 3, 4, 6, 7, 8, 9];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A060_GraphNode_RootDFS() {
        var root = NodeExample();
        var result = GraphNode.TraverseDepthFirst(root, n => n.Children).Select(n => n.Value).ToArray();
        int[] expected = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A070_GraphNode_CollectionDFS() {
        var root = NodeExample();
        var result = root.Children.TraverseDepthFirst(n => n.Children).Select(n => n.Value).ToArray();
        int[] expected = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A080_GraphNode_RootPostOrder() {
        var root = NodeExample();
        var result = GraphNode.TraversePostOrder(root, n => n.Children).Select(n => n.Value).ToArray();
        int[] expected = [3, 4, 2, 6, 5, 1, 8, 9, 7, 0];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A090_GraphNode_CollectionPostOrder() {
        var root = NodeExample();
        var result = root.Children.TraversePostOrder(n => n.Children).Select(n => n.Value).ToArray();
        int[] expected = [3, 4, 2, 6, 5, 1, 8, 9, 7];
        Assert.True(expected.SequenceEqual(result));
    }

}