namespace Woof.CSV;

/// <summary>
/// Represent an error that occured while parsing a CSV file line.
/// </summary>
public class CsvLineException : Exception {

    /// <summary>
    /// Creates new instance.
    /// </summary>
    /// <param name="innerException">Original exception thrown while processing the line.</param>
    /// <param name="line">The CSV line that caused the exception.</param>
    /// <param name="lineIndex">Line number (starting from 1).</param>
    public CsvLineException(Exception innerException, string? line, int lineIndex) : base("CSV line parsing error", innerException) {
        Line = line;
        LineIndex = lineIndex;
    }

    /// <summary>
    /// Gets or sets the line that caused the original exception.
    /// </summary>
    public string? Line { get; set; }

    /// <summary>
    /// Gets or sets the line number (starting from 1).
    /// </summary>
    public int LineIndex { get; set; }

}
