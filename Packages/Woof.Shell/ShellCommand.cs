namespace Woof.Shell;

/// <summary>
/// <see cref="ProcessStartInfo"/> extension to execute shell commands on both Linux and Windows.
/// </summary>
public class ShellCommand {

    /// <summary>
    /// Gets or sets a value indicating that all shell I/O should be redirected to the program.
    /// </summary>
    public bool Redirect {
        get => StartInfo.RedirectStandardOutput;
        set {
            StartInfo.RedirectStandardInput = value;
            StartInfo.RedirectStandardOutput = value;
            StartInfo.RedirectStandardError = value;
            StartInfo.StandardErrorEncoding = value ? Encoding.UTF8 : null;
            StartInfo.StandardErrorEncoding = value ? Encoding.UTF8 : null;
        }
    }

    /// <summary>
    /// Gets the configuration of the process to start.
    /// </summary>
    public ProcessStartInfo StartInfo { get; }

    /// <summary>
    /// Sets UTF-8 encoding on Windows cmd.
    /// </summary>
    static ShellCommand() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        var psi = new ProcessStartInfo {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            FileName = "cmd",
            Arguments = "/C chcp 65001"
        };
        using var process = Process.Start(psi);
    }

    /// <summary>
    /// Creates <see cref="ShellCommand"/> for a shell command.
    /// </summary>
    /// <param name="command">Shell command.</param>
    /// <exception cref="PlatformNotSupportedException">Not Linux nor Windows.</exception>
    public ShellCommand(string command) {
        var (shell, exec) =
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ("bash", "-c") :
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ("cmd", "/C") :
            throw new PlatformNotSupportedException();
        var arguments = new[] { exec, command };
        StartInfo = new ProcessStartInfo(shell, new SpaceDelimitedStringParser().Join(arguments)) { CreateNoWindow = true };
    }

    /// <summary>
    /// Creates <see cref="ShellCommand"/> for a shell command.
    /// </summary>
    /// <param name="command">Shell command without the arguments.</param>
    /// <param name="arguments">Shell command arguments collection.</param>
    /// <exception cref="PlatformNotSupportedException">Not Linux nor Windows.</exception>
    public ShellCommand(string command, IEnumerable<string> arguments) : this(Join(command, arguments)) { }

    /// <summary>
    /// Executes the shell command and returns its output.
    /// </summary>
    /// <returns>Command output.</returns>
    /// <exception cref="InvalidOperationException">Shell execute failed.</exception>
    /// <exception cref="ShellExecException">Command returned non zero exit code.</exception>
    public string Exec() {
        Redirect = true;
        using var process = Process.Start(this);
        if (process is null) throw new InvalidOperationException("Shell execute failed");
        var output = process.StandardOutput.ReadToEnd().Trim();
        var error = process.StandardError.ReadToEnd().Trim();
        process.WaitForExit();
        return process.ExitCode != 0
            ? throw new ShellExecException(error.Length > 0 ? error : null, process.ExitCode, output.Length > 0 ? output : null)
            : output;
    }

    /// <summary>
    /// Executes the shell command and returns its output.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> returning command output.</returns>
    /// <exception cref="InvalidOperationException">Shell execute failed.</exception>
    /// <exception cref="ShellExecException">Command returned non zero exit code.</exception>
    public async ValueTask<string> ExecAsync() {
        Redirect = true;
        using var process = Process.Start(this);
        if (process is null) throw new InvalidOperationException("Shell execute failed");
        var output = (await process.StandardOutput.ReadToEndAsync()).Trim();
        var error = (await process.StandardError.ReadToEndAsync()).Trim();
        await process.WaitForExitAsync();
        return process.ExitCode != 0
            ? throw new ShellExecException(error.Length > 0 ? error : null, process.ExitCode, output.Length > 0 ? output : null)
            : output;
    }

    /// <summary>
    /// Executes the shell command and throws when it fails.
    /// </summary>
    /// <exception cref="InvalidOperationException">Shell execute failed.</exception>
    /// <exception cref="ShellExecException">Command returned non zero exit code.</exception>
    public void ExecVoid() {
        Redirect = true;
        using var process = Process.Start(this);
        if (process is null) throw new InvalidOperationException("Shell execute failed");
        var output = process.StandardOutput.ReadToEnd().Trim();
        var error = process.StandardError.ReadToEnd().Trim();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new ShellExecException(error.Length > 0 ? error : null, process.ExitCode, output.Length > 0 ? output : null);
    }

    /// <summary>
    /// Executes the shell command and throws when it fails.
    /// </summary>
    /// <exception cref="InvalidOperationException">Shell execute failed.</exception>
    /// <exception cref="ShellExecException">Command returned non zero exit code.</exception>
    public async ValueTask ExecVoidAsync() {
        Redirect = true;
        using var process = Process.Start(this);
        if (process is null) throw new InvalidOperationException("Shell execute failed");
        var output = (await process.StandardOutput.ReadToEndAsync()).Trim();
        var error = (await process.StandardError.ReadToEndAsync()).Trim();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new ShellExecException(error.Length > 0 ? error : null, process.ExitCode, output.Length > 0 ? output : null);
    }

    /// <summary>
    /// Executes the shell command and ignores its output and exit code.
    /// </summary>
    /// <exception cref="InvalidOperationException">Shell execute failed.</exception>
    public void ExecAndForget() {
        Redirect = true;
        using var process = Process.Start(this);
        if (process is null) return;
        process.StandardOutput.Close();
        process.StandardError.Close();
        process.WaitForExit();
    }

    /// <summary>
    /// Executes the shell command and ignores its output and exit code.
    /// </summary>
    /// <exception cref="InvalidOperationException">Shell execute failed.</exception>
    public async ValueTask ExecAndForgetAsync() {
        Redirect = true;
        using var process = Process.Start(this);
        if (process is null) return;
        process.StandardOutput.Close();
        process.StandardError.Close();
        await process.WaitForExitAsync();
    }

    /// <summary>
    /// Tries to execute the shell command and return its output.
    /// </summary>
    /// <returns>Command output or null if failed.</returns>
    public string? TryExec() {
        Redirect = true;
        using var process = Process.Start(this);
        if (process is null) return null;
        var output = process.StandardOutput.ReadToEnd().Trim();
        process.StandardError.Close();
        process.WaitForExit();
        return (process.ExitCode == 0 && output.Length > 0) ? output : null;
    }

    /// <summary>
    /// Tries to execute the shell command and return its output.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> returning command output or null if failed.</returns>
    public async ValueTask<string?> TryExecAsync() {
        Redirect = true;
        using var process = Process.Start(this);
        if (process is null) return null;
        var output = (await process.StandardOutput.ReadToEndAsync()).Trim();
        process.StandardError.Close();
        await process.WaitForExitAsync();
        return (process.ExitCode == 0 && output.Length > 0) ? output : null;
    }

    /// <summary>
    /// Implicitly converts <see cref="ShellCommand"/> to <see cref="ProcessStartInfo"/>.
    /// </summary>
    /// <param name="startInfo"></param>
    public static implicit operator ProcessStartInfo(ShellCommand startInfo) => startInfo.StartInfo;

    #region Helpers

    /// <summary>
    /// Joins the separate command and arguments into a single command line.
    /// </summary>
    /// <param name="command">Command without arguments.</param>
    /// <param name="arguments">Arguments collection.</param>
    /// <returns>Command line.</returns>
    private static string Join(string command, IEnumerable<string> arguments)
        => new SpaceDelimitedStringParser().Join(Enumerable.Empty<string>().Append(command).Concat(arguments));

    #endregion

}