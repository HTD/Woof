namespace Publish.Models;

public record PackageItem {

    public string Name { get; }

    public string Version { get; }

    public PackageItem(string name, string version) {
        Name = name;
        Version = version;
    }
}
