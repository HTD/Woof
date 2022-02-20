//
// This file (and project) can be used as a template for services.
// It tests all advanced features like mult-platform support, Azure Key Vault access and multi-platform data protection.
//
ServiceInstaller.AssertAdmin(args);
await Settings.Default.LoadAsync();
ServiceMetadata serviceSettings = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? Settings.Default.WindowsService : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
    ? Settings.Default.SystemDaemon : throw new PlatformNotSupportedException();
await ServiceInstaller.ConfigureAndRunAsync<TestService>(serviceSettings, args);