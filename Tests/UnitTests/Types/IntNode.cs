namespace UnitTests.Types;

public class IntNode {

    public int Value { get; init; }

    public List<IntNode> Children { get; } = [];

    public IntNode() { }

    public IntNode(int value) {
        Value = value;
    }

    public IntNode? At(int value) => Children.FirstOrDefault(n => n.Value == value);

    public void Add(int value) => Children.Add(new(value));


}