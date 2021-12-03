namespace Woof.Internals;

/// <summary>
/// Resolves available API members.
/// </summary>
/// <remarks>
/// Available means referenced in the project as packages or projects.
/// When the target is built as single file, the target api assembly must be linked in.
/// </remarks>
public static class ApiResolver {

    /// <summary>
    /// Gets the current OS name as <see cref="OSPlatform"/> property name.<br/>
    /// One of "Windows", "Linux", "OSX" or "FreeBSD" strings.
    /// </summary>
    public static string OSName { get; } =
        new (string name, OSPlatform platform)[] {
            (nameof(OSPlatform.Windows), OSPlatform.Windows),
            (nameof(OSPlatform.Linux), OSPlatform.Linux),
            (nameof(OSPlatform.OSX), OSPlatform.OSX),
            (nameof(OSPlatform.FreeBSD), OSPlatform.FreeBSD)
        }.First(p => RuntimeInformation.IsOSPlatform(p.platform)).name;

    /// <summary>
    /// Gets non-Windows DPAPI if referenced in project.
    /// </summary>
    /// <typeparam name="TInterface">Target interface.</typeparam>
    /// <returns>DPAPI instance.</returns>
    public static TInterface? GetNonWindowsDPAPI<TInterface>() where TInterface : class
        => GetInstance<TInterface>($"Woof.DataProtection.{OSName}", $"Woof.DataProtection.Api.DPAPI_{OSName}");

    /// <summary>
    /// Gets an instance of the specified type.
    /// If it's not already loaded, the target assembly will be loaded.
    /// </summary>
    /// <typeparam name="TInterface">Type interface.</typeparam>
    /// <param name="assemblyName">Assembly name.</param>
    /// <param name="typeName">Type name.</param>
    /// <returns>Instance or null if not found.</returns>
    public static TInterface? GetInstance<TInterface>(string assemblyName, string typeName) where TInterface : class {
        Type? type;
        try {
            var assembly = Assembly.Load(assemblyName);
            type = assembly.GetType(typeName);
        }
        catch { return null; }
        return type is null ? null : (TInterface?)Activator.CreateInstance(type);
    }

}