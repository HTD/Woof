using System;

namespace Test.Client;

/// <summary>
/// Annotates the method as the test delegate.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
class TestAttribute : Attribute {

    /// <summary>
    /// Creates a new <see cref="TestAttribute"/>.
    /// </summary>
    /// <param name="description">Optional test description.</param>
    public TestAttribute(string? description = default) => Description = description;

    /// <summary>
    /// Gets or sets the optional test description.
    /// </summary>
    public string? Description { get; set; }

}