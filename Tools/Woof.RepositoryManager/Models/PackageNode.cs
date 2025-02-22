namespace Woof.RepositoryManager.Models;

public record PackageNode : PackageItem, INotifyPropertyChanged {

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsChecked {
        get => _IsChecked;
        set {
            _IsChecked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            if (Dependencies is null) return;
            foreach (var d in Dependencies.All()) d.IsChecked = _IsChecked;
        }
    }

    public bool IsExpanded {
        get => _IsExpanded;
        set {
            _IsExpanded = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
        }
    }

    public ObservableList<PackageNode>? Dependencies { get; set; }

    public PackageNode(string name, string version, ObservableList<PackageNode>? dependencies) : base(name, version) => Dependencies = dependencies;

    public PackageNode(PackageNode node) : base(node) => Dependencies = node.Dependencies;

    public virtual bool Equals(PackageNode? other) => other is PackageNode n && n.Name == Name && n.Version == Version;

    public override int GetHashCode() => HashCode.Combine(Name, Version);

    private bool _IsChecked;
    private bool _IsExpanded;

}
