namespace Woof.CSV;

/// <summary>
/// Complete CSV reader.
/// </summary>
public class CsvReader {

    #region Properties

    /// <summary>
    /// Gets or sets the default date and time format for cells.
    /// Default format ("yyyy-MM-dd") can be overriden with <see cref="DateTimeFormatAttribute"/>
    /// </summary>
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Gets or sets cell delimiter. Default ','.
    /// </summary>
    public char Delimiter {
        get => Splitter.Delimiter;
        set => Splitter.Delimiter = value;
    }

    /// <summary>
    /// Gets or sets default encoding for text files and streams.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Gets or sets the default format provider for cells.
    /// Default: <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Gets or sets the value indicating whether the CSV file has a header row.
    /// </summary>
    public bool HasHeader { get; set; }

    /// <summary>
    /// Gets or sets the name of the special record property containing line index within the file.
    /// </summary>
    public string LineIndexPropertyName { get; set; } = "LineIndex";

    /// <summary>
    /// Gets or sets the value indicating wheter null will be set instead of empty strings. Default false.
    /// </summary>
    public bool NullEmptyStrings { get; set; }

    /// <summary>
    /// Gets or sets cell quote character. Default '"'.
    /// </summary>
    public char? Quote {
        get => Splitter.Quote;
        set => Splitter.Quote = value;
    }

    /// <summary>
    /// Gets or sets the name of the special record property containing CSV line raw text.
    /// </summary>
    public string RawTextPropertyName { get; set; } = "RawText";

    /// <summary>
    /// Gets or sets optional function to process a CSV line before it's splitted into cells.
    /// </summary>
    public TextPreprocessorDelegate? LinePreprocessor { get; set; }

    /// <summary>
    /// Gets or sets optional function to process a CSV cell after being passed from the splitter.
    /// </summary>
    public TextPreprocessorDelegate? CellPreprocessor { get; set; }

    /// <summary>
    /// Gets or sets a function for filtering the rows.
    /// </summary>
    public RowFilterDelegate? RowFilter { get; set; }

    /// <summary>
    /// Gets the splitter used to split CSV text into rows and cells.
    /// </summary>
    public CsvSplitter Splitter { get; } = new CsvSplitter();

    /// <summary>
    /// Gets or sets the default time span format for cells.
    /// Default format ("g") can be overriden with <see cref="TimeSpanFormatAttribute"/> for properties.
    /// </summary>
    public string TimeSpanFormat { get; set; } = "g";

    /// <summary>
    /// Gets a value indicating the last parsing operation didn't caused any exceptions.
    /// </summary>
    public bool IsParserClear => !_LineExceptions.Any();

    /// <summary>
    /// Gets an <see cref="AggregateException"/> containing all exceptions that occured while parsing CSV lines.
    /// </summary>
    public AggregateException ParseException => new(_LineExceptions.ToArray());

    #endregion

    /// <summary>
    /// Reads CSV text into a collection of predefined records.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="text">CSV text.</param>
    /// <returns>Collection of records.</returns>
    public IEnumerable<T?> Read<T>(string text) where T : new() {
        var index = 0;
        var skip = HasHeader;
        _LineExceptions.Clear();
        foreach (var line in Splitter.GetLines(text)) {
            if (skip) { skip = false; continue; }
            T value;
            try {
                var cells = Splitter.GetCells(LinePreprocessor == null ? line : LinePreprocessor(line)).ToArray();
                if (RowFilter != null) {
                    var action = RowFilter(index, cells);
                    if (action == FilterAction.Reject) continue;
                    else if (action == FilterAction.Break) break;
                }
                value = ReadRowToPropertiesOfNew<T>(index++, cells, line);
            }
            catch (Exception x) {
                _LineExceptions.Add(new CsvLineException(x, line, index));
                continue;
            }
            yield return value;
        }
    }

    /// <summary>
    /// Reads CSV text into a collecton of <see cref="CsvRowData"/> rows.
    /// </summary>
    /// <param name="text">CSV text.</param>
    /// <returns>Collection of rows.</returns>
    public IEnumerable<CsvRowData?> Read(string text) {
        var index = 0;
        var skip = HasHeader;
        _LineExceptions.Clear();
        foreach (var line in Splitter.GetLines(text)) {
            if (skip) { skip = false; continue; }
            CsvRowData value;
            try {
                var cells = Splitter.GetCells(LinePreprocessor == null ? line : LinePreprocessor(line)).ToArray();
                if (RowFilter != null) {
                    var action = RowFilter(index, cells);
                    if (action == FilterAction.Reject) continue;
                    else if (action == FilterAction.Break) break;
                }
                value = new CsvRowData(index++, cells, line);
            }
            catch (Exception x) {
                _LineExceptions.Add(new CsvLineException(x, line, index));
                continue;
            }
            yield return value;
        }
    }

