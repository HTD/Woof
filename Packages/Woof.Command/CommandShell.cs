
using Woof.Command.Internals;

namespace Woof.Command;

/// <summary>
/// Commands Hell v0.666 ;)
/// </summary>
public class CommandShell {

    #region Events

    /// <summary>
    /// Occurs whenever the Enter key is pressed - current line is passed as event argument.
    /// The event handler can optionally end the shell session.
    /// </summary>
    public event EventHandler<CommandEventArgs>? Command;

    /// <summary>
    /// Occurs when the command history is changed.
    /// Provide a handler to save history.
    /// </summary>
    public event EventHandler? HistoryChanged;

    /// <summary>
    /// Occurs when the command history is requested.
    /// Provide a handler to load history.
    /// </summary>
    public event EventHandler? HistoryRequested;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the shell header message.
    /// </summary>
    public string Header { get; set; }

    /// <summary>
    /// Gets the command history.
    /// </summary>
    public CommandHistory History { get; }

    /// <summary>
    /// Gets the auto-complete list.
    /// </summary>
    public CommandAutoCompleteList AutoComplete { get; }

    /// <summary>
    /// Gets the manual pages for the built-in and custom commands.
    /// </summary>
    public ManPagesDictionary ManPages { get; } = new ManPagesDictionary(new BuiltInManual());

    /// <summary>
    /// Gets or sets the shell command prompt format string.
    /// </summary>
    public string PromptFormat { get; set; }

    #endregion

    #region Private data

    #region Keyboard shortcuts

    private static readonly ConsoleKeyShortcut KeyExec = new(0, ConsoleKey.Enter);
    private static readonly ConsoleKeyShortcut KeyExit = new(ConsoleModifiers.Control, ConsoleKey.D);
    private static readonly ConsoleKeyShortcut KeyClear = new(ConsoleModifiers.Control, ConsoleKey.L);
    private static readonly ConsoleKeyShortcut KeyHistoryPrev = new(0, ConsoleKey.UpArrow);
    private static readonly ConsoleKeyShortcut KeyHistoryNext = new(0, ConsoleKey.DownArrow);
    private static readonly ConsoleKeyShortcut KeyAutoComplete = new(0, ConsoleKey.Tab);

    #endregion

    #region Console colors

    private readonly ConsoleColor ColorDefault = Console.ForegroundColor;
    private const ConsoleColor ColorHeader = ConsoleColor.DarkCyan;
    private const ConsoleColor ColorPrompt = ConsoleColor.Gray;
    private const ConsoleColor ColorSpecial = ConsoleColor.Green;
    private const ConsoleColor ColorNotice = ConsoleColor.Cyan;
    private const ConsoleColor ColorWarning = ConsoleColor.Yellow;
    private const ConsoleColor ColorError = ConsoleColor.Red;
    private const ConsoleColor ColorContent = ConsoleColor.DarkGray;
    private const ConsoleColor ColorHistory = ConsoleColor.DarkGray;
    private const ConsoleColor ColorPeek = ConsoleColor.DarkGreen;

    #endregion

    #region State

    /// <summary>
    /// Current line interactive object.
    /// </summary>
    private readonly CommandLineRenderer CurrentLine;

    #endregion

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the instance with optional default header and prompt format.
    /// </summary>
    /// <param name="header">Header to display when the shell starts.</param>
    /// <param name="prompt">Command prompt format string, use {0} for current directory.</param>
    public CommandShell(string header = "Commands Hell v0.666", string prompt = "CS {0}> ") {
        Header = header;
        PromptFormat = prompt;
        AutoComplete = new CommandAutoCompleteList(ManPages.Keys) { PeekColor = ColorPeek };
        CurrentLine = new CommandLineRenderer();
        History = new CommandHistory();
    }

    /// <summary>
    /// Starts the shell session. Blocks the current thread until the shell is exited.
    /// </summary>
    public void Start() {
        OnHistoryRequested();
        Console.ForegroundColor = ColorHeader;
        Console.WriteLine(Header);
        while (true) {
            Prompt();
            CurrentLine.Render();
            while (true) {
                var k = Console.ReadKey(true);
                HandleResets(k);
                HandleAutoComplete(k);
                HandleHistory(k);
                HandleClear(k);
                if (HandleExec(k, out var shouldExit)) if (shouldExit) return; else break;
                if (HandleExit(k)) return;
                CurrentLine.AcceptKey(k);
            }
        }
    }

