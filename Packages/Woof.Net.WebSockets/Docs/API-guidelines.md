# API guidelines

## WebSocket API implementation

WebSocket API implementation requires:

- defining API messages
- implementing `Woof.Net.IAuthenticationProvider` to authenticate clients.
- implementing at least one class derived from `Woof.Net.WebSockets.Server`.
- implementing at least one class derived from `Woof.Net.WebSockets.Client`.

Server can be authenticated with standard HTTPS certificate.
In most cases client authentication with just an API key should be sufficient.

## Message identifiers

The simplest way to identify the type of the message is just use unique numbers.
The shortest numbers we could use are bytes. One byte allows 256 types of messages.
It's enough for simple applications, but it might be a little tight space.
Byte is also non-native for the C# language. Even boolean value in C# uses 32 bit
integer under the hood. We can still manipulate bytes and bits in C#, but using
integers (both 32 and 64 bit) is faster, especially 64 bit integers.

So it's done. We have 2 gigs of positive and 2 gigs of negative numbers.
For any application it's more than enough.

However - no matter if we use 8, 16, 32 or 64 bit integers - there are always possible conflicts.
The lower numbers are of higer demand. We just like them.
They are easy to remember and relate to different things.

But that's also their disadvantage. When the same number is used to define 2 messages
bad things happen.

Another problem is ordering. If we want messages grouped in a logical sequence,
we need the numbers to not be random.

But there is a problem when we want to add some messages related to some already defined logical part of the API.
The problem results with increasing mess in the message numbering.

With the huge 32 bit pool of possible numbers this problem can be efficiently solved.

### Message number pools

#### User space

- 0x0000..0x0FFF - RESERVED for protocol state and error messages, defined within Woof.Net.Messages namespace.
- 0x1000..0x1FFF - authentication (endpoint authenticates to the other endpoints)
- 0x2000..0x2FFF - notifications (endpoint notifies the other endpoint about its state changed)
- 0x3000..0x3FFF - subscription requests (endpoint subscribes to some notifications from the other endpoint)
- 0x4000..0x4FFF - subscribed notifications (endpoint notifies the subscribers about its state changed)
- 0x5000..0x5FFF - commands (endpoint instructs the other endpoint to perfom an action)
- 0x6000..0x6FFF - data queries (endpoint queries the other endpoint for some data)
- 0x7000..0x7FFF - data upload (endpoint uploads some data to the other endpoint, the data are not previously requested)
- 0x8000..0xFFFF - everything else

#### User subspaces

- 0x0001_0000 .. 0x0FFF_0000 - whenever you need to have some order like above, but specific to a separate system part
  for example - we have a subsystem using its own messages, but we want their numbers to be distinct from the main system, 
  so subsystem 1 commands space would be [0x0001_5000..0x0001_5FFF].

#### Woof space

The Woof toolkit may use it's own internal message types.

- 0x1000_0000..0x7fff_ffff - WOOF!

#### Negative space

- 0x8000_0000..0xFFFF_FFFF - for all your "special needs". can be confusing because written in hex can have 2 meanings.

## Streaming support

Woof subprotocol supports streaming data. However - system `WebSocket` implementation doesn't support multiple simultanous
data transfer over the same connection. For that reason a separate streaming channel (as a separate client / server instances)
should be provided using different IP end points.