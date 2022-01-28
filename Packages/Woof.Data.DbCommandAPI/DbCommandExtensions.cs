namespace Woof.Data.DbCommandApi;

/// <summary>
/// Allows setting the SQL command parameters from an anonymous object.
/// </summary>
public static class DbCommandExtensions {

    /// <summary>
    /// Sets the SQL parameters from (anonymous) object's properties.
    /// </summary>
    /// <typeparam name="TParam">A RDBMS specific implementation of the <see cref="DbParameter"/>.</typeparam>
    /// <param name="cmd">Command.</param>
    /// <param name="parameters">An object whose properties with names will be used to set command parameters.</param>
    public static void SetParameters<TParam>(this DbCommand cmd, object parameters) where TParam : DbParameter, new() {
        foreach (var property in parameters.GetType().GetProperties())
            cmd.Parameters.Add(new TParam {
                ParameterName = property.Name,
                Value = property.GetValue(parameters)
            });
    }

}