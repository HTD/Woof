using Woof.Config;
using System.Text.Json;
using UnitTests.Types;

namespace UnitTests;

public class JsonConfigTests {

    /// <summary>
    /// Tests the <see cref="NodePath"/> processing tool.
    /// </summary>
    [Fact]
    public void A010_NodePath() {
        Assert.Equal("one", NodePath.GetSectionPath("one"));
        Assert.Equal("one", NodePath.GetSectionPath("$.one"));
        Assert.Equal("one:two", NodePath.GetSectionPath("one.two"));
        Assert.Equal("one:two", NodePath.GetSectionPath("one:two"));
        Assert.Equal("one:two", NodePath.GetSectionPath("$.one.two"));
        Assert.Equal("one:two:three", NodePath.GetSectionPath("one.two.three"));
        Assert.Equal("one:two:three", NodePath.GetSectionPath("one:two:three"));
        Assert.Equal("one:two:three", NodePath.GetSectionPath("$.one.two.three"));
        Assert.Equal("one:two:three:four", NodePath.GetSectionPath("$.one.two.three[four]"));
        var nodePath = new NodePath("$.a.b.c");
        var parts = nodePath.Parts.ToArray();
        Assert.Equal("a", parts[0].Key);
        Assert.Equal("a", parts[0].Path);
        Assert.Equal("b", parts[1].Key);
        Assert.Equal("a:b", parts[1].Path);
        Assert.Equal("c", parts[2].Key);
        Assert.Equal("a:b:c", parts[2].Path);
        Assert.Equal("b", nodePath.Parent.Key);
        Assert.Equal("a:b", nodePath.Parent.Path);
    }

    /// <summary>
    /// Tests if the <see cref="JsonNodeSection.Select(string)"/> returns what it should return.
    /// </summary>
    /// <remarks>
    /// It is crucial that separate empty and null nodes should be returned with the correct paths and parent nodes.
    /// </remarks>
    [Fact]
    public void A020_Select() {
        var section = JsonNodeSection.Parse(@"{""s1"":{""n1"":null,""s2"":{""n2"":null,""a"":[1,{},[],null]}},""v1"":true}");
        var v1 = section.Select("v1");
        var s1 = section.Select("s1");
        var s2 = section.Select("s1:s2");
        var s3 = section.Select("s1:s2:s3");
        var s4 = section.Select("s1:s2:s3:s4");
        var a = section.Select("s1:s2:a");
        var a1 = section.Select("s1:s2:a:0");
        var b1 = section.Select("s1:s2").GetSection("a:0");
        var a2 = section.Select("s1:s2:a:1");
        var a3 = section.Select("s1:s2:a:2");
        var a4 = section.Select("s1:s2:a:3");
        var a5 = section.Select("s1:s2:a:4");
        var n1 = section.Select("s1:n1");
        var n2 = section.Select("s1:s2:n2");
        var n3 = section.Select("s3:n3");
        Assert.Equal("true", v1.Value);
        Assert.Equal(JsonNodeType.Object, s1.NodeType);
        Assert.Equal(JsonNodeType.Object, s2.NodeType);
        Assert.Equal(JsonNodeType.Empty, s3.NodeType);
        Assert.NotNull(s3.Parent);
        Assert.Equal(JsonNodeType.Empty, s4.NodeType);
        Assert.Null(s4.Parent);
        Assert.Equal(JsonNodeType.Empty, n3.NodeType);
        Assert.Equal(JsonNodeType.Null, n1.NodeType);
        Assert.Equal(JsonNodeType.Null, n2.NodeType);
        Assert.Equal(JsonNodeType.Array, a.NodeType);
        Assert.Equal(JsonNodeType.Value, a1.NodeType);
        Assert.Equal(JsonNodeType.Object, a2.NodeType);
        Assert.Equal(JsonNodeType.Array, a3.NodeType);
        Assert.Equal(JsonNodeType.Null, a4.NodeType);
        Assert.Equal(JsonNodeType.Empty, a5.NodeType);
        Assert.Equal("n1", n1.Key);
        Assert.Equal("s1:n1", n1.Path);
        Assert.Equal("n2", n2.Key);
        Assert.Equal("s1:s2:n2", n2.Path);
        Assert.Equal("1", a1.Value);
        Assert.Equal("1", b1.Value);
    }

