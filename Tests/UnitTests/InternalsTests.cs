namespace UnitTests;

public class InternalsTests {

    [Fact]
    public void A010_Trees_Traverse() {
        var root = new TreeNode<int> { Children = [], Value = 0 };
        root.Children.Add(new() { Value = 1, Children = [new() { Value = 2 }, new() { Value = 3 }] });
        root.Children.Add(new() { Value = 4, Children = [new() { Value = 5 }, new() { Value = 6 }] });
        var result = root.Children.TraverseDepthFirst(n => n.Children).Select(n => n.Value).ToArray();
        var expected = new[] { 1, 2, 3, 4, 5, 6 };
        Assert.True(expected.SequenceEqual(result));
        result = GraphNode.TraverseDepthFirst(root, n => n.Children).Select(n => n.Value).ToArray();
        expected = [0, 1, 2, 3, 4, 5, 6];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A020_Trees_TraverseBFS() {
        var root = new TreeNode<int> { Children = [], Value = 0 };
        root.Children.Add(new() { Value = 1, Children = [new() { Value = 3 }, new() { Value = 4 }] });
        root.Children.Add(new() { Value = 2, Children = [new() { Value = 5 }, new() { Value = 6 }] });
        var result = root.Children.TraverseBreadthFirst(n => n.Children).Select(n => n.Value).ToArray();
        var expected = new[] { 1, 2, 3, 4, 5, 6 };
        Assert.True(expected.SequenceEqual(result));
        result = GraphNode.TraverseBreadthFirst(root, n => n.Children).Select(n => n.Value).ToArray();
        expected = [0, 1, 2, 3, 4, 5, 6];
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A030_Trees_TraverseDFS() {
        var root = new TreeNode<int> { Children = [], Value = 7 };
        root.Children.Add(new() { Value = 3, Children = [new() { Value = 1 }, new() { Value = 2 }] });
        root.Children.Add(new() { Value = 6, Children = [new() { Value = 4 }, new() { Value = 5 }] });
        var result = root.Children.TraverseDepthFirst(n => n.Children).Select(n => n.Value).ToArray();
        var expected = new[] { 1, 2, 3, 4, 5, 6 };
        Assert.True(expected.SequenceEqual(result));
        result = GraphNode.TraverseDepthFirst(root, n => n.Children).Select(n => n.Value).ToArray();
        expected = [1, 2, 3, 4, 5, 6, 7];
        Assert.True(expected.SequenceEqual(result));
    }

}