using System;
using System.Threading.Tasks;

using Test.Api;

using Woof.ConsoleTools;
using Woof.Net;
using Woof.Net.WebSockets;

namespace Test.Client;

/// <summary>
/// Console test for the <see cref="TestClient"/>.
/// </summary>
class Program {

    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <returns>Task completed when the program is completed.</returns>
    static async Task Main() {
        await Settings.Default.LoadAsync();
        await using var client = new TestClient();
        client.StateChanged += Client_StateChanged;
        client.ReceiveException += Client_OnReceiveException;
        client.MessageReceived += Client_MessageReceived;
        await new TestBench(client).StartAsync();
        await ConsoleEx.WaitForCtrlCAsync("Listening to further messages, press Ctrl+C to exit.");
        await client.StopAsync();
    }

    #region Event handlers

    /// <summary>
    /// Handles MessageReceived event.
    /// </summary>
    /// <param name="sender">Client.</param>
    /// <param name="e">Message event data.</param>
    static ValueTask Client_MessageReceived(object? sender, MessageReceivedEventArgs e) {
        switch (e.DecodeResult.Message) {
            case TimeNotification timeNotification: ConsoleEx.Log($"Server time: {timeNotification.Time}"); break;
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Handles client state changed events.
    /// </summary>
    /// <param name="sender">Client.</param>
    /// <param name="e">State event data.</param>
    static void Client_StateChanged(object? sender, StateChangedEventArgs e) => ConsoleEx.Log('i', $"Client state: {e.State}.");

    /// <summary>
    /// Handles client receive exceptions.
    /// </summary>
    /// <param name="sender">Client.</param>
    /// <param name="e">Exception event data.</param>
    static ValueTask Client_OnReceiveException(object? sender, ExceptionEventArgs e) {
        if (e.Exception.Data is not null && e.Exception.Data["TypeId"] is int typeId && e.Exception.Data["rawData"] is Memory<byte> data) {
            ConsoleEx.Log('i', $"Unknown message type received, id={typeId}, hex=0x{typeId:x8}, raw data:");
            HexStream.WriteData(data);
        }
        else ConsoleEx.Log('e', e.Exception.Message);
        return ValueTask.CompletedTask;
    }

    #endregion

}