    /// <summary>
    /// Tests the node building using the section setter.
    /// </summary>
    [Fact]
    public void A030_NodeBuilding() {
        var section = JsonNodeSection.Parse("{}");
        section["v1"] = "=true";
        //section["s1"] = "{}"; // the node setter should take care of this...
        section["s1:v1"] = "=1";
        //section["s1:s11"] = "{}"; // ...and this...
        section["s1:s11:v1"] = "=1";
        //section["s1:a11"] = "[]"; // ...and this.
        section["s1:a11:0"] = "=1";
        section["s1:a11:1"] = "=2";
        section["s1:a11:2"] = "=3";
        var serialized = section.Node!.ToString();
        var reference = JsonNodeSection.Parse(serialized);
        Assert.True(reference.GetValue("v1", false));
        Assert.Equal(1, reference.GetValue("s1:v1", 0));
        Assert.Equal(1, reference.GetValue("s1:a11:0", 0));
        Assert.Equal(2, reference.GetValue("s1:a11:1", 1));
        Assert.Equal(3, reference.GetValue("s1:a11:2", 2));
    }

    /// <summary>
    /// Tests the basic array processing vs <see cref="JsonNodeSection.GetValue{T}(string)"/>.
    /// </summary>
    [Fact]
    public void A040_Array() {
        var section = JsonNodeSection.Parse(@"{""level1"":{""test"":[1,2,3]}}");
        Assert.Equal(1, section.GetValue<int>("Level1:Test:0"));
        Assert.Null(section.GetValue<int?>("Level1:Test:3"));
        Assert.Null(section.GetValue<string?>("Level1:Test:Test"));
        Assert.Equal(-1, section.GetValue("Level1:Test:4", -1));
        Assert.Equal(-1, section.GetValue("Level1:Error", -1));
    }

    /// <summary>
    /// Tests the arrays of sections vs <see cref="JsonNodeSection.GetValue{T}(string)"/>.
    /// </summary>
    [Fact]
    public void A050_ArrayOfSections() {
        var section = JsonNodeSection.Parse(@"{""level1"":{""test"":[{""x"":1,""y"":2},{""z"":3},""surprise""]}}");
        Assert.Equal(1, section.GetValue<int>("Level1:Test:0:x"));
        Assert.Equal(2, section.GetValue<int>("Level1:Test:0:y"));
        Assert.Equal(3, section.GetValue<int>("Level1:Test:1:z"));
        Assert.Equal("surprise", section.GetValue<string>("Level1:Test:2"));
    }

    /// <summary>
    /// Tests the nested array feature vs <see cref="JsonNodeSection.GetValue{T}(string)"/>.
    /// </summary>
    [Fact]
    public void A060_NestedArrays() {
        var section = JsonNodeSection.Parse(@"{""root"":[[1],[1,2],[1,2,3,[1,2,3,4,5]]]}");
        Assert.Equal(5, section.GetValue<int>("root:2:3:4"));
    }

    /// <summary>
    /// Tests the case sensitive property matching.
    /// </summary>
    [Fact]
    public void A070_CaseSensitiveMatching() {
        var section = JsonNodeSection.Parse(@"{""level1"":{""test"":1}}", caseSensitive: true);
        Assert.Equal(1, section.GetValue<int>("level1:test"));
        Assert.Null(section.GetValue<int?>("Level1:Test"));
    }

