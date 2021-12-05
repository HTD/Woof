using Woof.Config;
using Woof.Config.Internals;
using UnitTests.TestSubjects;
using Microsoft.Extensions.Configuration;

namespace UnitTests {

    public class JsonConfigTests {

        [Fact]
        public void A010_Direct() {
            var config = JsonNodeSection.Loader.Parse(@"{""b"":true,""i"":1,""f"":3.1415926535,""s"":""0""}");
            Assert.True(config.GetValue<bool>("b"));
            Assert.Equal(1, config.GetValue<int>("i"));
            Assert.Equal(3.1415926535, config.GetValue<double>("f"));
            Assert.Equal(3.1415927f, config.GetValue<float>("f"));
            Assert.Equal(3.1415926535m, config.GetValue<decimal>("f"));
            Assert.Equal("0", config.GetValue<string>("s"));
        }

        [Fact]
        public void A020_Nested() {
            var config = JsonNodeSection.Loader.Parse(@"{""level1"":{""test"":1}}");
            Assert.Equal(1, config.GetValue<int>("Level1:Test"));
        }

        [Fact]
        public void A030_Array() {
            var config = JsonNodeSection.Loader.Parse(@"{""level1"":{""test"":[1,2,3]}}");
            Assert.Equal(1, config.GetValue<int>("Level1:Test:0"));
            Assert.Null(config.GetValue<int?>("Level1:Test:3"));
            Assert.Null(config.GetValue<string?>("Level1:Test:Test"));
            Assert.Equal(-1, config.GetValue("Level1:Test:4", -1));
            Assert.Equal(-1, config.GetValue("Level1:Error", -1));
        }

        [Fact]
        public void A040_ArrayOfSections() {
            var config = JsonNodeSection.Loader.Parse(@"{""level1"":{""test"":[{""x"":1,""y"":2},{""z"":3},""surprise""]}}");
            Assert.Equal(1, config.GetValue<int>("Level1:Test:0:x"));
            Assert.Equal(2, config.GetValue<int>("Level1:Test:0:y"));
            Assert.Equal(3, config.GetValue<int>("Level1:Test:1:z"));
            Assert.Equal("surprise", config.GetValue<string>("Level1:Test:2"));
        }

        [Fact]
        public void A050_NestedArrays() {
            var config = JsonNodeSection.Loader.Parse(@"{""root"":[[1],[1,2],[1,2,3,[1,2,3,4,5]]]}");
            Assert.Equal(5, config.GetValue<int>("root:2:3:4"));
        }

        [Fact]
        public void A060_CaseSensitiveMatching() {
            var config = JsonNodeSection.Loader.Parse(@"{""level1"":{""test"":1}}", caseSensitive: true);
            Assert.Equal(1, config.GetValue<int>("level1:test"));
            Assert.Null(config.GetValue<int?>("Level1:Test"));
        }

