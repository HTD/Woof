namespace UnitTests.Types;

public class ArrayOfObjects {

    public DirectSimple[] A { get; } = [new(), new(), new()];

    public DirectSimple[] B { get; set; } = Array.Empty<DirectSimple>();

    public DirectSimple[]? N { get; set; }

}