    /// <summary>
    /// Tests the comments ignoring feature.
    /// </summary>
    [Fact]
    public void A080_Comments() {
        var testId = Guid.NewGuid();
        var testDateString = "1986-04-26";
        var testDouble = 2.5;
        var testTimeout = TimeSpan.FromSeconds(testDouble);
        var defaultTimeout = TimeSpan.FromSeconds(5);
        var config = JsonNodeSection.Parse(@$"{{
            ""id"": ""{testId}"", // test comment 1
            ""date"":""{testDateString}"", // test comment 2
            ""timeout"": {testDouble.ToString(CultureInfo.InvariantCulture)} // {{ this should be ignored }}
        }}");
        Assert.Equal(testId, config.GetValue<Guid>("id"));
        Assert.Null(config.GetValue<Guid?>("id2"));
        Assert.Equal(testId, config.GetValue<Guid>("id2", testId));
        Assert.Equal(DateTime.Parse(testDateString), config.GetValue<DateTime>("date"));
        Assert.Equal(testDouble, config.GetValue("timeout", 5.0));
        Assert.Equal(testTimeout, TimeSpan.FromSeconds(config.GetValue("timeout", 5.0)));
        Assert.Equal(defaultTimeout, TimeSpan.FromSeconds(config.GetValue("timeout1", 5.0)));
    }

    /// <summary>
    /// Tests all supported values binding to the direct properties of an object.
    /// </summary>
    [Fact]
    public void A090_SimpleBinding() {
        var config = JsonNodeSection.Parse("{}");
        var sample = DirectSupported.Default;
        config.Set(sample);
        var json = config.Node!.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        var parsedConfig = JsonNodeSection.Parse(json);
        var parsedData = parsedConfig.Get<DirectSupported>();
        parsedData.AssertEqual(sample);
    }

    /// <summary>
    /// Tests values binding to a complex object, that is an object that contains sections that contain sections.
    /// </summary>
    [Fact]
    public void A100_ComplexBinding() {
        var config = JsonNodeSection.Parse("{}");
        var sample = ComplexBasic.Default;
        config.Set(sample);
        var json = config.Node!.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        var parsedConfig = JsonNodeSection.Parse(json);
        var parsedData = parsedConfig.Get<ComplexBasic>();
        parsedData.AssertEqual(sample);
    }

    /// <summary>
    /// Tests the asynchronous section loading.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the test is done.</returns>
    [Fact]
    public async ValueTask A110_LoadAsync() {
        const string testInput = @"{""level1"":{""test"":[{""x"":1,""y"":2},{""z"":3},""surprise""]}}";
        using var testStream = new MemoryStream(Encoding.UTF8.GetBytes(testInput));
        testStream.Position = 0;
        var config = await new JsonNodeLoader().LoadAsync(testStream);
        Assert.Equal(1, config.GetValue<int>("Level1:Test:0:x"));
        Assert.Equal(2, config.GetValue<int>("Level1:Test:0:y"));
        Assert.Equal(3, config.GetValue<int>("Level1:Test:1:z"));
        Assert.Equal("surprise", config.GetValue<string>("Level1:Test:2"));
    }

    /// <summary>
    /// Tests the asynchronous section saving.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the test is done.</returns>
    [Fact]
    public async ValueTask A120_SaveAsync() {
        var initial = JsonNodeSection.Parse(@"{""n"":null,""b"":false,""s"":""initial"",""i"":0,""d"":0.123456789}");
        initial["z"] = "a new value";
        initial["n"] = "not null";
        initial["b"] = "true";
        initial["s"] = "test";
        initial["i"] = "0.123456789";
        initial["d"] = "42";
        await using var testStream = new MemoryStream();
        await initial.SaveAsync(testStream);
        testStream.Position = 0;
        var loaded = await new JsonNodeLoader().LoadAsync(testStream);
        Assert.Equal("not null", loaded.GetValue<string?>("n"));
        Assert.True(loaded.GetValue<bool>("b"));
        Assert.Equal("test", loaded.GetValue<string>("s"));
        Assert.Equal(0.123456789, loaded.GetValue<double>("i"));
        Assert.Equal(42, loaded.GetValue<int>("d"));
        Assert.Equal("a new value", loaded.GetValue<string?>("z"));
        testStream.Position = 0;
        using var textReader = new StreamReader(testStream);
        var output = await textReader.ReadToEndAsync();
    }

    /// <summary>
    /// Tests reading of the missing properties.
    /// </summary>
    [Fact]
    public void A130_MissingProperties() {
        var config = JsonNodeSection.Parse(@"{""n"":null,""b"":false,""s"":""initial"",""i"":0,""d"":0.123456789}");
        Assert.Equal(1, config.GetValue("Level1:Test:0:x", 1));
        Assert.Equal(2, config.GetValue("Level1:Test:0:y", 2));
        Assert.Equal(3, config.GetValue("Level1:Test:1:z", 3));
        Assert.Equal("surprise", config.GetValue("Level1:Test:2", "surprise"));
    }

}