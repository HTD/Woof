namespace UnitTests;

public class JsonSettingsTests {

    /// <summary>
    /// Tests the <see cref="JsonNodePath"/> class.
    /// </summary>
    [Fact]
    public void A000_NodePath() {
        Assert.Equal("z", new JsonNodePath("$.x.y.z").Key);
        Assert.Equal("$.x.y.z", new JsonNodePath("$.x.y.z").ToString());
        Assert.Equal("y", new JsonNodePath("$.x.y.z").Parent?.Key);
        Assert.Equal("$.x.y", new JsonNodePath("$.x.y.z").Parent?.ToString());
        Assert.True(new JsonNodePath("$.x[0]").IsIndex);
        Assert.Equal(16384, new JsonNodePath("$.x[16384]").Index);
        Assert.Equal("$.x.y.z", new JsonNodePath("$.x.y", "z").ToString());
        var parts = new JsonNodePath($"$.a.b.c.d").Parts.ToArray();
        Assert.Equal("$", parts[0].Key);
        Assert.Equal("$", parts[0].ToString());
        Assert.Equal("a", parts[1].Key);
        Assert.Equal("$.a", parts[1].ToString());
        Assert.Equal("b", parts[2].Key);
        Assert.Equal("$.a.b", parts[2].ToString());
        Assert.Equal("c", parts[3].Key);
        Assert.Equal("$.a.b.c", parts[3].ToString());
        Assert.Equal("d", parts[4].Key);
        Assert.Equal("$.a.b.c.d", parts[4].ToString());
    }

    /// <summary>
    /// Tests the <see cref="JsonNodeTree.Traverse(JsonNode)"/> method.
    /// </summary>
    [Fact]
    public void A001_JsonNodeTree() {
        var json = @"{""p1"":{""p11"":[1,null,2],""p12"":1},""p2"":null}";
        var node = (JsonNodeLoader.Default.Parse(json) as JsonObject)!;
        var leaves = JsonNodeTree.Traverse(node).ToArray();
        Assert.Equal("$.p2", leaves[0].Path.ToString());
        Assert.Null(leaves[0].Value);
        Assert.Equal("$.p1.p12", leaves[1].Path.ToString());
        Assert.Equal(1, (int?)leaves[1].Value);
        Assert.Equal("$.p1.p11[0]", leaves[2].Path.ToString());
        Assert.Equal(1, (int?)leaves[2].Value);
        Assert.Equal("$.p1.p11[1]", leaves[3].Path.ToString());
        Assert.Null(leaves[3].Value);
        Assert.Equal("$.p1.p11[2]", leaves[4].Path.ToString());
        Assert.Equal(2, (int?)leaves[4].Value);
    }

    /// <summary>
    /// Tests the <see cref="ObjectTree.Traverse(object)"/> method.
    /// </summary>
    [Fact]
    public void A002_ObjectTreeNode() {
        var data = new { p1 = new { p11 = new int?[] { 1, null, 2 }, p12 = 1 }, p2 = "0" };
        var leaves = ObjectTree.Traverse(data).ToArray();
        Assert.Equal("$.p2", leaves[0].Path.ToString());
        Assert.Equal("0", leaves[0].Content as string);
        Assert.Equal("$.p1.p12", leaves[1].Path.ToString());
        Assert.Equal(1, leaves[1].Content as int?);
        Assert.Equal("$.p1.p11[0]", leaves[2].Path.ToString());
        Assert.Equal(1, leaves[2].Content as int?);
        Assert.Equal("$.p1.p11[1]", leaves[3].Path.ToString());
        Assert.Null(leaves[3].Content);
        Assert.Equal("$.p1.p11[2]", leaves[4].Path.ToString());
        Assert.Equal(2, leaves[4].Content as int?);
    }

