var nugetCacheDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages"));
var currentTarget = new DirectoryInfo(Directory.GetCurrentDirectory());
Console.WriteLine("Woof Package Cache NUKE");
Console.WriteLine(String.Empty.PadRight(79, '-'));
Console.WriteLine($"Current target: {currentTarget.FullName}");
Console.WriteLine($"Package cache target: {nugetCacheDir.FullName}");
var defaultForeground = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine($"Press any key to CLEAR package cache and PURGE ALL bin and obj directories in target.");
Console.ForegroundColor= defaultForeground;
Console.WriteLine($"Press Ctrl+C to cancel...");
Console.ReadKey(intercept: true);
var packages = nugetCacheDir.EnumerateDirectories();
var objs = currentTarget.EnumerateDirectories("obj", SearchOption.AllDirectories);
var bins = currentTarget.EnumerateDirectories("bin", SearchOption.AllDirectories);
var targets = packages.Concat(objs).Concat(bins);
foreach (var target in targets) {
    Console.WriteLine($"REMOVING {target}...");
    target.Delete(recursive: true);
}