using CppMerge;

internal class Program {

    [STAThread]
    private static void Main(string[] args) {
        var cdb = new Codebase();
        cdb.Load("D:\\Source\\CodeDog\\Products\\GMD_H757_Riverdi\\Tools", "AppThread.hpp");
        foreach (var v in cdb.DependencyChain) Console.WriteLine(v);
        cdb.BuildClipboardContent();
    }

}