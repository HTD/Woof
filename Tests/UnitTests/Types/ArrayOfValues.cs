namespace UnitTests.Types;

public class ArrayOfValues {

    public int[] A { get; } = [0, 0, 0];

    public int[] B { get; set; } = Array.Empty<int>();

    public int[]? N { get; set; }

}