        [Fact]
        public void A070_AdvancedFeatures() {
            var testId = Guid.NewGuid();
            var testDateString = "1986-04-26";
            var testDouble = 2.5;
            var testTimeout = TimeSpan.FromSeconds(testDouble);
            var defaultTimeout = TimeSpan.FromSeconds(5);
            var config = JsonNodeSection.Loader.Parse(@$"{{
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

        [Fact]
        public void A080_Bindig() {
            var basicTypeConfig = JsonNodeSection.Loader.Parse(@"{""p1"":true,""p2"":1,""p3"":""test""}");
            var basicTypeData = basicTypeConfig.Get<BasicType>();
            Assert.True(basicTypeData.P1);
            Assert.Equal(1, basicTypeData.P2);
            Assert.Equal("test", basicTypeData.P3);
            var comlexTypeConfig = JsonNodeSection.Loader.Parse(@"{""s1"":{""p1"":true,""p2"":1,""p3"":""test""},""s2"":{""p1"":false,""p2"":0,""p3"":""spam""}}");
            var comlexTypeData = comlexTypeConfig.Get<ComplexType>();
            Assert.True(comlexTypeData.S1.P1);
            Assert.Equal(1, comlexTypeData.S1.P2);
            Assert.Equal("test", comlexTypeData.S1.P3);
            Assert.False(comlexTypeData.S2.P1);
            Assert.Equal(0, comlexTypeData.S2.P2);
            Assert.Equal("spam", comlexTypeData.S2.P3);
        }

        [Fact]
        public void A090_WriteSupport() {
            var config = JsonNodeSection.Loader.Parse(@"{""n"":null,""b"":false,""s"":""initial"",""i"":0,""d"":0.123456789}");
            config["b"] = "true";
            config["s"] = "test";
            config["i"] = "0.123456789";
            config["d"] = "42";
            Assert.True(config.GetValue<bool>("b"));
            Assert.Equal("test", config.GetValue<string>("s"));
            Assert.Equal(0.123456789, config.GetValue<double>("i"));
            Assert.Equal(42, config.GetValue<int>("d"));
            try {
                config["n"] = "."; // it is not yet defined what should be written for an unknown type...
            }
            catch (NotImplementedException) { } // ...so it obvously throws "not implemented".
        }

        [Fact]
        public async ValueTask A100_LoadAsync() {
            const string testInput = @"{""level1"":{""test"":[{""x"":1,""y"":2},{""z"":3},""surprise""]}}";
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes(testInput));
            testStream.Position = 0;
            var config = await JsonNodeSection.Loader.LoadAsync(testStream);
            Assert.Equal(1, config.GetValue<int>("Level1:Test:0:x"));
            Assert.Equal(2, config.GetValue<int>("Level1:Test:0:y"));
            Assert.Equal(3, config.GetValue<int>("Level1:Test:1:z"));
            Assert.Equal("surprise", config.GetValue<string>("Level1:Test:2"));
        }

        [Fact]
        public async ValueTask A110_SaveAsync() {
            var initial = JsonNodeSection.Loader.Parse(@"{""n"":null,""b"":false,""s"":""initial"",""i"":0,""d"":0.123456789}");
            initial["b"] = "true";
            initial["s"] = "test";
            initial["i"] = "0.123456789";
            initial["d"] = "42";
            await using var testStream = new MemoryStream();
            await initial.SaveAsync(testStream);
            testStream.Position = 0;
            var loaded = await JsonNodeSection.Loader.LoadAsync(testStream);
            Assert.True(loaded.GetValue<bool>("b"));
            Assert.Equal("test", loaded.GetValue<string>("s"));
            Assert.Equal(0.123456789, loaded.GetValue<double>("i"));
            Assert.Equal(42, loaded.GetValue<int>("d"));
            testStream.Position = 0;
            using var textReader = new StreamReader(testStream);
            var output = await textReader.ReadToEndAsync();
        }

        [Fact]
        public void A120_MissingPropertiesBinding() {
            var config = JsonNodeSection.Loader.Parse(@"{""n"":null,""b"":false,""s"":""initial"",""i"":0,""d"":0.123456789}");
            Assert.Equal(1, config.GetValue("Level1:Test:0:x", 1));
            Assert.Equal(2, config.GetValue("Level1:Test:0:y", 2));
            Assert.Equal(3, config.GetValue("Level1:Test:1:z", 3));
            Assert.Equal("surprise", config.GetValue("Level1:Test:2", "surprise"));
        }

        [Fact]
        public void Traverse() => Assert.Equal(7, PropertyTraverser.Traverse(new NestedRoot()).Count());

        [Fact]
        public void NullNodes() {
            var config = JsonNodeSection.Loader.Parse(@"{""p1"":null,""p2"":{},""p3"":{""p31"":""test"",""p32"":null}}");
            //var p1 = config.GetSection("p1");
            //var p2 = config.GetSection("p2");
            //var p31 = config.GetSection("p3:p31");
            var p32 = config.GetSection("p3:p32");
            ;
        }

    }

}

#region Test subjects

namespace UnitTests.TestSubjects {

    record BasicType {

        public bool P1 { get; init; }

        public int P2 { get; init; }

        public string P3 { get; init; } = string.Empty;

        public DateOnly Unused { get; init; }

    }

    record ComplexType {

        public BasicType S1 { get; } = new();

        public BasicType S2 { get; } = new();

        public TimeOnly Unused { get; init; }

    }

    record NestedRoot {

        public bool R1 { get; init; } // $.R1

        public NestedBranch_1 R2 { get; } = new();

        public int R3 { get; init; } // $.R3

        public NestedBranch_2 R4 { get; } = new();

        public string? R5 { get; init; } // $.R5

    }

    record NestedBranch_1 {

        public int B1 { get; init; } // $.R2.B1

        public NestedBranch_11 B2 { get; } = new();

        public Guid B3 { get; init; }

    }

    record NestedBranch_11 {

        public int B111 { get; init; } // $.R2.B2.B111

    }

    record NestedBranch_2 {

        public int B21 { get; init; } // $.R4.B21

    }

}

#endregion