namespace Woof.Data.DbCommandApi;

/// <summary>
/// Provides conversions between SQL and CLR data types.
/// </summary>
public static class DataConverters {

    /// <summary>
    /// Replaces DBNull-s with null-s, converts to target type.
    /// </summary>
    /// <param name="x">Database object.</param>
    /// <param name="t">CLR type object.</param>
    /// <returns>CLR safe object.</returns>
    private static object? GetClrType(this object x, Type t) {
        if (x is DBNull) return null;
        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
            if (x is null) return null;
            else t = Nullable.GetUnderlyingType(t) ?? throw new InvalidOperationException("Can't determine CLR type");
        }
        return Convert.ChangeType(x, t);
    }

    /// <summary>
    /// Reads a row from <see cref="DbDataReader"/> to the matching properties of a record.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="reader">A <see cref="DbDataReader"/> instance.</param>
    /// <returns>Data record.</returns>
    /// <remarks>Doesn't work with structs.</remarks>
    public static T? ReadToPropertiesOfNew<T>(this DbDataReader reader) where T : new() {
        if (reader is null) return default;
        var type = typeof(T);
        if (type.IsValueType || type.IsArray || type.IsPointer) throw new InvalidCastException();
        var record = new T();
        for (int i = 0, c = reader.FieldCount; i < c; i++) {
            var name = reader.GetName(i);
            var property = type.GetProperty(name);
            var value = reader.GetValue(i);
            if (property is not null) {
                if (property.SetMethod is null) throw new InvalidOperationException($"No setter defined for {name}.");
                property.SetValue(record, value.GetClrType(property.PropertyType));
            }
        }
        return record;
    }

    /// <summary>
    /// Gets a data table from a collection of elements, the element properties will be matched with table columns.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="items">Collection of elements.</param>
    /// <returns>Data table.</returns>
    public static DataTable? AsDataTable<T>(this IEnumerable<T> items) {
        var type = typeof(T);
        if (type.IsValueType || type.IsArray || type.IsPointer) throw new InvalidCastException();
        DataTable? table = null;
        var properties = type.GetProperties();
        foreach (var i in items) {
            if (table is null) {
                table = new DataTable();
                foreach (var p in properties) table.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);
            }
            var row = table.NewRow();
            foreach (var p in properties) row[p.Name] = p.GetValue(i) ?? DBNull.Value;
            table.Rows.Add(row);
        }
        return table;
    }

}
