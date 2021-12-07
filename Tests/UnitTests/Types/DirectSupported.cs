namespace UnitTests.Types;

public class DirectSupported {

    public string? String { get; set; }

    public TestEnum Enum { get; set; }

    public bool Boolean { get; set; }

    public byte Byte { get; set; }

    public sbyte SByte { get; set; }

    public short Short { get; set; }

    public ushort UShort { get; set; }

    public int Int { get; set; }

    public uint UInt { get; set; }

    public long Long { get; set; }

    public ulong ULong { get; set; }

    public float Float { get; set; }

    public double Double { get; set; }

    public Decimal Decimal { get; set; }

    public DateTime DateTime { get; set; }

    public DateOnly DateOnly { get; set; }

    public TimeSpan TimeSpan { get; set; }

    public TimeOnly TimeOnly { get; set; }

    public Guid Guid { get; set; }

    public Uri? Uri { get; set; }

    public FileInfo? FileInfo { get; set; }

    public DirectoryInfo? DirectoryInfo { get; set; }

    public byte[]? Key { get; set; }

    public static DirectSupported Default => new() {
        String = "0.1",
        Enum = TestEnum.One,
        Boolean = true,
        Byte = byte.MaxValue,
        SByte = sbyte.MinValue,
        Short = short.MinValue,
        UShort = ushort.MaxValue,
        Int = int.MinValue,
        UInt = uint.MaxValue,
        Long = long.MinValue,
        ULong = ulong.MaxValue,
        Float = (float)Math.E,
        Double = Math.PI,
        Decimal = (decimal)Math.Pow(2, 0.5) ,
        DateTime = DateTime.Parse("1986-04-26 01:23:40"),
        DateOnly = DateOnly.Parse("1986-04-26"),
        TimeSpan = TimeSpan.FromSeconds(0.1),
        TimeOnly = TimeOnly.Parse("01:23:40"),
        Guid = new Guid("6d5593fd-fb43-4853-9260-bf4e33ec9615"),
        Uri = new Uri("https://www.codedog.pl/"),
        FileInfo = new FileInfo("test.txt"),
        DirectoryInfo = new DirectoryInfo("test"),
        Key = Convert.FromBase64String("yrK353x8uuPigdI0lMQwGYQ7LMGudOtljtMs1nxCeE8=")
    };

    public void AssertEqual(DirectSupported other) {
        Assert.Equal(other.String, String);
        Assert.Equal(other.Enum, Enum);
        Assert.Equal(other.Boolean, Boolean);
        Assert.Equal(other.Byte, Byte);
        Assert.Equal(other.SByte, SByte);
        Assert.Equal(other.Short, Short);
        Assert.Equal(other.UShort, UShort);
        Assert.Equal(other.Int, Int);
        Assert.Equal(other.UInt, UInt);
        Assert.Equal(other.Long, Long);
        Assert.Equal(other.ULong, ULong);
        Assert.Equal(other.Float, Float);
        Assert.Equal(other.Double, Double);
        Assert.Equal(other.Decimal, Decimal);
        Assert.Equal(other.DateTime, DateTime);
        Assert.Equal(other.DateOnly, DateOnly);
        Assert.Equal(other.Guid, Guid);
        Assert.Equal(other.Uri!.OriginalString, Uri!.OriginalString);
        Assert.Equal(other.FileInfo!.FullName, FileInfo!.FullName);
        Assert.Equal(other.DirectoryInfo!.FullName, DirectoryInfo!.FullName);
        Assert.True(other.Key!.SequenceEqual(Key!));
    }

}
