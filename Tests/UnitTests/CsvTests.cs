namespace UnitTests;

public class CsvTests {

    [Fact]
    public void A010_ValidFile_NoConversion() {
        const string fileName = @"Samples\Test1.csv";
        var reader = new CsvReader() {
            Delimiter = ',',
            Encoding = Encoding.UTF8,
            HasHeader = true,
            Quote = '"'
        };
        var rows =  reader.ReadFile(fileName).ToArray();
        Assert.Equal(4, rows.Length);
        Assert.True(reader.IsParserClear);
    }

    [Fact]
    public void A020_InvalidFile_RowTypeConversion() {
        const string fileName = @"Samples\Test2.csv";
        var reader = new CsvReader() {
            FormatProvider = CultureInfo.GetCultureInfo("pl"),
            Delimiter = ',',
            Encoding = Encoding.UTF8,
            HasHeader = true,
            Quote = '"'
        };
        var rows = reader.ReadFile<SampleRow>(fileName).ToArray();
        Assert.False(reader.IsParserClear);
        var parseException = reader.ParseException;
        Assert.Single(parseException.InnerExceptions);
        var lineException = parseException.InnerExceptions.First() as CsvLineException;
        Assert.Equal(3, lineException!.LineIndex);
        Assert.Equal(3, rows.Length);
        Assert.Equal(4.56m, rows[2]!.Amount);
    }

    private class SampleRow {

        [DateTimeFormat("d.MM.yyyy")]
        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public string? Comment { get; set; }

        public int Number { get; set; }

    }

}
