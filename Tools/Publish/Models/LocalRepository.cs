namespace Publish.Models;

/// <summary>
/// Local package repository.
/// </summary>
public class LocalRepository {

    /// <summary>
    /// Gets a full path to the local repository.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the repository prefix.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Gets the toolkit root.
    /// </summary>
    public string Root { get; }

    /// <summary>
    /// Creates the new local repository instance.
    /// </summary>
    public LocalRepository() {
        if (!Settings.Default.IsLoaded) throw new InvalidOperationException("Repository settings not loaded");
        Root = System.IO.Path.GetFullPath(Settings.Default.Paths.Root);
        Path = System.IO.Path.Combine(Root, Settings.Default.Paths.Repo);
        Prefix = Settings.Default.Prefix;
    }

    /// <summary>
    /// Gets all packages metadata from the configured local repository.
    /// </summary>
    /// <returns>Asynchronous collection of <see cref="IPackageSearchMetadata"/>.</returns>
    public async IAsyncEnumerable<IPackageSearchMetadata> GetPackagesAsync() {
        var listResource = await Repository
            .CreateSource(Repository.Provider.GetCoreV3(), new PackageSource(Path), FeedType.FileSystemV3)
            .GetResourceAsync<ListResource>();
        var packages = await listResource.ListAsync(Prefix, prerelease: true, allVersions: false, includeDelisted: false, NullLogger.Instance, CancellationToken.None);
        var enumerator = packages.GetEnumeratorAsync();
        while (await enumerator.MoveNextAsync()) {
            var item = enumerator.Current;
            if (!item.Identity.Id.Contains("Template")) yield return item;
        }
    }

}