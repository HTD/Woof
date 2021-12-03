# Woof.Net.WebSockets

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

## About

WebSocket Server, WebSocket client. You can build a WS API.

## Features:

A fast communication channel between any number of clients and
the server. It's way faster than REST, it's almost real-time.
Most online exchanges use it. The client can be used to access them.
Also, the client can receive notifications from server immediately.
It doesn't have to query the server all the time.

It also supports streaming. So it can stream audio, video and other data
very fast. Streaming means you can request any part
of the stream any time. No need to download a complete file.

Authentication is built in. Just implement `IAuthenticationProvider`
and you can use your secure API keys, secrets and message signing.
It works with real world crypto exchanges APIs.

## Provides:

- WebSocket client and server supporting any supprotocol and API.
- Comaptible with other clients and servers.
- Built-in subprotocol for fast and reliable message exchange.
- Support for unreliable connections, servers and clients.
- Support for any kind of authentication backend.
- Fully asynchronous, modern implementation.
- Simplicity (minimal implementation is trivial).
- Performance.
- Security (secure authentication and message signing support).
- Reliability (a test bench included).

## Usage

First time: Pull the Git repository. Run the provided test bench.
See the test source. There is full documentation in XMLdoc.

Not first time: Just install Woof.Net.WebSockets with NuGet.

READ:
 - [Docs/Basics.md](Docs/Basics.md) to understand the basics,
 - Docs/API-guidelines.md file before defining your own API messages.

---

## Disclaimer

This package is made for pros. For now. Using it requires some experience
in making network based applications and using some basic DI.
It makes implementing WebSocket APIs way easier, but it's not a trivial task
using ANY programming library. The goal of the package is to move the low-level
protocol handling out of the way.
It's meant to make things like authentication or exception handling as simple
as it gets. It doesn't mean it's simple per se.
Let's say with this package is as simple as using REST, or even a little easier.

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.