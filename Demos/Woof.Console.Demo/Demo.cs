using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Woof.ConsoleFilters;
using Woof.ConsoleTools;

class Demo {

    static async Task Main() {
        ConsoleEx.Init();
        //ConsoleEx.SuccessText = "✓"; // how crazy is that? ;)
        //ConsoleEx.FailText = "✘"; // needs a good Unicode font though.
        ConsoleEx.BulletIndentation = 2;
        ConsoleEx.AlignStart = 35;
        ConsoleEx.AssemblyHeader<Cursor>(HeaderItems.All);
        BulletsTest(3);
        Console.WriteLine();
        await CursorsTestAsync(4);
        Console.WriteLine();
        if (ConsoleEx.IsHexColorEnabled) {
            ColorTest();
            Console.WriteLine();
        }
        LogTest();
        Console.WriteLine();
        await LogTestAsync();
        Console.WriteLine();
        HexStreamTest(48);
        Console.WriteLine();
        DelayFitlerTest();
        Console.WriteLine();
        Console.WriteLine(ConsoleEx.SeparatorLine);
        ConsoleEx.WaitForCtrlC("All test completed successfully, press Ctrl+C to exit...");
    }

    private static void ColorTest() {
        ConsoleEx.Header("Color test:");
        Console.WriteLine("`000`###` `777``000b` # ` [0x000] = Black       `ccc`###` `000``cccb` # ` [0xccc] = Gray    ");
        Console.WriteLine("`007`###` `000``007b` # ` [0x007] = DarkBlue    `00f`###` `000``00fb` # ` [0x00f] = Blue    ");
        Console.WriteLine("`070`###` `000``070b` # ` [0x070] = DarkGreen   `0f0`###` `000``0f0b` # ` [0x0f0] = Green   ");
        Console.WriteLine("`077`###` `000``077b` # ` [0x077] = DarkCyan    `0ff`###` `000``0ffb` # ` [0x0ff] = Cyan    ");
        Console.WriteLine("`700`###` `000``700b` # ` [0x700] = DarkRed     `f00`###` `000``f00b` # ` [0xf00] = Red     ");
        Console.WriteLine("`707`###` `000``707b` # ` [0x707] = DarkMagenta `f0f`###` `000``f0fb` # ` [0xf0f] = Magenta ");
        Console.WriteLine("`770`###` `000``770b` # ` [0x770] = DarkYellow  `ff0`###` `000``ff0b` # ` [0xff0] = Yellow  ");
        Console.WriteLine("`777`###` `000``777b` # ` [0x777] = DarkGray    `fff`###` `000``fffb` # ` [0xfff] = White   ");
    }

    private static void LogTest() {
        ConsoleEx.Header("Synchronous log test:");
        ConsoleEx.Log("This is an example debug message.");
        ConsoleEx.Log('I', "This is an example information text.");
        ConsoleEx.Log('W', "This is an example warning text");
        ConsoleEx.Log('E', "This is an example error text");
    }

    private static async Task LogTestAsync() {
        ConsoleEx.Header("Asynchronous log test:");
        _ = ConsoleEx.LogAsync("This is an example debug message.");
        _ = ConsoleEx.LogAsync('I', "This is an example information text.");
        _ = ConsoleEx.LogAsync('W', "This is an example warning text");
        _ = ConsoleEx.LogAsync('E', "This is an example error text");
        await Task.Delay(10);
    }

    private static void BulletsTest(int n) {
        ConsoleEx.Header("Bullets test:");
        for (var i = 0; i < n; i++) {
            ConsoleEx.Item($"Item {i + 1}");
        }
    }

    private static async Task CursorsTestAsync(int n) {
        ConsoleEx.Header("Cursors test:");
        Console.WriteLine("0         1         2         3         4         5         6         7");
        Console.WriteLine("01234567890123456789012345678901234567890123456789012345678901234567890123456789");
        var tasks = new List<Task>(n);
        for (var i = 0; i < n; i++) {
            var cursor = ConsoleEx.Start($"Starting test task #{PRNG.Next(1, 0x10000)}.");
            tasks.Add(TestTask(cursor, 0.66));
            await Task.Delay(16);
        }
        await Task.WhenAll(tasks);
    }

    private static void HexStreamTest(int n) {
        ConsoleEx.Header("HexStream test:");
        var testData = new byte[n];
        PRNG.NextBytes(testData);
        using var hs = new HexStream();
        hs.Write(testData);
    }

    private static void DelayFitlerTest() {
        ConsoleEx.Header("Delay filter test:");
        const string text = "The quick brown fox jumps over the lazy dog.";
        if (!Console.IsOutputRedirected) {
            var delayFilter = new DelayFilter();
            Console.SetOut(delayFilter);
            Console.WriteLine(text);
            Console.SetOut(delayFilter.Out); // remove the delay filter.
        }
        else Console.WriteLine(text);
    }

    public static async Task TestTask(Cursor cursor, double successRate = 1) {
        var t0 = DateTime.Now;
        var randomDelayValue = PRNG.Next(16, 48);
        for (int i = 0; i < 10; i++) {
            await Task.Delay(randomDelayValue);
            cursor.Dot();
        }
        var time = DateTime.Now - t0;
        ConsoleEx.Complete(cursor, PRNG.NextDouble() < successRate, $"Time: {time:m\\.fff}s");
    }

    private static readonly Random PRNG = new();

}