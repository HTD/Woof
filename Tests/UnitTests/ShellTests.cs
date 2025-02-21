using Woof.Shell;

namespace UnitTests;

public class ShellTests {

    public ShellCommand DirOK { get; } = new ShellCommand("dir /N /O:GN .");

    public ShellCommand DirFail { get; } = new ShellCommand("dir nonExistingDir");

    [Fact]
    public void Exec() {
        var dir = DirOK.Exec();
        Assert.True(dir.Length > 40);
        try {
            DirFail.Exec();
            ThrowException();
        }
        catch (ShellExecException exception) {
            CheckException(exception);
        }
    }

    [Fact]
    public async Task ExecAsync() {
        var dir = await DirOK.ExecAsync();
        Assert.True(dir.Length > 40);
        try {
            await DirFail.ExecAsync();
            ThrowException();
        }
        catch (ShellExecException exception) {
            CheckException(exception);
        }
    }

    [Fact]
    public void ExecVoid() {
        DirOK.ExecVoid();
        try {
            DirFail.ExecVoid();
            ThrowException();
        }
        catch (ShellExecException exception) {
            CheckException(exception);
        }
    }

    [Fact]
    public async Task ExecVoidAsync() {
        await DirOK.ExecVoidAsync();
        try {
            await DirFail.ExecVoidAsync();
            ThrowException();
        }
        catch (ShellExecException exception) {
            CheckException(exception);
        }
    }

    [Fact]
    public void ExecAndForget() {
        DirOK.ExecAndForget();
        DirFail.ExecAndForget();
    }

    [Fact]
    public async Task ExecAndForgetAsync() {
        await DirOK.ExecAndForgetAsync();
        await DirFail.ExecAndForgetAsync();
    }

    [Fact]
    public void TryExec() {
        var d1 = DirOK.TryExec();
        var d2 = DirFail.TryExec();
        Assert.NotNull(d1);
        Assert.Null(d2);
    }

    [Fact]
    public async Task TryExecAsync() {
        var d1 = await DirOK.TryExecAsync();
        var d2 = await DirFail.TryExecAsync();
        Assert.NotNull(d1);
        Assert.Null(d2);
    }

    private static void ThrowException() => throw new InvalidOperationException("Invalid command should throw");

    private static void CheckException(ShellExecException exception) {
        Assert.Equal(1, exception.ExitCode);
        Assert.True(exception.Message.Length > 0);
        Assert.True(exception.CommandOutput?.Length > 0);
        Assert.False(exception.Message.EndsWith('\n'));
        Assert.False(exception.CommandOutput?.EndsWith('\n'));
    }

}