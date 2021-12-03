namespace Woof.DataProtection.Internals;

/// <summary>
/// Resolves available API members.
/// </summary>
internal static class ApiResolver {

    /// <summary>
    /// Gets an instance of the specified type.
    /// If it's not already loaded, the target assembly will be loaded.
    /// </summary>
    /// <typeparam name="TInterface">Type interface.</typeparam>
    /// <param name="assemblyName">Assembly name.</param>
    /// <param name="typeName">Type name.</param>
    /// <returns>Instance or null if not found.</returns>
    public static TInterface? GetInstance<TInterface>(string assemblyName, string typeName) {
        var type = Type.GetType(typeName);
        if (type is null) {
            try {
                var assembly = Assembly.Load(assemblyName);
                type = assembly.GetType(typeName);
            }
            catch { }
        }
        return type is null ? default : (TInterface?)Activator.CreateInstance(type);
    }

}