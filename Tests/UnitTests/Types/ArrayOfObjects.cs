namespace UnitTests.Types;

public class ArrayOfObjects {

    public DirectSimple[] A { get; } = [new(), new(), new()];

    public DirectSimple[] B { get; set; } = [];

    public DirectSimple[]? N { get; set; }

}
