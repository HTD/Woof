namespace Woof.LinuxAdmin.Test;

/// <summary>
/// Contains functional tests.
/// </summary>
internal class Tests {

    const string BaseDirectory = "/woof/linux-admin/demo";
    const string ServiceUser = "demo";
    const string ServiceGroup = "demo";
    const string ServiceDirectory = "/woof/linux-admin/demo/service";
    static string TestFSSource => Path.Combine(ServiceDirectory, "TestFS");
    static string TestFSTarget => Path.Combine(ServiceDirectory, "TestFS1");

    /// <summary>
    /// Runs the tests.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when done.</returns>
    public static async ValueTask RunAsync() {
        if (!OS.IsLinux) {
            Console.WriteLine("It's supposed to be run on Linux.");
            return;
        }
        if (!CurrentUser.IsRoot) {
            Console.WriteLine("Use sudo.");
            return;
        }
        var sudoUser = CurrentUser.BehindSudo;
        Console.WriteLine($"Sudo user: {sudoUser}, home directory location: {sudoUser!.Directory}.");
        var exe = Process.GetCurrentProcess()?.MainModule?.FileName!;
        var stat = Linux.Stat(exe);
        if (stat is not null) {
            Console.WriteLine($"Current executable name: {exe}");
            Console.WriteLine($"OwnerId: {stat.Uid}");
            Console.WriteLine($"GroupId: {stat.Gid}");
            Console.WriteLine($"Permissions: {stat.Permissions}");
            Console.WriteLine($"Created: {stat.CreationTime.AsDateTime:yyyy-MM-dd HH:mm:ss.fff}");
            Console.WriteLine($"Accessed: {stat.AccessTime.AsDateTime:yyyy-MM-dd HH:mm:ss.fff}");
            Console.WriteLine($"Modified: {stat.LastModificationTime.AsDateTime:yyyy-MM-dd HH:mm:ss.fff}");
        }
        else Console.WriteLine("Stat for the current executable failed.");
        if (Linux.UserExists(ServiceUser)) {
            await CleanUpAsync();
            return;
        }
        Console.WriteLine("Testing Woof.LinuxAdmin module...");
        if (await Linux.AddSystemUserAsync(ServiceUser, ServiceGroup, BaseDirectory))
            Console.WriteLine($"System user \"{ServiceUser}:{ServiceGroup}\" added.");
        if (Linux.AddSystemDirectory(BaseDirectory, ServiceUser, ServiceGroup))
            Console.WriteLine($"System base directory ready.");
        var idOptuput = await new ShellCommand($"id {ServiceUser}").ExecAsync();
        Console.WriteLine($"Service user ID: \"{idOptuput.Trim()}\".");
        if (Linux.AddSystemDirectory(ServiceDirectory, ServiceUser, ServiceGroup))
            Console.WriteLine($"System service directory ready.");
        var targetFile = new FileInfo(exe);
        var targetPath = targetFile.FullName;
        var linkPath = Path.Combine(ServiceDirectory, targetFile.Name);
        if (Linux.Symlink(targetPath, linkPath))
            Console.WriteLine($"Symlink \"{targetPath} => {linkPath}\" created.");
        Console.WriteLine("Creating test file system...");
        TestFileSystem.CreateTestStructure(TestFSSource);
        Console.WriteLine("Copying test file system content...");
        Console.WriteLine(FileSystem.CopyDirectoryContent(TestFSSource, TestFSTarget) ? "OK." : "Incomplete.");
        Console.WriteLine("Changing ownership of test FS copy...");
        Console.WriteLine(Linux.ChownR(TestFSTarget, ServiceUser, ServiceGroup) ? "OK." : "Incomplete.");
        Console.WriteLine("Changing permissions of test FS copy...");
        await Task.Delay(1000);
        Console.WriteLine(Linux.ChmodR(TestFSTarget, "o-rwx") ? "OK." : "Incomplete.");
        Console.WriteLine("Completed. Run again to clean up.");
    }

    /// <summary>
    /// Cleans up after the tests.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when done.</returns>
    private static async ValueTask CleanUpAsync() {
        Console.WriteLine("Cleaning up...");
        try { Directory.Delete(BaseDirectory, recursive: true); } catch { }
        if (Linux.UserExists(ServiceUser)) await new ShellCommand($"userdel {ServiceUser}").ExecAndForgetAsync();
        if (Linux.GroupExists(ServiceGroup)) await new ShellCommand($"groupdel {ServiceUser}").ExecAndForgetAsync();
        Console.WriteLine("OK.");
    }

}