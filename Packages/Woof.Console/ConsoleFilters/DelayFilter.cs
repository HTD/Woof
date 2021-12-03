namespace Woof.ConsoleFilters;

/// <summary>
/// A toy filter that introduces a delay to console output.
/// </summary>
public class DelayFilter : TextWriter {

    /// <summary>
    /// Character delay in milliseconds.
    /// </summary>
    readonly int Ms;

    /// <summary>
    /// Creates the filter over text output.
    /// </summary>
    /// <param name="ms">Character delay in milliseconds.</param>
    public DelayFilter(int ms = 16) => Ms = ms;

    /// <summary>
    /// Gets bound console encoding.
    /// </summary>
    public override Encoding Encoding => Console.OutputEncoding;

    /// <summary>
    /// Writes a character to the console or tries parse it.
    /// </summary>
    /// <param name="c"></param>
    public override void Write(char c) {
        Out.Write(c);
        Thread.Sleep(Ms);
    }

    /// <summary>
    /// Console out text writer.
    /// </summary>
    public readonly TextWriter Out = Console.Out;

}