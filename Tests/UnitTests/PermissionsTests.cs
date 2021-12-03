using Woof.LinuxAdmin;

namespace UnitTests;

public class PermissionsTests {

    [Fact]
    public void FromInt() {
        var mode = Convert.ToUInt32("660", 8);
        var p1 = new Permissions(mode);
        Permissions p2 = mode;
        Assert.Equal(mode, p1.Mode);
        Assert.Equal(mode, p2.Mode);
        Assert.False(p1.IsRelative);
        Assert.False(p2.IsRelative);
    }

    [Fact]
    public void FromOctal() {
        var octal = "660";
        var mode = Convert.ToUInt32(octal, 8);
        var p1 = new Permissions(octal);
        Permissions p2 = octal;
        Assert.Equal(mode, p1.Mode);
        Assert.Equal(mode, p2.Mode);
        Assert.False(p1.IsRelative);
        Assert.False(p2.IsRelative);
    }

    [Fact]
    public void Relative() {
        TestRelative("00640", "o+r", "00644");
        TestRelative("00640", "g+x", "00650");
        TestRelative("00640", "ug+x", "00750");
        TestRelative("00000", "+rwx", "00666");
        TestRelative("00444", "+rwx", "00777");
        TestRelative("00000", "+rwX", "00666");
        TestRelative("00000", "u+rwx,g+rwx", "00660");
        TestRelative("00440", "u+rwx,g+rwx", "00770");
        TestRelative("00640", "+x", "00750");
        TestRelative("00777", "o-x", "00776");
        TestRelative("40660", "+X", "40770");
        TestRelative("00660", "+X", "00660");
        TestRelative("00661", "+X", "00771");
        TestRelative("00661", "u+X,o-x", "00760");
        TestRelative("40770", "-X", "40660");
        TestRelative("00770", "-X", "00660");
        TestRelative("00770", "-X", "00660");
    }

    [Fact]
    public void LsFormat() {
        Assert.Equal("----------", new Permissions("000").LsString);
        Assert.Equal("-------rwx", new Permissions("007").LsString);
        Assert.Equal("----rwxrwx", new Permissions("077").LsString);
        Assert.Equal("-rwxrwxrwx", new Permissions("777").LsString);
        Assert.Equal("drwxrwxrwx", new Permissions("40777").LsString);
        Assert.Equal("drwxr-xr--", new Permissions("40754").LsString);
    }

    [Fact]
    public void IsMatch() {
        var sample = new Permissions("740");
        Assert.True(sample.IsMatch(Permissions.Target.User, Permissions.Bits.Read));
        Assert.True(sample.IsMatch(Permissions.Target.User, Permissions.Bits.Write));
        Assert.True(sample.IsMatch(Permissions.Target.User, Permissions.Bits.Execute));
        Assert.True(sample.IsMatch(Permissions.Target.Group, Permissions.Bits.Read));
        Assert.False(sample.IsMatch(Permissions.Target.Group, Permissions.Bits.Write));
        Assert.False(sample.IsMatch(Permissions.Target.Group, Permissions.Bits.Execute));
        Assert.False(sample.IsMatch(Permissions.Target.Other, Permissions.Bits.Read));
        Assert.False(sample.IsMatch(Permissions.Target.Other, Permissions.Bits.Write));
        Assert.False(sample.IsMatch(Permissions.Target.Other, Permissions.Bits.Execute));
    }

    private static void TestRelative(Permissions original, Permissions target, Permissions expected) {
        Assert.False(original.IsRelative);
        Assert.True(target.IsRelative);
        target.Modify(original);
        Assert.False(target.IsRelative);
        Assert.Equal(expected.ToString(), target.ToString());
    }

}