namespace Woof.NPOI;

/// <summary>
/// Represents an <see cref="ActionResult"/> that when executed will write
/// a file from a stream to the response.
/// </summary>
/// <typeparam name="TItem">Item type.</typeparam>
public class XWorkbookResult<TItem> : FileStreamResult {

    /// <summary>
    /// Creates the <see cref="ActionResult"/> from the item collection.
    /// </summary>
    /// <param name="items">Items for rows.</param>
    /// <param name="fileDownloadName">The default file name for the download.</param>
    public XWorkbookResult(IEnumerable<TItem> items, string fileDownloadName)
        : base(items.ToXSSFWorkbook().ToMemoryStream(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") => FileDownloadName = fileDownloadName;

}
