namespace Woof.TestTerminal;

/// <summary>
/// Configures and runs Windows Terminal to test one or more projects in the solution.
/// </summary>
public class Terminal {

    /// <summary>
    /// Gets or sets a value indicating that Windows Terminal window should be maximized.
    /// </summary>
    public bool IsMaximized { get; set; } = true;

    /// <summary>
    /// Gets or sets a dictionary with keys representing the short names, and values representing the project names.<br/>
    /// Appropriate projects must exist in the solution.
    /// </summary>
    public Dictionary<string, string> Projects { get; set; } = null!;

    /// <summary>
    /// Gets a command starting a configured cmd.exe.
    /// </summary>
    /// <param name="dir">Startup directory.</param>
    /// <param name="title">Pane title.</param>
    /// <param name="prompt">Prompt value.</param>
    /// <param name="run">Additional command to run when the cmd.exe is started.</param>
    /// <returns>A string to be injected into wt.exe arguments.</returns>
    private static string GetCommand(string dir, string? title = null, string? prompt = "$P$G", string? run = null)
        => run is null
            ? $"--title \"{title}\" cmd /K \"{dir[0..2]}&&cd {dir}&&PROMPT={prompt}\""
            : $"--title \"{title}\" cmd /K \"{dir[0..2]}&&cd {dir}&&PROMPT={prompt}&&{run}\"";

    /// <summary>
    /// Gets the project metadata.
    /// </summary>
    /// <param name="name">Project name.</param>
    /// <returns>Project metadata.</returns>
    private static DotNetProject GetProject(string name) {
        var solutionFile = DotNetSolution.CurrentSolutionFile;
        if (solutionFile is null) throw new FileNotFoundException();
        var solution = new DotNetSolution(solutionFile);
        return new(solution.Projects.First(p => p.Name == name).Path);
    }

    /// <summary>
    /// Starts the configured Windows Terminal.
    /// </summary>
    /// <param name="asAdministrator">True to start as the Administartor user.</param>
    /// <param name="run">Additional commands to be run in configured panes.</param>
    public void Start(bool asAdministrator = true, params string[] run) {
        StringBuilder builder = new();
        if (IsMaximized) builder.Append("-M ");
        var e = Projects.GetEnumerator();
        var r = run.AsEnumerable().GetEnumerator();
        var more = e.MoveNext();
        if (!more) throw new InvalidOperationException("No projects added");
        while (more) {
            var name = e.Current.Key;
            var project = GetProject(e.Current.Value);
            var runCommand = r.MoveNext() ? r.Current : null;
            var command = GetCommand(project.OutputDirectory.FullName, name, $"{name} $G ", runCommand);
            builder.Append(command);
            more = e.MoveNext();
            if (more) builder.Append(" ; split-pane ");
        }
        var processStartInfo = new ProcessStartInfo("wt", builder.ToString()) {
            UseShellExecute = true,
            CreateNoWindow = true,
            WorkingDirectory = SolutionDirectory,
            Verb = asAdministrator ? "RunAs" : ""
        };
        Process.Start(processStartInfo);
    }

    private readonly string SolutionDirectory = DotNetSolution.CurrentSolutionFile?.Directory?.FullName
        ?? throw new InvalidOperationException("Can't get the current solution directory");

}
