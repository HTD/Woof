namespace UnitTests;

public class NPOITest {

    private readonly ITestOutputHelper Output;

    public NPOITest(ITestOutputHelper output) => Output = output;

    /// <summary>
    /// Tests the sample creation. See the output.
    /// </summary>
    [Fact]
    public void A010_Sample() {
        foreach (var row in TestRow.GetSample(1))
            Output.WriteLine($"{row.Date}: {row.Amount}");
    }

    /// <summary>
    /// Tests the Excel file creation. See the output directory for the test file created.
    /// </summary>
    [Fact]
    public void A015_ToExcel_Alignment() {
        List<AlignmentTestRow> test = new();
        test.Add(new AlignmentTestRow());
        //test.Add(new AlignmentTestRow());
        test.ToExcel("AlignmentTest.xlsx");
    }

    /// <summary>
    /// Tests the Excel file creation. See the output directory for the test file created.
    /// </summary>
    [Fact]
    public void A020_ToExcel() => TestRow.GetSample(1).ToExcel("Test.xlsx");

}