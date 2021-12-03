using System.Reflection;

using Woof.ConsoleFilters;

namespace Woof.ConsoleTools;

/// <summary>
/// Window console extensions.
/// </summary>
public static class ConsoleEx {

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating that HexColor filter is enabled.
    /// </summary>
    public static bool IsHexColorEnabled {
        get => _IsHexColorEnabled;
        set {
            if (_IsHexColorEnabled = value) {
                if (OperatingSystem.IsLinux()) Environment.SetEnvironmentVariable("TERM", "xterm-256color");
                Console.SetOut(new HexColorFilter());
            }
            else Console.SetOut(Console.Out);
        }
    }

    /// <summary>
    /// Gets or sets the console color theme.
    /// </summary>
    public static ConsoleTheme Theme { get; set; } = new();

    /// <summary>
    /// Gets or sets the bullet symbol used for bullet points.
    /// </summary>
    public static string BulletSymbol { get; set; } = "■";

    /// <summary>
    /// Gets or sets the number of spaces inserted before bullet points.
    /// </summary>
    public static int BulletIndentation { get; set; }

    /// <summary>
    /// Gets or sets a number of column used to align start completions with dots. Default 0 - no alignment, 3 dots inserted before completion.
    /// </summary>
    public static int AlignStart { get; set; }

    /// <summary>
    /// Gets or sets the text displayed on successfull <see cref="Complete(Cursor, bool, string?)"/>.
    /// </summary>
    public static string SuccessText { get; set; } = "OK";

    /// <summary>
    /// Gets or sets the text displayed on failed <see cref="Complete(Cursor, bool, string?)"/>.
    /// </summary>
    public static string FailText { get; set; } = "FAIL";

