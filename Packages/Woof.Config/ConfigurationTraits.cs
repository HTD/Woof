namespace Woof.Config;

/// <summary>
/// Provides <see cref="SetValue(IConfiguration, string, object?)"/> method.
/// </summary>
public static class ConfigurationTraits {

    /// <summary>
    /// Sets <paramref name="configuration"/>'s <paramref name="key"/> with specified <paramref name="value"/>.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="key">The key of the configuration section.</param>
    /// <param name="value">Value to set.</param>
    /// <exception cref="InvalidOperationException">Not supported type as value.</exception>
    public static void SetValue(this IConfiguration configuration, string key, object? value) {
        var c = CultureInfo.InvariantCulture;
        var valueString = value switch {
            null => null,
            string v => v,
            Uri v => v.ToString(),
            byte[] v => Convert.ToBase64String(v),
            bool v => v.ToString(c),
            int v => v.ToString(c),
            decimal v => v.ToString(c),
            double v => v.ToString(c),
            uint v => v.ToString(c),
            long v => v.ToString(c),
            ulong v => v.ToString(c),
            short v => v.ToString(c),
            ushort v => v.ToString(c),
            byte v => v.ToString(c),
            sbyte v => v.ToString(c),
            float v => v.ToString(c),
            _ => throw new InvalidOperationException($"Cannot set value of type {value.GetType()}")
        };
        configuration[key] = valueString;
    }

}