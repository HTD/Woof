namespace UnitTests.Types;

public class ComplexBasic {

    public string? Direct1 { get; set; }

    public IntermediateA SectionA { get; } = new();

    public IntermediateB SectionB { get; set; } = new();

    public IntermediateC? SectionC { get; set; }

    public string? Direct2 { get; set; }

    public static ComplexBasic Default {
        get {
            var instance = new ComplexBasic();
            instance.Direct1 = nameof(instance.Direct1);
            instance.Direct2 = nameof(instance.Direct2);
            instance.SectionA.Section1.Value = "SectionA1";
            instance.SectionA.Value2 = nameof(instance.SectionA.Value2);
            instance.SectionA.Value3 = nameof(instance.SectionA.Value3);
            instance.SectionB.AB1 = [new() { Value = "AB11" }, new() { Value = "AB12" }];
            //instance.SectionB.AB2 = new(); // test existing case
            instance.SectionB.AB2.AddRange(new DirectSimple[] { new() { Value = "AB21" }, new() { Value = "AB22" } });
            instance.SectionB.AB3 = new DirectSimple { Value = "AB31" };
            instance.SectionC = new();
            instance.SectionC.Value1 = nameof(instance.SectionC.Value1);
            instance.SectionC.Value2 = nameof (instance.SectionC.Value2);
            instance.SectionC.Section3.Value = "SectionC3";
            return instance;
        }
    }

    public void AssertEqual(ComplexBasic other) {
        Assert.Equal(other.Direct1, Direct1);
        Assert.Equal(other.Direct2, Direct2);
        Assert.Equal(other.SectionA.Section1.Value, SectionA.Section1.Value);
        Assert.Equal(other.SectionA.Value2, SectionA.Value2);
        Assert.Equal(other.SectionA.Value3, SectionA.Value3);
        Assert.True(other.SectionB.AB1!.Select(i => i.Value)!.SequenceEqual(SectionB.AB1!.Select(i => i.Value)));
        Assert.True(other.SectionB.AB2!.Select(i => i.Value)!.SequenceEqual(SectionB.AB2!.Select(i => i.Value)));
        Assert.Equal(other.SectionB.AB3!.Value, SectionB.AB3!.Value);
        Assert.Equal(other.SectionC!.Value1, SectionC!.Value1);
        Assert.Equal(other.SectionC.Value2, SectionC.Value2);
        Assert.Equal(other.SectionC.Section3.Value, SectionC.Section3.Value);
    }

}