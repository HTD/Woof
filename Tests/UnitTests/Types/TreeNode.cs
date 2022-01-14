namespace UnitTests.Types;

public class TreeNode<T> {

    public List<TreeNode<T>>? Children { get; init; }

    public T? Value { get; init; }

}