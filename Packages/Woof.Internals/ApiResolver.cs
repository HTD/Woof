namespace Woof.Internals;

/// <summary>
/// Resolves available API members.
/// </summary>
public static class ApiResolver {

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