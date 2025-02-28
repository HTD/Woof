﻿namespace Woof.RepositoryManager.Models;

/// <summary>
/// Extension methods for NuGet package collections.
/// </summary>
public static class PackageCollectionTraits {

    /// <summary>
    /// Gets all packages (including local dependencies) in the collection in DFS order.
    /// </summary>
    /// <param name="items">Packages with dependencies.</param>
    /// <returns>Packages with dependencies.</returns>
    public static IEnumerable<PackageNode> All(this ObservableList<PackageNode> items) => items.TraverseDepthFirst(i => i.Dependencies);

    /// <summary>
    /// Gets the packages from the local repository.
    /// </summary>
    /// <param name="items">Target list to fill asynchronously.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the all the packages are fetched from the local repository.</returns>
    public static async ValueTask GetAsync(this ObservableList<PackageNode> items) {
        items.Clear();
        await foreach (var package in LocalRepository.GetPackagesAsync()) {
            var lastVersion = (await package.GetVersionsAsync())?.Max(i => i.Version)?.ToString();
            var name = package.Identity.Id;
            var version = lastVersion;
            ObservableList<PackageNode>? dependencies = null;
            if (package.DependencySets.FirstOrDefault() is PackageDependencyGroup group) {
                foreach (var dependency in group.Packages) {
                    if (dependency.Id.StartsWith(Settings.Default.Prefix, StringComparison.Ordinal)) {
                        dependencies ??= [];
                        dependencies.Add(new(dependency.Id, dependency!.VersionRange.OriginalString!, null));
                    }
                }
            }
            items.Add(new(name, version!, dependencies));
        }
        var reference = items.ToArray();
        foreach (var item in reference.TraverseDepthFirst(i => i.Dependencies)) {
            var direct = reference.First(i => i.Name == item.Name);
            if (item.Dependencies is null && direct.Dependencies is not null) {
                item.Dependencies = [];
                foreach (var d in direct.Dependencies) item.Dependencies.Add(new(d));
            }
        }
    }

    /// <summary>
    /// Gets all the packages (including local dependencies) that are currently checked.
    /// </summary>
    /// <param name="items">Packages bound to the view.</param>
    /// <returns>All package items that are checked in the view.</returns>
    public static IEnumerable<PackageNode> GetChecked(this IEnumerable<PackageNode> items)
        => items.Where(i => i.IsChecked).TraversePostOrder(n => n.Dependencies).Distinct().Where(i => i.IsChecked);
    
}
