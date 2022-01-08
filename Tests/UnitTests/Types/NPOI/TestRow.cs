using NPOI.SS.UserModel;

namespace UnitTests.Types;

/// <summary>
/// A very basic test row to generate a spreadsheet from.
/// </summary>
[XSheet(Name = "Test")] //, IsSumInTheFirstRow = true, SumLabel = "Total:"
internal class TestRow {

    /// <summary>
    /// Gets or sets the accounting time.
    /// </summary>
    [XCell(Name = "Date", DataFormat = "dd/mm/yyyy", Width = 11)]
    public DateTime Date { get; set; }

    [XCell(DataFormat = "hh:mm")]
    public DateTime? Time { get; set; }

    [XCell(DataFormat = "hh:mm")]
    public TimeSpan TimeSpan { get; set; }

    [XCell(Alignment = HorizontalAlignment.Left)]
    public string LeftTxt { get; set; } = "L";

    [XCell(Alignment = HorizontalAlignment.Center)]
    public string CenterTxt { get; set; } = "C";

    [XCell(Alignment = HorizontalAlignment.Right)]
    public string RightTxt { get; set; } = "R";

    [XCell(Skip = true)]
    public int HideMe { get; set; }

    /// <summary>
    /// Gets or sets the accounting amount.
    /// </summary>
    [XCell(AddSum = true)]
    public decimal Amount { get; set; }

    [XCell(AddSum = true)]
    public double Number { get; set; }

    [XCell(DataFormat = "@")]
    public double NumberAsText { get; set; }

    /// <summary>
    /// Gets or sets the optional comment.
    /// </summary>
    [XCell(Name = "Comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Gets a sample of test rows.
    /// </summary>
    /// <param name="rowCount">Number of rows to generate.</param>
    /// <returns>A test row sample.</returns>
    public static IEnumerable<TestRow> GetSample(int rowCount) {
        var startDate = DateTime.Now.AddYears(-1);
        var timeIncrement = TimeSpan.FromDays(1);
        var minAmount = -10000m;
        var maxAmount = 10000m;
        var prng = new Random();
        var getAmount = () => Math.Round((decimal)prng.NextDouble() * (maxAmount - minAmount) + minAmount, 2);
        for (var i = 0; i < rowCount; i++) yield return new TestRow {
            Date = startDate + i * timeIncrement,
            Time = startDate - i * timeIncrement,
            TimeSpan = TimeSpan.FromMinutes(i),
            Amount = getAmount(),
            Number = prng.NextDouble(),
            NumberAsText = prng.NextDouble(),
            Comment = "The quick brown fox jumps over the lazy dog."
        };
    }

}