    /// <summary>
    /// Reads CSV stream using configured encoding into collection of predefined records.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="stream">CSV stream.</param>
    /// <returns>Collection of records.</returns>
    public IEnumerable<T?> Read<T>(Stream stream) where T : new() {
        using var reader = new StreamReader(stream, Encoding, true);
        return Read<T>(reader.ReadToEnd());
    }

    /// <summary>
    /// Reads CSV stream using configured encoding into collection of <see cref="CsvRowData"/> rows.
    /// </summary>
    /// <param name="stream">CSV stream.</param>
    /// <returns>Collection of rows.</returns>
    public IEnumerable<CsvRowData?> Read(Stream stream) {
        using var reader = new StreamReader(stream, Encoding, true);
        return Read(reader.ReadToEnd());
    }

    /// <summary>
    /// Reads CSV stream using configured encoding into collection of predefined records.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="stream"></param>
    /// <returns>Asynchronous stream returning items after whole stream is loaded and split into lines.</returns>
    public async IAsyncEnumerable<T?> ReadAsync<T>(Stream stream) where T : new() {
        using var reader = new StreamReader(stream, Encoding, true);
        var text = await reader.ReadToEndAsync();
        var items = Read<T>(text);
        foreach (var item in items) yield return item;
    }

    /// <summary>
    /// Reads CSV stream using configured encoding into collection of <see cref="CsvRowData"/> rows.
    /// </summary>
    /// <param name="stream">CSV stream.</param>
    /// <returns>Asynchronous stream returning items after whole stream is loaded and split into lines.</returns>
    public async IAsyncEnumerable<CsvRowData?> ReadAsync(Stream stream) {
        using var reader = new StreamReader(stream, Encoding, true);
        var text = await reader.ReadToEndAsync();
        var items = Read(text);
        foreach (var item in items) yield return item;
    }

