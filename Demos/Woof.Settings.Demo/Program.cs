var settings = new Settings();
Console.WriteLine("Loading configuration...");
await settings.LoadAsync();
Console.WriteLine(settings.IsLoaded ? "OK." : "FAILED!");
if (!settings.IsLoaded) Environment.Exit(-1);
if (!settings.ProtectedKey!.IsProtected) {
    Console.WriteLine("Unprotected data detected, protecting...");
    await settings.SaveAsync();
    Console.WriteLine("OK.");
}
else {
    Console.WriteLine("The sensitive data is already protected.");
}
Assert(settings.Uri == new Uri("https://www.codedog.pl"), "Uri");
Assert(settings.Ip?.ToString() == "13.95.20.240", "IP address");
Assert(settings.ExpectedKey!.SequenceEqual(settings.ProtectedKey!.Value), "Protected data");
Assert(settings.ExpectedKey!.SequenceEqual(settings.AkvKey!), "AKV");

static void Assert(bool testResult, string description) => Console.WriteLine($"{description}: {(testResult ? "PASSED." : "FAILED!")}");