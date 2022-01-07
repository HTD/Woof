using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using Woof.ConsoleTools;

namespace Test.Client;

/// <summary>
/// Defines an asynchronous test method returning a <see cref="ValueTask"/> used for automated testing.<br/>
/// See <see cref="GetTests(object)"/>.
/// </summary>
class TestItem {

    /// <summary>
    /// Gets the test delegate.
    /// </summary>
    public Delegate Test { get; }

    /// <summary>
    /// Gets the test name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the cursor for the test completion description.
    /// </summary>
    public Cursor? Cursor { get; set; }

    /// <summary>
    /// Creates a test item for a method.
    /// </summary>
    /// <param name="target">Test bench instance.</param>
    /// <param name="method">Method info.</param>
    /// <param name="attribute">Test attribute.</param>
    private TestItem(object target, MethodInfo method, TestAttribute attribute) {
        var methodParameters = method.GetParameters()
            .Select(p => p.ParameterType)
            .Concat(new[] { method.ReturnType })
            .ToArray();
        var delegateType = Expression.GetDelegateType(methodParameters);
        Test = method.CreateDelegate(delegateType, target);
        Name = attribute.Description ?? method.Name;
    }

    /// <summary>
    /// Starts the test and displays the result with <see cref="ConsoleEx.Complete(Cursor, bool, string?)"/>.
    /// </summary>
    /// <returns>Task completed when the test is completed or failed with exception.</returns>
    /// <remarks>This method never throws.</remarks>
    public async ValueTask StartAsync() {
        if (Cursor is null) Cursor = ConsoleEx.Start(Name);
        string? message = null;
        try {
            if (Test is Func<ValueTask<string>> getMessageAsync) message = await getMessageAsync();
            else if (Test is Func<ValueTask> testAsync) await testAsync();
            else throw new InvalidOperationException($"Unknown test delegate type. Use Func<ValueTask> and Func<ValueTask<string>> only.");
            ConsoleEx.Complete(Cursor, true, message);
        }
        catch (Exception x) {
            ConsoleEx.Complete(Cursor, false, $"{x.GetType().Name}: {x.Message}");
        }
    }

    /// <summary>
    /// Gets the defined tests from the test bench instance.
    /// </summary>
    /// <param name="target">Test bench instance.</param>
    /// <returns>All available tests.</returns>
    public static TestItem[] GetTests(object target) =>
        target.GetType().GetMethods()
        .Select(i => (method: i, attribute: i.GetCustomAttribute<TestAttribute>()))
        .Where(i => i.attribute != null)
        .Select(i => new TestItem(target, i.method, i.attribute!))
        .ToArray();

}