    /// <summary>
    /// Sets console window size to a preset dimensions.
    /// </summary>
    public static ConsoleSize Size {
        set {
            int w, h, wl = Console.LargestWindowWidth, hl = Console.LargestWindowHeight;
            switch (value) {
                case ConsoleSize.Normal:
                    if (OperatingSystem.IsWindows()) Console.SetWindowSize(80, 25);
                    return;
                case ConsoleSize.Double:
                    w = 160; h = 50;
                    if (w > wl) w = wl;
                    if (h > hl) h = hl;
                    if (OperatingSystem.IsWindows()) Console.SetWindowSize(w, h);
                    break;
                case ConsoleSize.Max:
                    if (OperatingSystem.IsWindows()) Console.SetWindowSize(wl, hl);
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the maximal full line based on <see cref="Console.WindowWidth"/> if available, 80 if this fails.
    /// </summary>
    public static int MaxLineLength {
        get {
            if (Console.IsOutputRedirected) return 80;
            try {
                return Console.WindowWidth;
            }
            catch {
                return 80;
            }
        }
    }

    /// <summary>
    /// Displays horizontal line separator across the console window.
    /// </summary>
    public static string SeparatorLine =>
        (IsHexColorEnabled ? $"`{Theme.SeparatorLine}`" : "") +
        String.Empty.PadRight(MaxLineLength - 1, '-') +
        (IsHexColorEnabled ? "`" : "");

    /// <summary>
    /// Gets or sets current console state.
    /// </summary>
    public static ConsoleState State {
        get => new() {
            Background = Console.BackgroundColor,
            Foreground = Console.ForegroundColor,
            X = Console.CursorLeft,
            Y = Console.CursorTop,
            WinX = Console.WindowLeft,
            WinY = Console.WindowTop
        };
        set {
            Console.BackgroundColor = value.Background;
            Console.ForegroundColor = value.Foreground;
            Console.SetCursorPosition(value.X, value.Y);
            if (OperatingSystem.IsWindows()) Console.SetWindowPosition(value.WinX, value.WinY);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initializes the console: sets output encoding to UTF8 and enables the color output if the standard output is not redirected.
    /// </summary>
    public static void Init() {
        Console.OutputEncoding = Encoding.UTF8;
        IsHexColorEnabled = !Console.IsOutputRedirected;
    }

    /// <summary>
    /// Displays a formatted header from entry assembly info.
    /// </summary>
    /// <param name="items">Items to display.</param>
    /// <param name="assembly">Assembly to use, if none specified - entry assembly is used.</param>
    public static void AssemblyHeader(HeaderItems items = HeaderItems.Basic, Assembly? assembly = default) {
        if (assembly is null) assembly = Assembly.GetEntryAssembly();
        if (assembly is null) throw new NullReferenceException();
        var product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        var version = assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        var versionShort = new Version(version ?? "0.0.0").ToString(3);
        var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        Console.Clear();
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = title = product ?? title ?? assembly.FullName ?? assembly.Location;
        var builder = new StringBuilder();
        builder.AppendLine(SeparatorLine);
        var resetColor = false;
        if (items.HasFlag(HeaderItems.Title)) {
            resetColor = IsHexColorEnabled;
            builder.Append(IsHexColorEnabled ? $"`{Theme.Title}`{title}" : $"{title}");
        }
        if (items.HasFlag(HeaderItems.Version)) {
            resetColor = IsHexColorEnabled;
            builder.Append($" version {versionShort}");
        }
        if (items.HasFlag(HeaderItems.Copyright) && !String.IsNullOrEmpty(copyright)) {
            builder.Append(IsHexColorEnabled ? $" `{Theme.Copyright}`{copyright}" : copyright);
        }
        if (resetColor) builder.AppendLine("`"); else builder.AppendLine();
        if (items.HasFlag(HeaderItems.Description) && !String.IsNullOrEmpty(description))
            builder.AppendLine(IsHexColorEnabled ? $"`{Theme.Description}`{description}`" : description);
        builder.AppendLine(SeparatorLine);
        Console.Write(builder.ToString());
    }

    /// <summary>
    /// Displays a formatted header from entry assembly info.
    /// </summary>
    /// <typeparam name="T">A type from the target assembly.</typeparam>
    /// <param name="items">Items to display.</param>
    public static void AssemblyHeader<T>(HeaderItems items = HeaderItems.Basic) =>
        AssemblyHeader(items, Assembly.GetAssembly(typeof(T)));

    /// <summary>
    /// Displays one or more messages with timestamps.
    /// </summary>
    /// <param name="messages">One or more messages to display.</param>
    public static void Log(params string[] messages) => Log(DateTime.Now, messages);

    /// <summary>
    /// Displays one or more debug messages.
    /// </summary>
    /// <param name="severity">
    /// INFO:    0, 'I' or 'i',<br/>
    /// WARNING: 1, 'W' or 'w',<br/>
    /// ERROR:   2, 'E' or 'e'.
    /// </param>
    /// <param name="messages">Messages to log.</param>
    public static void Log(int severity, params string[] messages) => Log(DateTime.Now, severity, messages);

    /// <summary>
    /// Displays one or more messages with accurate timestamps asynchronously.
    /// </summary>
    /// <param name="messages">Messages to log.</param>
    /// <returns>Task completed when the messages are displayed.</returns>
    public static async Task LogAsync(params string[] messages) {
        var time = DateTime.Now;
        await Task.Run(() => Log(time, messages));
    }

    /// <summary>
    /// Displays one or more debug messages with accurate timestamps asynchronously.
    /// </summary>
    /// <param name="severity">
    /// INFO:    0, 'I' or 'i',<br/>
    /// WARNING: 1, 'W' or 'w',<br/>
    /// ERROR:   2, 'E' or 'e'.
    /// </param>
    /// <param name="messages">Messages to log.</param>
    /// <returns>Task completed when messages are displayed.</returns>
    public static async Task LogAsync(int severity, params string[] messages) {
        var time = DateTime.Now;
        await Task.Run(() => Log(time, severity, messages));
    }

    /// <summary>
    /// Displays header message.
    /// </summary>
    /// <param name="message">Header message.</param>
    public static void Header(string message) => Console.WriteLine(IsHexColorEnabled ? $"`{Theme.HeaderText}`{message}`" : message);

    /// <summary>
    /// Displays error message.
    /// </summary>
    /// <param name="message">Error message.</param>
    public static void Error(string message) => Console.WriteLine(IsHexColorEnabled ? $"`{Theme.ErrorLabel}`!!!` {message}" : $"!!! {message}");

    /// <summary>
    /// Returns value as string with optional error color format.
    /// </summary>
    /// <param name="value">A value convertible to string.</param>
    /// <returns>Formatted value.</returns>
    public static string ErrorValue(object value) => IsHexColorEnabled ? $"`[`{Theme.ErrorValue}`{value}`]" : $"[{value}]";

    /// <summary>
    /// Returns value as string with optional correct color format.
    /// </summary>
    /// <param name="value">A value convertible to string.</param>
    /// <returns>Formatted value.</returns>
    public static string CorrectValue(object value) => IsHexColorEnabled ? $"`[`{Theme.CorrectValue}`{value}`]" : $"[{value}]";

    /// <summary>
    /// Displays start message and returns a <see cref="Cursor"/> instance used to show completion status.
    /// </summary>
    /// <param name="message">Message to display.</param>
    /// <returns>A <see cref="Cursor"/> instance used to show completion status.</returns>
    public static Cursor Start(string message) {
        var builder = new StringBuilder();
        var bulletSpace = BulletIndentation;
        if (bulletSpace > 0) builder.Append(string.Empty.PadRight(bulletSpace));
        if (!String.IsNullOrEmpty(BulletSymbol)) {
            builder.Append(IsHexColorEnabled ? $"`{Theme.StartSymbol}`{BulletSymbol}`" : BulletSymbol);
            builder.Append(' ');
            bulletSpace += BulletSymbol.Length + 1;
        }
        builder.Append(IsHexColorEnabled ? $"`{Theme.StartText}`{message}" : message);
        if (AlignStart < 1) builder.Append("...");
        else {
            var dotCount = AlignStart - bulletSpace - message.Length;
            if (dotCount < 3) dotCount = 3;
            builder.Append(string.Empty.PadRight(dotCount, '.'));
        }
        if (IsHexColorEnabled) builder.Append('`');
        lock (Lock) Console.Write(builder.ToString());
        return new Cursor();
    }

    /// <summary>
    /// Display item as bullet point.
    /// </summary>
    /// <param name="item">Item.</param>
    public static void Item(string item) {
        var builder = new StringBuilder();
        var bulletSpace = BulletIndentation;
        if (bulletSpace > 0) builder.Append(string.Empty.PadRight(bulletSpace));
        if (!String.IsNullOrEmpty(BulletSymbol)) {
            builder.Append(IsHexColorEnabled ? $"`{Theme.BulletSymbol}`{BulletSymbol}`" : BulletSymbol);
            builder.Append(' ');
        }
        builder.Append(IsHexColorEnabled ? $"`{Theme.BulletText}`{item}`" : item);
        lock (Lock) Console.WriteLine(builder.ToString());
    }

    /// <summary>
    /// Completes the action started with the <see cref="Start(string)"/>.
    /// </summary>
    /// <param name="cursor">Cursor.</param>
    /// <param name="success">Status of the completed operation.</param>
    /// <param name="message">Optional message to display after the status label.</param>
    public static void Complete(Cursor cursor, bool success = true, string? message = null) {
        var builder = new StringBuilder();
        var status = success ? SuccessText : FailText;
        var padding = Math.Max(SuccessText.Length, FailText.Length) - status.Length;
        var color = success ? Theme.CompleteLabelSuccess : Theme.CompleteLabelFail;
        builder.Append(
            IsHexColorEnabled
                ? $"`{Theme.CompleteBrackets}`[`{color}`{status}`{Theme.CompleteBrackets}`]`"
                : $"[{status}]"
        );
        if (padding > 0) builder.Append(string.Empty.PadRight(padding));
        if (message is not null) {
            builder.Append(' ');
            builder.Append(IsHexColorEnabled ? $"`{Theme.CompleteMessage}`{message}`" : message);
        }
        if (IsHexColorEnabled && !string.IsNullOrEmpty(BulletSymbol))
            cursor.Write($"`{color}`{BulletSymbol}`", BulletIndentation);
        cursor.WriteLine(builder.ToString());
    }

    /// <summary>
    /// Displays a message and waits until Ctrl+C is pressed.
    /// </summary>
    /// <param name="message">Optional alternative message to display.</param>
    public static void WaitForCtrlC(string? message = default) {
        using var semaphore = new SemaphoreSlim(0, 1);
        void handler(object? s, ConsoleCancelEventArgs e) { Console.CancelKeyPress -= handler; e.Cancel = true; semaphore.Release(); }
        Console.CancelKeyPress += handler;
        Console.WriteLine(message);
        semaphore.Wait();
    }

    /// <summary>
    /// Waits for Ctrl+C.
    /// </summary>
    /// <param name="message">Message to display.</param>
    /// <returns>Task completed when the Ctrl+C is pressed.</returns>
    public static async Task WaitForCtrlCAsync(string? message = default) {
        using var semaphore = new SemaphoreSlim(0, 1);
        void handler(object? s, ConsoleCancelEventArgs e) { Console.CancelKeyPress -= handler; e.Cancel = true; semaphore.Release(); }
        Console.CancelKeyPress += handler;
        Console.WriteLine(message);
        await semaphore.WaitAsync();
    }

    #endregion

    #region Everything else

    private static void Log(DateTime timestamp, params string[] messages) {
        lock (Lock)
            foreach (var message in messages)
                Console.WriteLine(
                    IsHexColorEnabled
                        ? $"`{Theme.TimeStampText}`{timestamp:HH:mm:ss.fff}` {message}"
                        : $"{timestamp:HH:mm:ss.fff} {message}");
    }

    private static void Log(DateTime timestamp, int severity, params string[] messages) {
        if (severity > 2) severity = SeverityMap[(char)severity];
        (string headerText, string headerColor) =
            severity switch {
                0 => ("INFO", $"`{Theme.InfoLabel}`"),
                1 => ("WARNING", $"`{Theme.WarningLabel}`"),
                2 => ("ERROR", $"`{Theme.ErrorLabel}`"),
                _ => throw new ArgumentException("Invalid value", nameof(severity))
            };
        var header = IsHexColorEnabled
            ? $"`{Theme.TimeStampText}`{timestamp:HH:mm:ss.fff}` {headerColor}{headerText}:` "
            : $"{timestamp:HH:mm:ss.fff} {headerText}: ";
        lock (Lock)
            for (int i = 0, n = messages.Length; i < n; i++)
                Console.WriteLine(i < 1 ? header + messages[i] : messages[i]);
    }

    /// <summary>
    /// Gets the console lock object for synchronous console accesss.
    /// </summary>
    public static readonly object Lock = new();

    private static bool _IsHexColorEnabled;
    private static readonly Dictionary<char, int> SeverityMap = new() {
        ['I'] = 0,
        ['W'] = 1,
        ['E'] = 2,
        ['i'] = 0,
        ['w'] = 1,
        ['e'] = 2
    };

    #endregion

}