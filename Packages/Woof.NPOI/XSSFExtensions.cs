namespace Woof.NPOI;

/// <summary>
/// Extension for fast generation of spreadsheets directly from object collections.
/// </summary>
public static class XSSFExtensions {

    /// <summary>
    /// A value the column width in characters should be multiplied to get the roughly proper internal column width.
    /// </summary>
    const double ColumnWidthMultiplier = 272.5755;

    /// <summary>
    /// Creates a spreadsheet from the rows collection.
    /// </summary>
    /// <typeparam name="TElement">Element type.</typeparam>
    /// <param name="collection">A collection of elements.</param>
    /// <returns><see cref="XSSFWorkbook"/></returns>
    public static XSSFWorkbook ToXSSFWorkbook<TElement>(this IEnumerable<TElement> collection) {
        var workbook = new XSSFWorkbook();
        var formats = new DataFormats(workbook.CreateDataFormat());
        // Columns:
        var type = typeof(TElement);
        var columns = type.GetProperties()
            .Select(p => {
                var metadata = p.GetCustomAttribute<XCellAttribute>();
                var column = (
                    Name: p.GetDisplayName(),
                    Metadata: metadata,
                    Property: p,
                    Format: metadata?.DataFormat is null
                        ? formats.FromType(p.PropertyType)
                        : formats.FromString(metadata.DataFormat),
                    Style: workbook.CreateCellStyle());
                column.Style.DataFormat = column.Format;
                if (metadata is not null && metadata.Alignment != default) column.Style.Alignment = metadata.Alignment;
                return column;
            })
            .Where(c => c.Metadata?.Skip != true)
            .ToArray();
        var n = columns.Length;
        // Sheet:
        var sheetName = type.GetDisplayName();
        var sheet = workbook.CreateSheet(sheetName);
        var sheetMetadata = type.GetCustomAttribute<XSheetAttribute>();
        // Styles
        var boldFont = workbook.CreateFont();
        boldFont.IsBold = true;
        var headerTextStyle = workbook.CreateCellStyle();
        headerTextStyle.DataFormat = formats.Text;
        headerTextStyle.SetFont(boldFont);
        sheet.CreateFreezePane(0, 1);
        // Header row
        int rowIndex = 0;
        var headerRow = sheet.CreateRow(rowIndex++);
        for (var i = 0; i < n; i++) {
            var cell = headerRow.CreateCell(i);
            cell.CellStyle = headerTextStyle;
            cell.SetCellValue(columns[i].Name);
        }
        // Data content
        foreach (var item in collection) {
            var row = sheet.CreateRow(rowIndex++);
            for (var i = 0; i < n; i++) {
                var (name, metadata, property, format, style) = columns[i];
                if (property.GetValue(item) is object value) {
                    var cell = row.CreateCell(i);
                    cell.SetValue(value, format == formats.Text);
                    if (style is not null || format != formats.Auto) cell.CellStyle = style;
                }
            }
        }
        // Column widths
        for (var i = 0; i < n; i++) {
            var columnWidth = columns[i].Metadata?.Width;
            var isAutoSize = sheetMetadata?.AutoSize == true || columnWidth is null || columnWidth.Value == default;
            if (isAutoSize) sheet.AutoSizeColumn(i);
            else if (columnWidth is not null)
                sheet.SetColumnWidth(i, columnWidth.Value);
        }
        // Sums
        var isAnySumPresent = columns.Any(c => c.Metadata?.AddSum == true);
        var isSumInTheFirstRow = sheetMetadata?.IsSumInTheFirstRow ?? false;
        if (isAnySumPresent) {
            if (isSumInTheFirstRow && sheetMetadata?.SumLabel is string sumLabel) {
                var cell = sheet.GetRow(0).CreateCell(n);
                cell.SetCellType(CellType.String);
                cell.SetCellValue(sumLabel);
                cell.CellStyle = workbook.CreateCellStyle();
                cell.CellStyle.Alignment = HorizontalAlignment.Right;
                cell.CellStyle.DataFormat = formats.Text;
                cell.CellStyle.SetFont(boldFont);
            }
            var row = isSumInTheFirstRow ? sheet.GetRow(0) : sheet.CreateRow(rowIndex);
            var targetColumnIndex = isSumInTheFirstRow ? n + 1 : 0;
            for (var i = 0; i < n; i++) {
                if (!isSumInTheFirstRow) targetColumnIndex = i;
                var isSum = columns[i].Metadata?.AddSum ?? false;
                var isApplicable = columns[i].Property.PropertyType != typeof(string);
                if (isSum) {
                    var cell = row.CreateCell(targetColumnIndex);
                    var sourceColumn = CellReference.ConvertNumToColString(i);
                    var formula = isApplicable
                        ? $"SUM({sourceColumn}2:{sourceColumn}{rowIndex})"
                        : $"{sourceColumn}{rowIndex}";
                    var sourceFormat = columns[i].Format;
                    var cellStyle = workbook.CreateCellStyle();
                    if (sourceFormat != formats.Auto) cellStyle.DataFormat = sourceFormat;
                    cellStyle.SetFont(boldFont);
                    cell.CellStyle = cellStyle;
                    cell.SetCellFormula(formula);
                    sheet.SetColumnWidth(targetColumnIndex, 16.0);
                    if (isSumInTheFirstRow) targetColumnIndex++;
                }
            }
        }
        return workbook;
    }

