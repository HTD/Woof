using CppMerge;
using Woof;
using Woof.ConsoleTools;

/// <summary>
/// Contains program options.
/// </summary>
[Usage("{command} [project_path] ([main_file])")]
enum Options {

    [Option("?|h|help", null, "Displays this help message.")]
    Help

}

/// <summary>
/// This program merges C/C++ small codebases into one markdown ready to pass to a LLM for review.
/// It places the file in dependecies-first order.
/// </summary>
internal class Program {

    /// <summary>
    /// Parses command line parameters and displays console output.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    [STAThread]
    private static void Main(string[] args) {
        ConsoleEx.Init();
        ConsoleEx.AssemblyHeader(HeaderItems.Basic | HeaderItems.Description);
        var cmd = Woof.CommandLine.Default;
        cmd.ParametersMin = 1;
        cmd.ParametersMax = 2;
        cmd.Map<Options>();
        cmd.Delegates.Add(Options.Help, Help);
        cmd.Parse(args);
        cmd.RunDelegates();
        if (cmd.HasOption(Options.Help)) return;
        if (cmd.ValidationErrors.Length > 0) {
            Console.WriteLine();
            foreach (var error in cmd.ValidationErrors) Console.Error.WriteLine(error);
            return;
        }
        else {
            try {
                Merge([.. cmd.Parameters]);
            }
            catch (Exception ex) {
                Console.WriteLine($"`f00`ERROR:` {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Merges the code base and places result in the system clipboard.
    /// </summary>
    /// <param name="args">Project directory and optional main file.</param>
    public static void Merge(params string[] args) {
        var cdb = new Codebase();
        cdb.Load(args[0], args.Length > 1 ? args[1] : null);
        ConsoleEx.Header("Dependency chain:");
        foreach (var v in cdb.DependencyChain) ConsoleEx.Item(v);
        cdb.BuildClipboardContent();
    }

    /// <summary>
    /// Displays application help / description message.
    /// </summary>
    public static void Help() {
        Console.WriteLine(
            "Marges target C/C++ file with its dependencies present in the project" + Environment.NewLine +
            "directory and puts it in the system clipboard as a markdown text ready" + Environment.NewLine +
            "to paste to a LLM for review." + Environment.NewLine
        );
        Console.WriteLine(CommandLine.Usage);
    }

}