    /// <summary>
    /// Tests the <see cref="ObjectTree.GetPropertyByPath(object, JsonNodePath)"/> method.
    /// </summary>
    [Fact]
    public void A003_GetPropertyByPath() {
        var data = new { p1 = new { p11 = new int?[] { 1, null, 2 }, p12 = 1 }, p2 = "0" };
        var p11 = ObjectTree.GetPropertyByPath(data, "p1.p11")!;
        var array = p11.Value as Array;
        var v_p11_1 = array!.GetValue(0);
        var v_p11_2 = array!.GetValue(1);
        var v_p11_3 = array!.GetValue(2);
        Assert.Equal(1, v_p11_1);
        Assert.Null(v_p11_2);
        Assert.Equal(2, v_p11_3);
        var p1 = ObjectTree.GetPropertyByPath(data, "p1");
        Assert.Equal(p11.Owner, p1.Value);
    }

    /// <summary>
    /// Tests the trimming of the values present in the CLR target and not present in JSON.
    /// </summary>
    [Fact]
    public void A004_TrimUnset() {
        var data = new TestVectors();
        data.A.X = 0;
        data.A.Y = 1;
        data.B.X = 2;
        data.B.Y = 3;
        var json = @"{""A"":{""X"":1,""Y"":2}}";
        var jsonNode = JsonNodeLoader.Default.Parse(json);
        jsonNode.Bind(data);
    }

    /// <summary>
    /// Tests whether the special properties would be properly ignored when serializing.
    /// </summary>
    [Fact]
    public void A005_SpecialSet() {
        var json = @"{""Regular"":1}";
        var node = JsonNodeLoader.Default.Parse(json);
        var data = new SpecialTestType { Regular = 2, Special = 4 };
        node.Set(data);
        var regularValue = node.Select("Regular")?.AsValue().As<int>();
        var specialValue = node.Select("Special")?.AsValue().As<int>();
        Assert.Equal(2, regularValue);
        Assert.Null(specialValue);
    }

    /// <summary>
    /// Tests whether the special property would be properly resolved.
    /// </summary>
    [Fact]
    public void A006_SpecialGet() {
        var json = @"{""Regular"":1}";
        var node = JsonNodeLoader.Default.Parse(json);
        SpecialAttribute.Resolve += (s, e) => {
            if (s is SpecialTestAttribute) e.Value = 2;
        };
        var data = node.Get<SpecialTestType>();
        Assert.Equal(1, data.Regular);
        Assert.Equal(2, data.Special);
    }

    [Fact]
    public void A007_BindTricky() {
        var direct = @"""Regular"":1,""Private"":1,""Static"":1,""Init"":1,""Internal"":1,""Internal1"":1,""Field"":1,""ReadOnly"":1,""Unknown"":1";
        var nested = @"""Nested"":{""Level1A"":1,""Level1B"":{""Level2"":1}}";
        var json = @$"{{{direct},{nested}}}";
        var node = JsonNodeLoader.Default.Parse(json);
        var instance = new Tricky();
        node.Bind(instance);
        Assert.Equal(1, instance.Regular);
        Assert.Equal(1, instance.Default);
        Assert.Equal(1, instance.Init);
        Assert.Equal(0, Tricky.Static);
        Assert.Equal(0, instance.Internal);
        Assert.Equal(0, instance.Internal1);
        Assert.Equal(0, instance.Field);
        Assert.Equal(0, instance.ReadOnly);
    }

    [Fact]
    public void A008_Utf8DecodeStream() {
        var test = Encoding.UTF8.GetBytes(@"\uD83D\uDC15 \uD83D\uDC15 \uD83D\uDC15");
        using var stream = new MemoryStream();
        var decoding = new Utf8DecodeStream(stream);
        decoding.Write(test);
        decoding.Flush();
        var result = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("🐕 🐕 🐕", result);
    }

