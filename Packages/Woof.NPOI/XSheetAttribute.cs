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
    /// Gets or sets a value indicating that all column sizes shoud be automatic.
    /// </summary>
    public bool AutoSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating that the sum should be placed in the first row, instead of the last one.
    /// </summary>
    public bool IsSumInTheFirstRow { get; set; }

    /// <summary>
    /// Gets or sets the label for the sum, effective when <see cref="IsSumInTheFirstRow"/> is true.
    /// </summary>
    public string? SumLabel { get; set; }

}
