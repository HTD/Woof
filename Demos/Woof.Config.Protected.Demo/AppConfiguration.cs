namespace Woof.Config.Protected.Demo;

#pragma warning disable CS8618 // DTO

internal record AppConfiguration {

    public string Login { get; init; }

    public string ApiKey { get; init; }

}

#pragma warning restore CS8618