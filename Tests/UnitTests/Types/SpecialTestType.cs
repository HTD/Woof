namespace UnitTests.Types;
public class SpecialTestType {

    public int Regular { get; set; }

    [SpecialTest]
    public int Special { get; set; }

}
