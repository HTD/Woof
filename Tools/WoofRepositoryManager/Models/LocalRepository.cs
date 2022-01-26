namespace WoofRepositoryManager.Models;

/// <summary>
/// Local package repository.
/// </summary>
public static class LocalRepository {

    /// <summary>
    /// Gets the toolkit root.
    /// </summary>
    public static string Root => System.IO.Path.GetFullPath(Settings.Default.Paths.Root);

    /// <summary>
    /// Gets a full path to the local repository.
    /// </summary>
    public static string Path => System.IO.Path.Combine(Root, Settings.Default.Paths.Repo);

    /// <summary>
    /// Gets the repository prefix.
    /// </summary>
    public static string Prefix => Settings.Default.Prefix;

    /// <summary>
    /// Asserts the settings loaded.
    /// </summary>
    static LocalRepository() => Settings.Default.Assert();

    /// <summary>
    /// Gets all packages metadata from the configured local repository.
    /// </summary>
    /// <returns>Asynchronous collection of <see cref="IPackageSearchMetadata"/>.</returns>
    public static async IAsyncEnumerable<IPackageSearchMetadata> GetPackagesAsync() {
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