    /// <summary>
    /// Tests the complex binding.
    /// </summary>
    [Fact]
    public void A009_BindComplex() {
        var instance = new ComplexBasic();
        var ds1 = @"{""Value"":""SectionA1""}";
        var ds3 = @"{""Value"":""SectionC3""}";
        var isA = $@"{{""Section1"":{ds1},""Value2"":""Value2"",""Value3"":""Value3""}}";
        var isB = $@"{{""AB1"":[{{""Value"":""AB11""}},{{""Value"":""AB12""}}],""AB2"":[{{""Value"":""AB21""}},{{""Value"":""AB22""}}],""AB3"":{{""Value"":""AB31""}}}}";
        var isC = $@"{{""Value1"":""Value1"",""Value2"":""Value2"",""Section3"":{ds3}}}";
        var json = @$"{{""Direct1"":""Direct1"",""SectionA"":{isA},""SectionB"":{isB},""SectionC"":{isC},""Direct2"":""Direct2""}}";
        var rootNode = JsonNodeLoader.Default.Parse(json);
        var indented = rootNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        var jsonLeaves = String.Join(Environment.NewLine, JsonNodeTree.Traverse(rootNode).Select(i => i.ToString()));
        rootNode.Bind(instance);
        instance.AssertEqual(ComplexBasic.Default);
        var newRoot = JsonNodeLoader.Default.Parse("{}");
        newRoot.Set(instance);
        rootNode.Set(instance);
        var newIndented = newRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        var indented2 = rootNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Tests all supported values binding to the direct properties of an object.
    /// </summary>
    [Fact]
    public void A010_GetSetSupported() {
        var node = JsonNodeLoader.Default.Parse("{}");
        var sample = DirectSupported.Default;
        node.Set(sample);
        var json = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        var parsedConfig = JsonNodeLoader.Default.Parse(json);
        var parsedData = parsedConfig.Get<DirectSupported>();
        parsedData.AssertEqual(sample);
    }

    /// <summary>
    /// Tests values binding to a complex object, that is an object that contains objects that contain objects.
    /// </summary>
    [Fact]
    public void A020_GetSetComplex() {
        var node = JsonNodeLoader.Default.Parse("{}");
        var sample = ComplexBasic.Default;
        node.Set(sample);
        var json = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        var parsedConfig = JsonNodeLoader.Default.Parse(json);
        var parsedData = parsedConfig.Get<ComplexBasic>();
        parsedData.AssertEqual(sample);
    }

    /// <summary>
    /// Tests the <see cref="JsonNode"/> Get method on an array of values.
    /// </summary>
    [Fact]
    public void A030_GetArrayOfValues() {
        var node = JsonNodeLoader.Default.Parse(@"{""A"":[1,2,3],""B"":[4,5,6],""N"":[7,8,9]}");
        var result = node.Get<ArrayOfValues>();
        Assert.Equal(1, result.A[0]);
        Assert.Equal(2, result.A[1]);
        Assert.Equal(3, result.A[2]);
        Assert.Equal(4, result.B[0]);
        Assert.Equal(5, result.B[1]);
        Assert.Equal(6, result.B[2]);
        Assert.Equal(7, result.N![0]);
        Assert.Equal(8, result.N![1]);
        Assert.Equal(9, result.N![2]);
    }

    /// <summary>
    /// Tests the <see cref="JsonNode"/> Get method on an array of objects.
    /// </summary>
    [Fact]
    public void A040_GetArrayOfObjects() {
        var a = @"""a"":[{""value"":""A1""},{""value"":""A2""},{""value"":""A3""}]";
        var b = @"""b"":[{""value"":""B1""},{""value"":""B2""},{""value"":""B3""}]";
        var n = @"""n"":[{""value"":""N1""},{""value"":""N2""},{""value"":""N3""}]";
        var json = '{' + a + ',' + b + ',' + n + '}';
        var node = JsonNodeLoader.Default.Parse(json);
        var result = node.Get<ArrayOfObjects>();
        Assert.Equal("A1", result.A[0].Value);
        Assert.Equal("A2", result.A[1].Value);
        Assert.Equal("A3", result.A[2].Value);
        Assert.Equal("B1", result.B[0].Value);
        Assert.Equal("B2", result.B[1].Value);
        Assert.Equal("B3", result.B[2].Value);
        Assert.Equal("N1", result.N![0].Value);
        Assert.Equal("N2", result.N![1].Value);
        Assert.Equal("N3", result.N![2].Value);
    }

    /// <summary>
    /// Tests the <see cref="JsonNode"/> Get method on a list of values.
    /// </summary>
    [Fact]
    public void A050_GetListOfValues() {
        var node = JsonNodeLoader.Default.Parse(@"{""L"":[1,2,3],""N"":[4,5,6]}");
        var result = node.Get<ListOfValues>();
        Assert.Equal(1, result.L[0]);
        Assert.Equal(2, result.L[1]);
        Assert.Equal(3, result.L[2]);
        Assert.Equal(4, result.N![0]);
        Assert.Equal(5, result.N![1]);
        Assert.Equal(6, result.N![2]);
    }

    /// <summary>
    /// Tests the <see cref="JsonNode"/> Get method on a list of objects.
    /// </summary>
    [Fact]
    public void A060_GetListOfObjects() {
        var a = @"""l"":[{""value"":""L1""},{""value"":""L2""},{""value"":""L3""}]";
        var n = @"""n"":[{""value"":""N1""},{""value"":""N2""},{""value"":""N3""}]";
        var json = '{' + a + ',' + n + '}';
        var node = JsonNodeLoader.Default.Parse(json);
        var result = node.Get<ListOfObjects>();
        Assert.Equal("L1", result.L[0].Value);
        Assert.Equal("L2", result.L[1].Value);
        Assert.Equal("L3", result.L[2].Value);
        Assert.Equal("N1", result.N![0].Value);
        Assert.Equal("N2", result.N![1].Value);
        Assert.Equal("N3", result.N![2].Value);
    }

    /// <summary>
    /// Tests the <see cref="JsonNode"/> Set method on a list of objects.
    /// </summary>
    [Fact]
    public void A070_SetListOfObjects() {
        var a = @"""l"":[{""value"":""L1""},{""value"":""L2""},{""value"":""L3""}]";
        var n = @"""n"":[{""value"":""N1""},{""value"":""N2""},{""value"":""N3""}]";
        var json = '{' + a + ',' + n + '}';
        var node = JsonNodeLoader.Default.Parse(json);
        var reference = new ListOfObjects();
        reference.L.Add(new DirectSimple { Value = "L1" });
        reference.L.Add(new DirectSimple { Value = "L2" });
        reference.N = new List<DirectSimple> { new() { Value = "N1" }, new() { Value = "N2" } };
        node.Set(reference);
        var result = node.Get<ListOfObjects>();
        Assert.Equal(reference.N.Count + 1, result.N!.Count);
        Assert.Equal(reference.L.Count + 1, result.L!.Count);
    }

    /// <summary>
    /// Tests the asynchronous section loading.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the test is done.</returns>
    [Fact]
    public async ValueTask A200_LoadAsync() {
        const string testInput = @"{""level1"":{""test"":[{""x"":1,""y"":2},{""z"":3},""surprise""]}}";
        using var testStream = new MemoryStream(Encoding.UTF8.GetBytes(testInput));
        testStream.Position = 0;
        var node = await new JsonNodeLoader().LoadAsync(testStream);
        Assert.Equal(1, node.GetValue<int>("Level1:Test:0:x"));
        Assert.Equal(2, node.GetValue<int>("Level1:Test:0:y"));
        Assert.Equal(3, node.GetValue<int>("Level1:Test:1:z"));
        Assert.Equal("surprise", node.GetValue<string>("Level1:Test:2"));
    }

    /// <summary>
    /// Tests the asynchronous section saving.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the test is done.</returns>
    [Fact]
    public async ValueTask A210_SaveAsync() {
        var initial = JsonNodeLoader.Default.Parse(@"{""n"":null,""b"":false,""s"":""initial"",""i"":0,""d"":0.123456789}");
        initial["z"] = "a new value";
        initial["n"] = "not null";
        initial["b"] = "true";
        initial["s"] = "test";
        initial["i"] = "0.123456789";
        initial["d"] = "42";
        await using var testStream = new MemoryStream();
        await JsonNodeLoader.Default.SaveAsync(initial, testStream);
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

}