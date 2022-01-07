using System;
using Woof.Settings;

namespace Test.Server;

internal class Settings : JsonSettings<Settings> {

    internal static Settings Default { get; } = new Settings();

    public Uri? EndPointUri { get; set; }

    public TimeSpan Timeout { get; set; }

    public TestType Test { get; } = new();

    internal record TestType {

        public Guid ClientId { get; set; }

        public Guid UserId { get; set; }

        public string? ApiKey { get; set; }

        public string? Secret { get; set; }

        public Guid StreamId { get; set; }

        public int StreamLength { get; set; }

        public int StreamSeed { get; set; }

    }

}
