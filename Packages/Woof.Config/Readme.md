# Woof.Config

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

## About

A proper solution to replace the legacy `.NET project XML settings`.
Universal application settings in `JSON` for both Linux and Windows
programs.

## Usage

The goal is to provide a dynamic configuration for the C# program that is
easily accessible and editable either by the user or the app itself.

The location of the file must be obvious. So does its name.

The name of the file without extension matches the main program executable name.

The file can reside either in user's directory, or in the same directory
the application is installed.

It's a similar idea to the legacy .NET project XML configuration files.

The `Woof.Config` module determines the location of the file internally.
It searches for it first in the main executable directory, then in user's
directory.

### Linux path:
(In search order)

- ./`[executable_name]`.json
- ~/.`[executable_name]`/`[executable_name]`.json

### Windows path:

- .\\`[executable_name]`.json
- `%LOCALAPPDATA%`\\`[company]`\\``[product]``\\`[executable_name]`.json
- `%LOCALAPPDATA%`\\``[product]``\\`[executable_name]`.json
- `%LOCALAPPDATA%`\\``[executable_name]``\\`[executable_name]`.json

In development, the configuration should just be copied to the output
directory, then it can be moved to user's directory either by the application
or the application installer.

The file can even not exist at all, it will be created when the configuration
is saved.

To find out where exactly the file was saved, check the `FilePath` property
of `JsonConfig.Locator`. 

There are 2 basic types of the configuration:
- `IConfiguration` - a basic structure providing only string values,
- User defined object or record - that is bound to `JsonNodeConfiguration`
  or `JsonConfig` object. The property binding automatically convert all system
  types to and from `JSON` values. That type of configuration is strong typed.
  You can store values as `Guid`, `Uri`, `FileInfo`, `DirectoryInfo`, `byte[]`,
  `DateTime`, `DateOnly`, `TimeOnly`, and `TimeSpan` and many more. More type
  conversions can be added to the `PropertyBinder`.`Conversions`.

For example, if we defined the configuration both in `JSON` file and already
created a strong typed record for it named `Configuration`, here's an example
usage:

```cs
var section = await JsonConfig.LoadAsync();
var config = section.Get<Configuration>();
```

or

```cs
var section = new JsonConfig();
var config = section.Get<Configuration>();
```

A modified configuration can be saved like this:

```cs
await section.SaveAsync(config);
```

or

```cs
section.Save(config);
```
For more details see the provided examples and use built-in XML documentation.

## Current version (early development) limitations

The current version of `PropertyBinder` cannot process collections.
Do not use arrays, lists and other collection types with the binder.
Do **please open an issue on GitHub** if you need the feature and I will add it.

## Extensions

- The support for the data protected configuration files is in
  [`Woof.Config.Protected`](../Woof.Config.Protected/Readme.md).
- The support for values stored in Azure Key Vault is in
  [`Woof.Config.AKV`](../Woof.Config.AKV/Readme.md).

## Performance

The most expensive part of the configuration usage is the binding operation,
since it uses `Reflection` to match the `JSON` properties to the bound
object's properties. It is done on load and save operations.

If you need faster and more real-time application state storage consider
using the `Registry` on Windows and `Redis` on Linux.

## Technology

The main idea was to make a configuration section object that implements
`IConfiguration` and bind it directly to `JSON` nodes.
There already is an implementation of this using `System.Text.Json` in
`Microsoft.Extensions.Configuration.Json`, however - it's **READ ONLY**.
It is also tailored specifically for usage in web applications and
`ASP.NET Core` dependency injection pattern. It doesn't solve the problem
of locating the configuration file either in application's or user's directory.

Also extending the class to support advanced data protection and
Azure Key Vault storage was very difficult to achieve and required a lot
of inefficient workarounds.

The answer is the shiny new `System.Text.Json.Nodes` from `.NET 6.0`.
This feature not only allows parsing the `JSON` files, but also allows
modifying and building the `JSON` documents.

The core part of the package is `JsonNodeSection` class that is a combination of
`JsonNode` instance and a `IConfiguration` interface implementation.

Node sections are searchable with `Select()` and `GetSection()` methods.
The `Select` name is inspired by a similar method from `Newtonsoft.Json`
package.

The `JsonNodeSection` uses section paths (keys separated with `':'` character),
but also understands `JsonNode` paths like `$.section.value[1]`.

To allow building the document structure from scratch some metadata was added
to the `JsonNodeSection` type, like special kinds of `Null` and `Empty` nodes.
An `Empty` node is a node that not really exists in the `JSON` document.
The `Null` node exists and its value is equal to `null`.

That helps the section setter to create missing container nodes for the new
properties and values.

I made a completely new property binder to add some commonly used system types
and simplify extending the supported conversions.

The cool new features are storing `TimeSpan` values as floating point
seconds and byte arrays as `Base64` encoded strings.

The `JsonConfig` class uses several modules to implement the proper
separation of concerns and extensibility:
- `IFileLocator` - locates the configuration files.
- `IJsonNodeLoader` - parses, loads and saves the configuration nodes.
- `IPropertyBinder` - binds the configuration values to the strong types.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.