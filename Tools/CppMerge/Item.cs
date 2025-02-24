using Woof.Internals;

namespace CppMerge;

/// <summary>
/// Provides dependency logic for C/C++ files.
/// </summary>
/// <param name="context">Graph context, a list that contains all dependency graph vertices (in other words: all files).</param>
internal class Item : GraphVertex<Item> {

    /// <summary>
    /// Gets a value indicating that the item is a header.
    /// </summary>
    public bool IsHeader { get; }

    /// <summary>
    /// Gets the text content type for Markdown.
    /// </summary>
    public string MarkdownType { get; }

    /// <summary>
    /// Gets the relative path to the header.
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// Gets the implementation item if exists.
    /// </summary>
    public Item? Implementation { get; private set; }

    /// <summary>
    /// Gets the list of all files directly included by this file.
    /// </summary>
    public override IList<Item> Vertices { get; } = [];

    /// <summary>
    /// Gets the direct dependencies of this file.
    /// </summary>
    public IEnumerable<Item> DirectDependencies => Vertices;

    /// <summary>
    /// Gets all dependencies of this file.
    /// </summary>
    public IEnumerable<Item> AllDependencies => this.PostOrder.SkipLast(1);

    /// <summary>
    /// Gets the content of the Code file.
    /// </summary>
    public string Content => File.ReadAllText(Path.Combine(Context.Dir, RelativePath));

    internal bool IsParsed { get; private set; }

    public Item(Codebase context, string relativePath) {
        Context = context;
        RelativePath = relativePath;
        var ext = Path.GetExtension(relativePath);
        IsHeader = ext is ".h" or ".hpp";
        MarkdownType = ext[1..];
        if (IsHeader) {
            var implPath = Context.FindImplementation(relativePath);
            var impl = implPath is not null ? Context.GetByRelativePath(implPath) : null;
            if (impl is null && implPath is not null) impl = new Item(Context, implPath);
            if (impl is not null) { Implementation = impl; }
        }
    }

    /// <summary>
    /// Parses the source file to find include directives.
    /// </summary>
    public void Parse() {
        if (IsParsed) return;
        IsParsed = true;
        var includedFiles = GetIncludedFiles();
        if (includedFiles.Count < 1) return;
        foreach (var includeFile in includedFiles) {
            Item? file = Context.GetByFileName(includeFile);
            if (file is null) { // not already in the project
                var path = Context.FindFile(includeFile);
                if (path is not null && File.Exists(Path.Combine(Context.Dir, path))) {
                    if (Context.Any(i => i.RelativePath == path)) throw new InvalidOperationException("WTF!?");
                    file = new Item(Context, path);
                    Context.Add(file);
                    file.Parse();
                    file.Implementation?.Parse();
                }
                else continue;
            }
            Vertices.Add(file);
        }
    }

    /// <summary>
    /// Uses state machine parser to extract `#include` directives.
    /// </summary>
    /// <returns>A list of included files. Some may contain relative includedFiles.</returns>
    private List<string> GetIncludedFiles() {
        List<string> results = [];
        var state = ParserState.Code;
        char prev = '\0', curr;
        string text = Content;
        bool isInclude = false;
        int s = -1;
        for (int i = 0, n = text.Length; i < n; ++i) {
            curr = text[i];
            if (curr == '\r') continue; // CR characters are irrelevant here and would break line end detection.
            var combo = i > 0 ? text.AsSpan(i - 1, 2) : text.AsSpan(i, 1);
            var codeFromHere = text.AsSpan(i);
            switch (state) {
                case ParserState.Code:
                    if (combo == "//") {
                        state = ParserState.LineComment;
                        continue;
                    }
                    if (combo == "/*") {
                        state = ParserState.BlockComment;
                        continue;
                    }
                    if (curr == '"' && prev != '"' && prev != '\\') {
                        state = ParserState.String;
                        if (isInclude) {
                            if (s < 0) s = ++i;
                        }
                        continue;
                    }
                    if (isInclude && curr == '<') {
                        if (s < 0) s = ++i;
                        continue;
                    }
                    if (isInclude && curr == '>') { // Got angular include includeFile!
                        results.Add(text[s..i]);
                        s = -1;
                        isInclude = false;
                        state = ParserState.Code;
                    }
                    if (codeFromHere.StartsWith("#include")) {
                        isInclude = true;
                        i += 8;
                        continue;
                    }
                    break;
                case ParserState.String:
                    if ((curr == '"' && prev != '\\') || curr == '\n') {
                        state = ParserState.Code;
                        if (isInclude && s >= 0) {
                            results.Add(text[s..i]);
                            s = -1;
                            isInclude = false;
                        }
                    }
                    break;
                case ParserState.LineComment:
                    if (curr == '\n') state = ParserState.Code;
                    break;
                case ParserState.BlockComment:
                    if (combo == "*/") state = ParserState.Code;
                    break;
            }
            prev = curr;
        }
        results.Sort();
        return results;
    }

    public override string ToString() => RelativePath;

    /// <summary>
    /// A list that contains all dependency graph vertices.
    /// </summary>
    private readonly Codebase Context;

    private enum ParserState { Code, String, LineComment, BlockComment }

}