    /// <summary>
    /// Reads CSV file using configured encoding into collection of predefined records.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="path">Path to CSV file.</param>
    /// <returns>Collection of records.</returns>
    public IEnumerable<T?> ReadFile<T>(string path) where T : new() {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream, Encoding, true);
        return Read<T>(reader.ReadToEnd());
    }

    /// <summary>
    /// Reads CSV file into a collecton of <see cref="CsvRowData"/> rows.
    /// </summary>
    /// <param name="path">A path to the CSV file.</param>
    /// <returns>Collection of rows.</returns>
    public IEnumerable<CsvRowData?> ReadFile(string path) {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream, Encoding, true);
        return Read(reader.ReadToEnd());
    }

    /// <summary>
    /// Reads CSV file using configured encoding into collection of predefined records.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="path">Path to CSV file.</param>
    /// <returns>>Asynchronous stream returning items after whole stream is loaded and split into lines.</returns>
    public async IAsyncEnumerable<T?> ReadFileAsync<T>(string path) where T : new() {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream, Encoding, true);
        var items = Read<T>(await reader.ReadToEndAsync());
        foreach (var item in items) yield return item;
    }

    /// <summary>
    /// Reads CSV file into a collecton of <see cref="CsvRowData"/> rows.
    /// </summary>
    /// <param name="path">A path to the CSV file.</param>
    /// <returns>>Asynchronous stream returning items after whole stream is loaded and split into lines.</returns>
    public async IAsyncEnumerable<CsvRowData?> ReadFileAsync(string path) {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream, Encoding, true);
        var items = Read(await reader.ReadToEndAsync());
        foreach (var item in items) yield return item;
    }

    /// <summary>
    /// Reads all matchin files in a directory to one collection of predefined records.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="directory">Directory path.</param>
    /// <param name="searchPattern">Optional search pattern to match specified extensions.</param>
    /// <param name="searchOption">Search option.</param>
    /// <returns>Collection of records.</returns>
    public IEnumerable<T?> ReadDirectory<T>(string directory, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) where T : new() {
        foreach (var file in Directory.EnumerateFiles(directory, searchPattern, searchOption)) {
            foreach (var record in ReadFile<T>(file)) yield return record;
        }
    }

    /// <summary>
    /// Reads all matching files in a directory to one collection of <see cref="CsvRowData"/>.
    /// </summary>
    /// <param name="directory">Directory path.</param>
    /// <param name="searchPattern">Search pattern, default "*" for all files.</param>
    /// <param name="searchOption">Allows recursive search, see <see cref="SearchOption"/>.</param>
    /// <returns>A collection of rows.</returns>
    public IEnumerable<CsvRowData?> ReadDirectory(string directory, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) {
        foreach (var file in Directory.EnumerateFiles(directory, searchPattern, searchOption)) {
            foreach (var record in ReadFile(file)) yield return record;
        }
    }

    /// <summary>
    /// Reads all matchin files in a directory to one collection of predefined records.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="directory">Directory path.</param>
    /// <param name="searchPattern">Optional search pattern to match specified extensions.</param>
    /// <param name="searchOption">Search option.</param>
    /// <returns>Asynchronous stream starting returning items after a file is loaded and split into lines.</returns>
    public async IAsyncEnumerable<T?> ReadDirectoryAsync<T>(string directory, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) where T : new() {
        foreach (var file in Directory.EnumerateFiles(directory, searchPattern, searchOption)) {
            await foreach (var record in ReadFileAsync<T>(file)) yield return record;
        }
    }

    /// <summary>
    /// Reads all matching files in a directory to one collection of <see cref="CsvRowData"/>.
    /// </summary>
    /// <param name="directory">Directory path.</param>
    /// <param name="searchPattern">Search pattern, default "*" for all files.</param>
    /// <param name="searchOption">Allows recursive search, see <see cref="SearchOption"/>.</param>
    /// <returns>Asynchronous stream starting returning items after a file is loaded and split into lines.</returns>
    public async IAsyncEnumerable<CsvRowData?> ReadDirectoryAsync(string directory, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) {
        foreach (var file in Directory.EnumerateFiles(directory, searchPattern, searchOption)) {
            await foreach (var record in ReadFileAsync(file)) yield return record;
        }
    }

    /// <summary>
    /// Reads CSV row into a predefined record's properties.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="cells">CSV row (string cell collection).</param>
    /// <param name="index">Index of the CSV line within the file.</param>
    /// <param name="rawText">Raw text of the CSV line.</param>
    /// <returns>Record.</returns>
    private T ReadRowToPropertiesOfNew<T>(int index, string[] cells, string? rawText) where T : new() {
        var propertyEnumerator = typeof(T).GetProperties().GetEnumerator();
        var cellEnumerator = cells.GetEnumerator();
        var item = new T();
        while (propertyEnumerator.MoveNext()) {
            var p = (PropertyInfo)propertyEnumerator.Current;
            if (p.Name == LineIndexPropertyName) p.SetValue(item, index);
            else if (p.Name == RawTextPropertyName) p.SetValue(item, rawText);
            else if (cellEnumerator.MoveNext()) {
                var s = (string)cellEnumerator.Current;
                string? format = null;
                if (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?)) format = p.GetCustomAttribute<DateTimeFormatAttribute>()?.Format;
                if (p.PropertyType == typeof(TimeSpan) || p.PropertyType == typeof(TimeSpan?)) format = p.GetCustomAttribute<TimeSpanFormatAttribute>()?.Format;
                object? value = Convert(s, p.PropertyType, format);
                if (value != null) p.SetValue(item, value);
            }
        }
        return item;
    }

    /// <summary>
    /// Converts a string value to target type with optional parsing format.
    /// </summary>
    /// <param name="value">String value.</param>
    /// <param name="type">Target type.</param>
    /// <param name="format">Parsing format.</param>
    /// <returns>Converted and boxed value.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Compiler is just wrong")]
    private object? Convert(string? value, Type type, string? format = null) {
        if (type == typeof(string)) return NullEmptyStrings && String.IsNullOrEmpty(value) ? null : value;
        // Non-nullable types:
        const string eNullArgMsg = "Value cannot be null for non-nullable type";
        if (type == typeof(byte)) return Byte.Parse(value ?? throw new ArgumentNullException(nameof(value), eNullArgMsg));
        if (type == typeof(Int16)) return Int16.Parse(value ?? throw new ArgumentNullException(nameof(value), eNullArgMsg));
        if (type == typeof(int)) return Int32.Parse(value ?? throw new ArgumentNullException(nameof(value), eNullArgMsg));
        if (type == typeof(long)) return Int64.Parse(value ?? throw new ArgumentNullException(nameof(value), eNullArgMsg));
        if (type == typeof(decimal)) return Decimal.Parse(value ?? throw new ArgumentNullException(nameof(value), eNullArgMsg), FormatProvider);
        if (type == typeof(double)) return Double.Parse(value ?? throw new ArgumentNullException(nameof(value), eNullArgMsg), FormatProvider);
        if (type == typeof(float)) return Single.Parse(value ?? throw new ArgumentNullException(nameof(value), eNullArgMsg), FormatProvider);
        if (type == typeof(DateTime)) return DateTime.ParseExact(value ?? throw new ArgumentNullException(nameof(value), eNullArgMsg), format ?? DateTimeFormat, FormatProvider);
        if (type == typeof(TimeSpan)) return TimeSpan.ParseExact(value ?? throw new ArgumentNullException(nameof(value), eNullArgMsg), format ?? TimeSpanFormat, FormatProvider);
        // Nullable types:
        if (type == typeof(byte?)) return String.IsNullOrWhiteSpace(value) ? null : (object)Byte.Parse(value);
        if (type == typeof(Int16?)) return String.IsNullOrWhiteSpace(value) ? null : (object)Int16.Parse(value);
        if (type == typeof(int?)) return String.IsNullOrWhiteSpace(value) ? null : (object)Int32.Parse(value);
        if (type == typeof(long?)) return String.IsNullOrWhiteSpace(value) ? null : (object)Int64.Parse(value);
        if (type == typeof(decimal?)) return String.IsNullOrWhiteSpace(value) ? null : (object)Decimal.Parse(value, FormatProvider);
        if (type == typeof(double?)) return String.IsNullOrWhiteSpace(value) ? null : (object)Double.Parse(value, FormatProvider);
        if (type == typeof(float?)) return String.IsNullOrWhiteSpace(value) ? null : (object)Single.Parse(value, FormatProvider);
        if (type == typeof(DateTime?)) return String.IsNullOrWhiteSpace(value) ? null : (object)DateTime.ParseExact(value, format ?? DateTimeFormat, FormatProvider);
        if (type == typeof(TimeSpan?)) return String.IsNullOrWhiteSpace(value) ? null : (object)TimeSpan.ParseExact(value, format ?? TimeSpanFormat, FormatProvider);
        throw new InvalidCastException("Unsupported property type");
    }

    /// <summary>
    /// Converts a string value to target type.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <param name="value">String value.</param>
    /// <returns>Converted value.</returns>
    public T? Convert<T>(string value) => Convert(value, typeof(T)) is T result ? result : default;

    /// <summary>
    /// Internal CSV line parsing exception list.
    /// </summary>
    private readonly List<CsvLineException> _LineExceptions = [];

}

