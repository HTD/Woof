using Woof;

namespace UnitTests;

public class FileSystemTests {

    [Fact]
    public void CopyDirectoryContent() {
        TestFileSystem.CreateTestStructure("TEST");
        FileSystem.CopyDirectoryContent("TEST", "TEST1");
        Assert.True(Directory.Exists(@"TEST1\dir1\dir11\dir111"));
        Assert.True(File.Exists(@"TEST1\dir2\dir22\dir222\file2223"));
        Directory.Delete("TEST", recursive: true);
        Directory.Delete("TEST1", recursive: true);
    }

}