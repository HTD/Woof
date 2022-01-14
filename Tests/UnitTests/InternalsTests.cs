namespace UnitTests;

public class InternalsTests {

    [Fact]
    public void A010_Trees_Traverse() {
        var root = new TreeNode<int> { Children = new(), Value = 0 };
        root.Children.Add(new() { Value = 1, Children = new() { new() { Value = 2 }, new() { Value = 3 } } });
        root.Children.Add(new() { Value = 4, Children = new() { new() { Value = 5 }, new() { Value = 6 } } });
        var result = root.Children.Traverse(n => n.Children).Select(n => n.Value).ToArray();
        var expected = new[] { 1, 2, 3, 4, 5, 6 };
        Assert.True(expected.SequenceEqual(result));
        result = Trees.Traverse(root, n => n.Children).Select(n => n.Value).ToArray();
        expected = new[] { 0, 1, 2, 3, 4, 5, 6 };
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A020_Trees_TraverseBFS() {
        var root = new TreeNode<int> { Children = new(), Value = 0 };
        root.Children.Add(new() { Value = 1, Children = new() { new() { Value = 3 }, new() { Value = 4 } } });
        root.Children.Add(new() { Value = 2, Children = new() { new() { Value = 5 }, new() { Value = 6 } } });
        var result = root.Children.TraverseBFS(n => n.Children).Select(n => n.Value).ToArray();
        var expected = new[] { 1, 2, 3, 4, 5, 6 };
        Assert.True(expected.SequenceEqual(result));
        result = Trees.TraverseBFS(root, n => n.Children).Select(n => n.Value).ToArray();
        expected = new[] { 0, 1, 2, 3, 4, 5, 6 };
        Assert.True(expected.SequenceEqual(result));
    }

    [Fact]
    public void A030_Trees_TraverseDFS() {
        var root = new TreeNode<int> { Children = new(), Value = 7 };
        root.Children.Add(new() { Value = 3, Children = new() { new() { Value = 1 }, new() { Value = 2 } } });
        root.Children.Add(new() { Value = 6, Children = new() { new() { Value = 4 }, new() { Value = 5 } } });
        var result = root.Children.TraverseDFS(n => n.Children).Select(n => n.Value).ToArray();
        var expected = new[] { 1, 2, 3, 4, 5, 6 };
        Assert.True(expected.SequenceEqual(result));
        result = Trees.TraverseDFS(root, n => n.Children).Select(n => n.Value).ToArray();
        expected = new[] { 1, 2, 3, 4, 5, 6, 7 };
        Assert.True(expected.SequenceEqual(result));
    }

}