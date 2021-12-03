# Woof.Net.WebSockets basics

## Minimal setup

To establish the most basic communication channel we need a server, a client and an API.

The `Server` class will listen to incomming connections, the `Client` will connect to the `Server`.
The API will just define the types of messages that both `Client` and `Server` can exchange.

There is an internal API built in Woof toolkit, so you can even skip the API part for a while.

Let's start from the server. Here's the most basic implementation:

```cs
public class TestServer : Server<WoofCodec> {

    public TestServer() {
        EndPointUri = new Uri("ws://127.0.0.1:12345");
        Codec.LoadMessageTypes<Woof.Net.WoofCodec>();
    }

}
```

The server would be listening on given IP and port.
It will understand messages present in the assembly containing the type `Woof.Net.WoofCodec`.
That's it.

It's not much. You can ping it and query for supported message types.

Let's leave it for brevity.

Now we implement the client:

```cs
public class TestClient : Client<WoofCodec> {
    
    public TestClient() {
        EndPointUri = new Uri("ws://127.0.0.1:12345");
        Codec.LoadMessageTypes<Woof.Net.WoofCodec>();
        Id = new Guid("{F2A4E046-C9F3-4292-9445-D98C7DE605F4}");
    }
}
```

It looks almost identical as server.

And this should already work due to internal API.
It's recommended to put those classes into separate projects of the same solution.

The solution can be set up to run them both, server first.

## Making them talk

Both client and server must be instantiated and started to communicate.

The end points (a common name for client and server here) are both `IAsyncDisposable` so they should be instantiated like this:

```cs
await using var client = new TestClient();
```

```cs
await using var server = new TestServer();
```

They are started with:

```cs
await client.StartAsync();
```

```cs
await server.StartAsync();
```

Now your code should asynchronously wait for the stop signal.
For the testing purposes even `Console.ReadLine()` should do.

To shutdown just call `server.StopAsync()`.

To test if they see each other you may call `PingAsync()` from client.

```cs
var time = await client.PingAsync();
```

You will receive the time it took to send the `PingRequest` and receive the `PingResponse`.
Note that the first message will take much longer to exchange, each subsequent message will be passed much faster.

To learn about other messages that are built in the package you might try `QueryApiAsync()` method of the client.

```cs
var messages = await client.QueryApiAsync();
```

You will receive a dictionary containing message identifiers and message class names.
Those are messages loaded with `Codec.LoadMessageTypes<T>()` method.

## The API

To make them exchange the user defined message the API is needed.
The API should be implemented as a separate project / assembly referenced by both
client and server.
It should be general class library.
It should contain classes that will define messages.

Let's define our first request:

```cs
[Message(0x6000), ProtoContract]
public class MyTestRequest { }
```

And our first response:

```cs
[Message(0x6001), ProtoContract]
public class MyTestResponse {

    [ProtoMember(1)]
    public int Answer { get; set; }

}
```

What happens here: 
The `[Message]` attribute and the hexadecimal numbers are the message identifier.
They should be unique. One number represents one messsage type.

To avoid getting lost in message numbering there are API guidelines defined in separate document.

According to the guidelines, 0x6000 is a start of user's data queries space.

`[ProtoContract]` tells the Protocol Buffer codec that it's a serializable class
that can be treated as a message.
The `[ProtoMember]` attribute states, that the property should be serialized, the number tells about the order in the serialization sequence. The numbers should be unique in a single message. Zero should not be used. For more information read some
Protocol Buffers documentation.

You can serialize as messages all types the Protocol Buffers can serialize.

Wait! You should load your messages. Let's assume you put your messages in a new project called `Test.Api`, namespace `Test.Api`.

To make life easier, define another class there, just as reference:

```cs
public abstract class Loader { }
```

Empty, that is.

Now let's back to our cliend and server constructors. Replace the `LoadMessageTypes()`
part with:

```cs
Codec.LoadMessageTypes<Test.Api.Loader>();
```

