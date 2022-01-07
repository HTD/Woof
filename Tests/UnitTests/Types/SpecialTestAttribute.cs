namespace UnitTests.Types;

/// <summary>
/// Attribute to test the analysis engine.
/// </summary>
public class SpecialTestAttribute : SpecialAttribute {

    /// <summary>
    /// Gets the special attribute type.
    /// </summary>
    public override object TypeId => typeof(SpecialTestAttribute);

}