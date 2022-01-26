namespace UnitTests.Types;

#pragma warning disable IDE0051
public class Tricky {

    public int Regular { get; set; }

    public int Default { get; set; } = 1;

    private int Private { get; set; }

    public static int Static { get; set; }

    public int Init { get; init; }

    internal int Internal { get; set; }

    [Internal]
    public int Internal1 { get; set; }

    public int Field;

    public int ReadOnly { get; }

}
#pragma warning restore IDE0051