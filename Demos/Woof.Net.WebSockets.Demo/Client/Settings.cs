using System;
using Woof.Settings;

namespace Test.Client;

internal class Settings : JsonSettings<Settings> {

    public static Settings Default { get; } = new Settings();

    public Uri? EndPointUri { get; set; }

    public double Timeout { get; set; }

    public CredentialsType Credentials { get; } = new CredentialsType();

    public TestType Test { get; } = new TestType();

    internal record CredentialsType {

        public Guid ClientId { get; set; }

        public Guid UserId { get; set; }

        public string? ApiKey { get; set; }

        public string? Secret { get; set; }

    }

    internal record TestType {

        public Guid StreamId { get; set; }

        public int StreamLength { get; set; }

        public int StreamSeed { get; set; }

    }

}