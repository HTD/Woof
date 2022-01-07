using System;
using System.Threading.Tasks;
using Woof.ConsoleTools;
using Woof.Net;
using Woof.Net.WebSockets;

namespace Test.Server;

class Program {

    static async Task Main() {
        await Settings.Default.LoadAsync();
        ConsoleEx.Init();
        ConsoleEx.AssemblyHeader<WoofCodec>(HeaderItems.All);
        var startCursor = ConsoleEx.Start("Starting server");
        await using var server = new TestServer();
        server.StateChanged += Server_StateChanged;
        server.ClientConnected += Server_ClientConnected;
        server.ClientDisconnecting += Server_ClientDisconnecting;
        server.ClientDisconnected += Server_ClientDisconnected;
        server.UserSignedIn += Server_UserSignedIn;
        server.UserSigningOut += Server_UserSigningOut;
        server.UserSignedOut += Server_UserSignedOut;
        server.UserSignInFail += Server_UserSignInFail;
        server.ConnectException += Server_ConnectException;
        server.ReceiveException += Server_ReceiveException;
        try {
            await server.StartAsync();
            ConsoleEx.Complete(startCursor);
        }
        catch (Exception x) {
            ConsoleEx.Complete(startCursor, false, $"{x.GetType().Name}: {x.Message}");
        }
        await ConsoleEx.WaitForCtrlCAsync("Press Ctrl+C to shut down.");
        await server.StopAsync();
    }

    private static async ValueTask Server_ClientConnected(object? sender, WebSocketEventArgs e)
        => await Task.Run(() => ConsoleEx.Log('i', $"Client connected from `707`{e.Context.LocalEndPoint}`."));

    private static async ValueTask Server_UserSignedIn(object? sender, WebSocketSessionEventArgs e)
        => await Task.Run(() => ConsoleEx.Log('i', $"Client signed in, id: `707`{e.Session?.Client?.Id}`."));

    private static async ValueTask Server_UserSigningOut(object? sender, WebSocketSessionEventArgs e)
        => await Task.Run(() => ConsoleEx.Log('i', $"Client signing out, id: `707`{e.Session?.Client?.Id}`."));

    private static async ValueTask Server_UserSignInFail(object? sender, WebSocketSessionEventArgs e)
        => await Task.Run(() => ConsoleEx.Log('w', $"Invalid sign in attempt."));

    private static void Server_UserSignedOut(Guid? id)
        => ConsoleEx.Log('i', id is null ? "User signed out." : $"User `707`{id}` signed out.");

    private static async ValueTask Server_ClientDisconnecting(object? sender, WebSocketEventArgs e)
        => await Task.Run(() => ConsoleEx.Log('i', $"Client (`707`{e.Context.LocalEndPoint}`) disconnecting..."));

    private static void Server_ClientDisconnected(Guid? id)
        => ConsoleEx.Log('i', id is null ? "Client disconnected." : $"Client `707`{id}` disconnected.");

    static void Server_StateChanged(object? sender, StateChangedEventArgs e)
        => ConsoleEx.Log('i', $"Server state: `ff0`{e.State}`.");

    static async ValueTask Server_ConnectException(object? sender, ExceptionEventArgs e)
        => await Task.Run(() => ConsoleEx.Log('e',
            $"Connect: {e.GetType().Name}: {e.Exception.Message}",
            $"`077`STACKTRACE:`",
            $"`777`{e.Exception.StackTrace}`"));

    static async ValueTask Server_ReceiveException(object? sender, ExceptionEventArgs e)
        => await Task.Run(() => ConsoleEx.Log('e', $"Receive: {e.GetType().Name}: {e.Exception.Message}{Environment.NewLine}STACKTRACE:{Environment.NewLine}{e.Exception.StackTrace}"));

}