    /// <summary>
    /// Starts the shell session task.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task completed when the shell is exited or the token is cancelled.</returns>
    public Task StartAsync(CancellationToken cancellationToken = default)
        => Task.Factory.StartNew(Start, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    #endregion

    #region Protected

    /// <summary>
    /// Internal command handler.
    /// If the command is not handled in <see cref="Command"/> event, it's handled here.
    /// </summary>
    /// <param name="e">Command arguments.</param>
    protected virtual void OnCommand(CommandEventArgs e) {
        Command?.Invoke(this, e);
        if (!e.IsHandled) {
            var commandPath = PathTools.IsFileAcessibleInPath(e.Command)
                ? PathTools.GetFullPath(e.Command) : null;
            if (commandPath is not null) {
                Execute(commandPath, false, e.Arguments?.Raw);
            }
            else if (e.Arguments.Switches["?|help"] && ManPages.ContainsKey(e.Command)) {
                Man(e.Command);
                e.IsHandled = true;
            }
            else switch (e.Command) {
                    case "cat": GetContent(e.Arguments.Positional[0]); break;
                    case "cd": SetLocation(e.Arguments.Positional[0]); break;
                    case "cls": Console.Clear(); break;
                    case "exit": e.ShouldExit = true; break;
                    case "history": if (e.Arguments.Switches["c|clear"]) HistoryClear(); else HistoryShow(); break;
                    case "ls": GetLocation(e.Arguments.Positional[0] ?? "."); break;
                    case "man": Man(e.Arguments.Positional[0]); break;
                    case "pwd": ShowMsg(Directory.GetCurrentDirectory()); break;
                    case "touch": Touch(e.Arguments.Positional[0]); break;
                    default:
                        Execute(e);
                        break;
                }
        }
        if (e.Output is not null) {
            if (e.OutputColor.HasValue) ShowTxt(e.Output, e.OutputColor.Value);
            else ShowMsg(e.Output, e.OutputType);
        }
    }

    /// <summary>
    /// Triggers <see cref="HistoryChanged"/> event.
    /// The event handler can get the current command history and save it somewhere.
    /// </summary>
    protected void OnHistoryChanged() => HistoryChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Triggers <see cref="HistoryRequested"/> event.
    /// The event handler can load the previous command history.
    /// </summary>
    protected void OnHistoryRequested() => HistoryRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Executes external command with STDOUT and STDERR redirected to this shell.
    /// Blocks current thread until external process exits.
    /// </summary>
    /// <param name="command">Command line.</param>
    protected static void Execute(ICommand command)
        => Execute($"cmd.exe", false, "/c", command.Arguments.Any ? $"{command.Command} {String.Join(" ", command.Arguments.Raw)}" : command.Command);

    /// <summary>
    /// Executes external command with STDOUT and STDERR redirected to this shell.
    /// Blocks current thread until external process exits.
    /// </summary>
    /// <param name="command">Command name or file name.</param>
    /// <param name="redirection">True to redirect standard output and standard error, if set true, the output from the process will be intercepted and own, colored output will occur.</param>
    /// <param name="arguments">Arguments to pass.</param>
    protected static void Execute(string command, bool redirection = true, params string[]? arguments) {
        var currentForeground = Console.ForegroundColor;
        Console.ForegroundColor = ColorContent;
        try {
            var startInfo = new ProcessStartInfo(command) { UseShellExecute = false };
            if (arguments is not null) foreach (var arg in arguments) startInfo.ArgumentList.Add(arg);
            using var process = new Process() {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            if (redirection) {
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += (s, e) => {
                    Console.ForegroundColor = ColorContent;
                    Console.WriteLine(e.Data);
                };
                process.ErrorDataReceived += (s, e) => {
                    Console.ForegroundColor = ColorError;
                    Console.WriteLine(e.Data);
                };
            }
            if (process.Start()) {
                if (redirection) {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                process.WaitForExit();
            }
        }
        catch (Exception x) {
            Console.ForegroundColor = ColorError;
            Console.WriteLine($"EXTERNAL COMMAND CAUSED EXCEPTION: {x.Message}");
        }
        finally {
            Console.ForegroundColor = currentForeground;
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Shows some text in a specified color.
    /// </summary>
    /// <param name="text">Text to display.</param>
    /// <param name="color">Color to use.</param>
    public static void ShowTxt(string text, ConsoleColor color) {
        if (text == null) return;
        var foregroundCurrent = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = foregroundCurrent;
    }

    /// <summary>
    /// Shows a text message.
    /// </summary>
    /// <param name="message">Text message.</param>
    /// <param name="type">Message type from <see cref="CommandMessageType"/> enumeration.</param>
    public static void ShowMsg(string message, CommandMessageType type = CommandMessageType.Content) {
        if (message == null) return;
        var foregroundCurrent = Console.ForegroundColor;
        switch (type) {
            case CommandMessageType.Content: Console.ForegroundColor = ColorContent; break;
            case CommandMessageType.Info: Console.ForegroundColor = ColorHeader; break;
            case CommandMessageType.Notice: Console.ForegroundColor = ColorNotice; break;
            case CommandMessageType.Special: Console.ForegroundColor = ColorSpecial; break;
            case CommandMessageType.Warning: Console.ForegroundColor = ColorWarning; break;
            case CommandMessageType.Error: Console.ForegroundColor = ColorError; break;
        }
        Console.WriteLine(message);
        Console.ForegroundColor = foregroundCurrent;
    }

    /// <summary>
    /// Shows a line collection.
    /// </summary>
    /// <param name="lines">Any text lines collection.</param>
    /// <param name="color">Optional text color.</param>
    public static void ShowLines(IEnumerable<string> lines, ConsoleColor color = ColorContent) {
        if (lines == null || !lines.Any()) return;
        var foregroundCurrent = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(String.Join(Environment.NewLine, lines));
        Console.ForegroundColor = foregroundCurrent;
    }

    #endregion

    #region Keyboard event handlers

    /// <summary>
    /// Handles resetting special shell modes like history and auto-complete.
    /// </summary>
    /// <param name="k">Keyboard chord.</param>
    void HandleResets(ConsoleKeyInfo k) {
        if (History.CurrentIndex >= 0 && !KeyHistoryPrev.IsMatch(k) && !KeyHistoryNext.IsMatch(k)) History.Reset();
        if (AutoComplete.CurrentIndex >= 0 && !KeyAutoComplete.IsMatch(k)) AutoComplete.Reset();
    }

    /// <summary>
    /// Handles clear shortcut.
    /// </summary>
    /// <param name="k">Keyboard chord.</param>
    void HandleClear(ConsoleKeyInfo k) {
        if (!KeyClear.IsMatch(k)) return;
        Console.Clear();
        Prompt();
        CurrentLine.UpdateText();
    }

    /// <summary>
    /// Handles auto-complete shortcut.
    /// </summary>
    /// <param name="k">Keyboard chord.</param>
    void HandleAutoComplete(ConsoleKeyInfo k) {
        if (!KeyAutoComplete.IsMatch(k)) return;
        if (AutoComplete.CurrentIndex < 0) {
            AutoComplete.Match(CurrentLine.CurrentPart, CurrentLine.CurrentPartIndex == 0 || CurrentLine.Command == "man");
        }
        var replacement = AutoComplete.Next();
        if (replacement != null) {
            CurrentLine.CurrentPart = replacement;
            CurrentLine.UpdateText();
        }
        if (CurrentLine.CurrentPart is not null && CurrentLine.CurrentPart.EndsWith(' ')) AutoComplete.Match();
        if (AutoComplete.Count > 1) AutoComplete.Peek();
    }

    /// <summary>
    /// Handles history shortcuts.
    /// </summary>
    /// <param name="k">Keyboard chord.</param>
    void HandleHistory(ConsoleKeyInfo k) {
        string? line = null;
        if (KeyHistoryPrev.IsMatch(k)) line = History.Prev(CurrentLine);
        else if (KeyHistoryNext.IsMatch(k)) line = History.Next();
        if (line != null) {
            CurrentLine.Text = line;
            CurrentLine.UpdateText();
            CurrentLine.End();
        }
    }

    /// <summary>
    /// Handles the enter key.
    /// </summary>
    /// <param name="k">Keyboard chord.</param>
    /// <param name="shouldExit">True when exit is requested either by internal or external command.</param>
    /// <returns>True if shortcut matched.</returns>
    bool HandleExec(ConsoleKeyInfo k, out bool shouldExit) {
        if (KeyExec.IsMatch(k)) {
            History.Add(CurrentLine);
            OnHistoryChanged();
            var e = new CommandEventArgs(CurrentLine);
            Console.WriteLine();
            CurrentLine.Text = String.Empty;
            CurrentLine.Cursor = 0;
            OnCommand(e);
            shouldExit = e.ShouldExit;
            return true;
        }
        shouldExit = false;
        return false;
    }

    /// <summary>
    /// Returns true if shell exit shortcut is detected.
    /// </summary>
    /// <param name="k">Keyboard chord.</param>
    /// <returns>True if exit shortcut is detected.</returns>
    static bool HandleExit(ConsoleKeyInfo k) => KeyExit.IsMatch(k);

    #endregion

    #region Internal commands

    /// <summary>
    /// Shows command prompt as defined.
    /// </summary>
    private void Prompt() {
        var cd = Directory.GetCurrentDirectory();
        Console.ForegroundColor = ColorPrompt;
        Console.Write(PromptFormat, cd);
        Console.ForegroundColor = ColorDefault;
        CurrentLine.SetCursorHome();
    }

    /// <summary>
    /// Displays the content of the text file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    static void GetContent(string? fileName) {
        if (fileName == null) ShowMsg(String.Join(Environment.NewLine, new string[] {
                    @"      |\__/,|   (`\  ",
                    @"    _.|o o  |_   ) ) ",
                    @"---(((---(((---------"
                }), CommandMessageType.Special);
        else if (!File.Exists(fileName)) ShowMsg("No such file.", CommandMessageType.Warning);
        else
            try {
                ShowMsg(File.ReadAllText(fileName));
            }
            catch (Exception x) {
                ShowMsg($"Could not read: {x.Message}", CommandMessageType.Error);
            }
    }

    /// <summary>
    /// Displays a list of directories and files in the specified directory.
    /// </summary>
    /// <param name="dir">Existing directory. A warning is displayed when the directory doesn't exist.</param>
    static void GetLocation(string? dir) {
        if (dir is not null && Directory.Exists(dir))
            ShowMsg(String.Join(Environment.NewLine,
                Directory.EnumerateDirectories(dir).Select(i => new DirectoryInfo(i)).OrderBy(i => i.Name).Select(i => $"{i.LastWriteTime}\t<DIR>\t{i.Name}").Concat(
                Directory.EnumerateFiles(dir).Select(i => new FileInfo(i)).OrderBy(i => i.Name).Select(i => $"{i.LastWriteTime}\t{i.Length}\t{i.Name}")))
            );
        else ShowMsg($"No such directory: {dir}.", CommandMessageType.Warning);
    }

    /// <summary>
    /// Clears the current command history and triggers <see cref="HistoryChanged"/> event.
    /// </summary>
    void HistoryClear() {
        History.Clear();
        OnHistoryChanged();
    }

    /// <summary>
    /// Shows the history lines (if any available).
    /// </summary>
    void HistoryShow() => ShowTxt(History.ToString(1) ?? "The list is empty.", ColorHistory);

    /// <summary>
    /// Shows the micro-manual for the internal shell command.
    /// </summary>
    /// <param name="page"></param>
    private void Man(string? page) {
        if (page == null) {
            ShowMsg(
                "Please specify micro-manual page from the following:" + Environment.NewLine +
                "[ " + String.Join(", ", ManPages.Keys) + " ]"
            );
        }
        else if (ManPages.TryGetValue(page, out var pageContent)) {
            ShowMsg(pageContent);
        }
        else ShowMsg($"There's no manual page on \"{page}\".", CommandMessageType.Warning);
    }

    /// <summary>
    /// Sets the current location to the directory specified, or displays the current directory on null parameter.
    /// </summary>
    /// <param name="dir">Existing directory. A warning is displayed when the directory doesn't exist.</param>
    static void SetLocation(string? dir) {
        if (dir == null) ShowMsg(Directory.GetCurrentDirectory());
        else if (Directory.Exists(dir)) Directory.SetCurrentDirectory(dir);
        else ShowMsg($"No such directory: {dir}.", CommandMessageType.Warning);
    }

    /// <summary>
    /// Either creates an empty file or modifies last write time of an existing file.
    /// </summary>
    /// <param name="fileName">A path to the file.</param>
    private static void Touch(string? fileName) {
        if (fileName is null) {
            ShowMsg("Can't touch null.", CommandMessageType.Error);
            return;
        }
        try {
            if (File.Exists(fileName)) File.SetLastWriteTime(fileName, DateTime.Now);
            else File.Create(fileName).Dispose();
        }
        catch {
            ShowMsg("Can't touch this.", CommandMessageType.Error);
        }
    }

    #endregion

}