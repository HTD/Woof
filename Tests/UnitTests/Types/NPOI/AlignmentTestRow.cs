using NPOI.SS.UserModel;

namespace UnitTests.Types;

internal class AlignmentTestRow {

    [XCell(Alignment = HorizontalAlignment.Left)]
    public string LeftText { get; set; } = "L";

    [XCell(Alignment = HorizontalAlignment.Center)]
    public string CenterText { get; set; } = "C";

    [XCell(Alignment = HorizontalAlignment.Right)]
    public string RightText { get; set; } = "R";

}
