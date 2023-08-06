namespace Woof.NPOI;

/// <summary>
/// Provides metadata for XSSF sheets.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class XSheetAttribute : Attribute {

    /// <summary>
    /// Gets or sets the sheet name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value automatic column filters are enabled in the first row.
    /// </summary>
    public bool AutoFilters { get; set; }

    /// <summary>
    /// Gets or sets a value indicating that all column sizes shoud be automatic.
    /// </summary>
    public bool AutoSize { get; set; }

    /// <summary>
    /// Gets or sets the column filters start index.
    /// Negative or zero will select first column.
    /// </summary>
    public int ColumnFiltersStart { get; set; } = -1;

    /// <summary>
    /// Gets or sets the column filters end index.
    /// Negative value will select the last column.
    /// </summary>
    public int ColumnFiltersEnd { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating that the sum should be placed in the first row, instead of the last one.
    /// </summary>
    public bool IsSumInTheFirstRow { get; set; }

    /// <summary>
    /// Gets or sets the label for the sum, effective when <see cref="IsSumInTheFirstRow"/> is true.
    /// </summary>
    public string? SumLabel { get; set; }

}