/// <summary>
/// Contains data of the CSV data row.
/// </summary>
public class CsvRowData {

    /// <summary>
    /// Gets the document line index.
    /// </summary>
    public int LineIndex { get; }

    /// <summary>
    /// Gets the cells array.
    /// </summary>
    public string[] Cells { get; }

    /// <summary>
    /// Gets the original string of the row.
    /// </summary>
    public string RawText { get; }

    /// <summary>
    /// Creates a CSV row data object.
    /// </summary>
    /// <param name="lineIndex">Document line index.</param>
    /// <param name="cells">Cells array.</param>
    /// <param name="rawText">Original string before splitting.</param>
    public CsvRowData(int lineIndex, string[] cells, string rawText) {
        LineIndex = lineIndex;
        Cells = cells;
        RawText = rawText;
    }

}

/// <summary>
/// Actions for the row filter.
/// </summary>
public enum FilterAction {
    /// <summary>
    /// This row is accepted as normal data row.
    /// </summary>
    Accept,
    /// <summary>
    /// This row is rejected as non-data.
    /// </summary>
    Reject,
    /// <summary>
    /// This and all subsequent rows are rejected.
    /// </summary>
    Break
}

/// <summary>
/// Text pre-processor function type.
/// </summary>
/// <param name="raw">Raw line text.</param>
public delegate string TextPreprocessorDelegate(string raw);

/// <summary>
/// Row filter function type.
/// </summary>
/// <param name="lineIndex">The index of the processed line.</param>
/// <param name="cells">Cells after splitting.</param>
/// <returns>What to do with the row, as one of <see cref="FilterAction"/> enumeration.</returns>
public delegate FilterAction RowFilterDelegate(int lineIndex, string[] cells);