    /// <summary>
    /// Exports the rows collection to an Excel file.
    /// </summary>
    /// <typeparam name="TElement">Element type.</typeparam>
    /// <param name="collection">A collection of elements.</param>
    /// <param name="fileName">File name.</param>
    public static void ToExcel<TElement>(this IEnumerable<TElement> collection, string fileName) {
        var workbook = collection.ToXSSFWorkbook();
        using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        workbook.Write(stream);
    }

    /// <summary>
    /// Writes the data to a memory stream.
    /// </summary>
    /// <param name="workbook"><see cref="XSSFWorkbook"/>.</param>
    /// <returns>An open memory stream set to zero position, ready for reading.</returns>
    public static MemoryStream ToMemoryStream(this XSSFWorkbook workbook) {
        var targetStream = new MemoryStream();
        workbook.Write(targetStream, leaveOpen: true);
        targetStream.Position = 0;
        return targetStream;
    }

    #region Helpers

    /// <summary>
    /// Gets the display name value from member annotation.
    /// </summary>
    /// <param name="memberInfo">Member information.</param>
    /// <returns>Display name.</returns>
    private static string GetDisplayName(this MemberInfo memberInfo) =>
        memberInfo.GetCustomAttribute<XCellAttribute>()?.Name ??
        memberInfo.GetCustomAttribute<XSheetAttribute>()?.Name ??
        memberInfo.GetCustomAttribute<DisplayAttribute>()?.Name ??
        memberInfo.Name;

    /// <summary>
    /// Sets the cell value from object knowing its CLR type, considering optional data format string.
    /// </summary>
    /// <param name="cell">Cell.</param>
    /// <param name="value">Object value.</param>
    /// <param name="asText">Set true to force setting as text.</param>
    private static void SetValue(this ICell cell, object value, bool asText = false) {
        if (value is null or DBNull) {
            cell.SetCellType(CellType.Blank);
            return;
        }
        if (asText) {
            cell.SetCellType(CellType.String);
            cell.SetCellValue(value.ToString());
        }
        else if (value is string stringValue) {
            cell.SetCellType(CellType.String);
            cell.SetCellValue(stringValue);
        }
        else if (value is DateTime dateTimeValue) {
            cell.SetCellValue(DateUtil.GetExcelDate(dateTimeValue));
        }
        else if (value is TimeSpan timeSpanValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(timeSpanValue.TotalDays);
        }
        else if (value is byte byteValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(byteValue);
        }
        else if (value is sbyte sbyteValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(sbyteValue);
        }
        else if (value is short shortValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(shortValue);
        }
        else if (value is ushort ushortValue) {
            cell.SetCellValue(ushortValue);
            cell.SetCellType(CellType.Numeric);
        }
        else if (value is int intValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(intValue);
        }
        else if (value is uint uintValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(uintValue);
        }
        else if (value is long longValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(longValue);
        }
        else if (value is ulong ulongValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(ulongValue);
        }
        else if (value is decimal decimalValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue((double)decimalValue);
        }
        else if (value is double doubleValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(doubleValue);
        }
        else if (value is float floatValue) {
            cell.SetCellType(CellType.Numeric);
            cell.SetCellValue(floatValue);
        }
        else {
            cell.SetCellValue(value.ToString());
        }
    }

    /// <summary>
    /// Sets the column width using Excel width value (roughly in number of characters to fit).
    /// </summary>
    /// <param name="sheet">Sheet.</param>
    /// <param name="columnIndex">The column to set (0-based).</param>
    /// <param name="value">Excel width value, number of characters to fit.</param>
    private static void SetColumnWidth(this ISheet sheet, int columnIndex, double value)
        => sheet.SetColumnWidth(columnIndex, (int)Math.Round(value * ColumnWidthMultiplier));

    #endregion

}
