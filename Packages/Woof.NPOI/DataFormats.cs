using System;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Woof.NPOI;

/// <summary>
/// Data format processor for NPOI.
/// </summary>
class DataFormats {

    /// <summary>
    /// Gets the automatic format.
    /// </summary>
    public short Auto { get; }
    
    /// <summary>
    /// Gets the accounting format.
    /// </summary>
    public short Accounting { get; }

    /// <summary>
    /// Gets the date format.
    /// </summary>
    public short Date { get; }
    
    /// <summary>
    /// Gets the date and time format.
    /// </summary>
    public short DateTime { get; }
    
    /// <summary>
    /// Gets the time format.
    /// </summary>
    public short Time { get; }

    /// <summary>
    /// Gets the text format.
    /// </summary>
    public short Text { get; }

    /// <summary>
    /// Gets or sets the default accounting format.
    /// </summary>
    public string DefaultAccountingFormat { get; set; } = "#,##0.00";

    /// <summary>
    /// Gets or sets the default date format.
    /// </summary>
    public string DefaultDateFormat { get; set; } = "YYYY-MM-DD";

    /// <summary>
    /// Gets or sets the default date and time format.
    /// </summary>
    public string DefaultDateTimeFormat { get; set; } = "YYYY-MM-DD HH:MM";

    /// <summary>
    /// Gets or sets the default time format.
    /// </summary>
    public string DefaultTimeFormat { get; set; } = "HH:MM";

    /// <summary>
    /// Creates data formats source for the specified data format table.
    /// </summary>
    /// <param name="dataFormatTable">Data format table for the workbook, see <see cref="XSSFWorkbook.CreateDataFormat()"/>.</param>
    public DataFormats(IDataFormat dataFormatTable) {
        DataFormatTable = dataFormatTable;
        Auto = (short)BuiltinFormats.GetBuiltinFormat("General");
        Accounting = DataFormatTable.GetFormat(DefaultAccountingFormat);
        Date = DataFormatTable.GetFormat(DefaultDateFormat);
        DateTime = DataFormatTable.GetFormat(DefaultDateTimeFormat);
        Time = DataFormatTable.GetFormat(DefaultTimeFormat);
        Text = (short)BuiltinFormats.GetBuiltinFormat("TEXT");
    }

    /// <summary>
    /// Gets a data format from string. Creates new format entries if needed.
    /// </summary>
    /// <param name="dataFormatString">Format string.</param>
    /// <returns>Format index.</returns>
    public short FromString(string dataFormatString) {
        var index = (short)BuiltinFormats.GetBuiltinFormat(dataFormatString);
        return index < 0 ? DataFormatTable.GetFormat(dataFormatString) : index;
    }

    /// <summary>
    /// Gets a data format from type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public short FromType(Type type) {
        var nullable = Nullable.GetUnderlyingType(type);
        if (nullable is not null) type = nullable;
        if (type == typeof(string)) return Text;
        else if (type == typeof(DateTime)) return DateTime;
        else if (type == typeof(TimeSpan)) return Time;
        else if (type == typeof(decimal)) return Accounting;
        return Auto;
    }

    private readonly IDataFormat DataFormatTable;

}