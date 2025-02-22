namespace UnitTests.Types;

public class IntVertex : GraphVertex<IntVertex> {

    public int Value { get; init; }

    public override List<IntVertex> Vertices { get; } = [];

    public IntVertex() { }

    public IntVertex(int value) {
        Value = value;
    }

    public IntVertex? At(int value) => Vertices.FirstOrDefault(n => n.Value == value);

    public void Add(int value) => Vertices.Add(new(value));

}
