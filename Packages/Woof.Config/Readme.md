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

The location of the file must be obvious. So does it's name.

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

Once `Woof.JsonConfig` object is loaded, it provides both pahts for that purpose.

In order to use the configuration the JSON file must be created first.

It provides the structure for the configuration properties.

Then, to access the configuration data in program - either `IConfiguration`
methods and properties can be used, or the object can be bound to a strong typed
configuration `record` with `IConfiguration.Get<T>()` extension.

Yes, the recommended type for the configuration section is a `record` type.
All objects (mapped to sections) should be read-only pre-initialized getters.

Each complex type must be defined as section record.

For example, if we defined the configuration both in `JSON` file and already
created a strong typed record for it named `Configuration`, here's an example
usage:

```cs
var jsonConfig = new JsonConfig();
var config = jsonConfig.Get<Configuration>();
```

In order to modify a configuration value use `IConfiguration.SetValue<T>()`
extension method.

A modified configuration can be saved like this:

```cs
await jsonConfig.SaveAsync();
```

or

```cs
jsonConfig.Save();
```

For more details see the provided examples and use built-in XML documentation.

## Version 6.2 API change

This package is incompatible with older versions.
Previous `Woof.Config` packed too many features in one module.
It provided both `data protection`, `Azure Key Vault` access and
`data encryption`.

Those features was moved to separate packages:

- [`Woof.Config.AKV`](../Woof.Config.AKV/Readme.md)
- `Woof.Config.Protected`
- `Woof.Config.AKV.Protected`

Read the packages documentation in order to migrate.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.