See, what we did here? That way both the client and the server will know that it should
load the assembly containing the class, that assembly also contains our messages.

## Make the server understand the request

In order to do so, it's best to just override `OnMessageReceivedAsync()` method:

```cs
protected override async ValueTask OnMessageReceivedAsync(DecodeResult decodeResult, WebSocketContext context) {
    await base.OnMessageReceivedAsync(decodeResult, context);
    switch (decodeResult.Message) {
        case MyTestRequest:
            await SendMessageAsync(new MyTestResponse { Answer = 42 }, context, decodeResult.MessageId);
            break;
    }
}
```

What we have here? A `DecodeResult`. It contains many useful information about message decoding status, our only points of interest here are `MessageId` and `Message`.
The `MessageId` property is a unique identifier of the message sent by a client. When we send the response with the same identifier - the client will know it's the response for its particular message.

Note that using request-response convention is optional here. You can just send the message and no response is really required, unless you explicitly await it.

To be able to await the response - you must set a unique identifier for the request.
In `Woof.Codec` a `Guid.NewGuid()` is sufficient.

In the `switch` block we use pattern matching to match the specific request type.
When the type is matched, we can send the response with `SendMessageAsync` method.

It takes the response as the first parameter, then the context (it's bound to the client's connection), then the identifier to match the response to the request.

It's all for the server for now.

## Make the client to query the server:

In the client we need just one simple method:

```cs
public async ValueTask<int> AskAboutMeaningOfLifeAsync()
    => (await SendAndReceiveAsync<MyTestRequest, MyTestResponse>(new ())).Answer;
```

It does just one simple thing - sends the test request and awaits the test response.

That's it for the basics...

Note that all calls to the `Server` and `Client` methods MUST be asynchronous.
In specific cases that your asynchronous implementation just returns immediately,
just return `ValueTask.CompletedTask`.

Both `Client` and `Server` trigger some events, most of the events are asynchronous.

Never, ever use `async void` for any event handling, nor block the async threads.

Also things like `Task.Run()` or `Task.Factory.StartAsync()` should be avoided unless
explicitly needed.

What happens if the message handling code (`OnMessageReceivedAsync()`) on server throws
an exception? The magic happens. The exception will be caught by the server's receiving loop, encoded into a special message and sent to the client. Then the client
mehtod (like `AskAboutMeaningOfLifeAsync()`) will throw similar exception. That you can
catch and handle in client's code.

The exceptions won't break the client-server connection. They are normal part of the communication protocol.

The exception handling here is very basic, it's intentionally made simple to make it as
automated as it gets. But of course, it can be extended in user's code.

## Further reading

In order to fully utilize the power of this package you need read the API guidelinses.
Then carefully study the example test code on GitHub. It uses some more advanced
features like authentication, message signing, subscriptions, streaming, advanced
exception handling, egde cases, time-outs, unexpected messages, advandced exception
handling and more.

In order to use client authentication features you need to implement 
`IAuthenticationProvider` interface. It's used to verify API keys and message signatures.

In order to use a different message serialization subprotocol you need to implement
`SubProtocolCodec` abstract class.

Refer to XMLDoc in sources and package documentation.

## Practical use

To use a server you need a server. It can be either Windows or Linux.
This package was tested on Windows 10, Windows 11 and Ubuntu.

BTW, IRL you also need a HTTP proxy to route the internal network trafic to the Internet. We tested it on Apache. Refer to your particular reverse-proxy documentation.

The code should run on anything that can run .NET 5.0 or newer.

Best way to use it on server is to make a system service.
On Windows you have Windows Service API, on Linux - systemd.

Both are covered by `Woof.ServiceInstaller` package.

The client can be built in anything. Can be a desktop app, mobile app, console app, web app using `Blazor` for example.

You can also write a client from scratch using any programming language like Python, Java, C, Go, Rust... Making WebSocket clients is way easier than making the server.

We are currently working on low-level client for IoT in C.
