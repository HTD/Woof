# Woof.Settings

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

## About

Provides JSON settings files for the modern `.NET` applications.
Current `.NET` doesn't support the legacy XML settings files.
This package provides something much better.
A complex program configuration can be created as an object containing
properties, that can be either objects and collections.
The initial values can be provided in a JSON file distributed with the
application, or created on first program run.
All changes to the configuration values can be saved on demand, both
synchronously and asynchronously.

The package provides serialization of complex objects, arrays and lists.
Almost all simple system types are converted. Also the advanced types, like
`IPAddress`, `IPEndPoint`, `Uri`, `Guid`, `FileInfo`, `DirectoryInfo`,
`DateTime`, `DateOnly`, `TimeSpan`, `TimeOnly` and `byte[]` are supported
by default.

No manual mapping is necessary, JSON structure is mapped to the configuration
object structure fully automatic.

This package is designed for all types of applications, it is not targeted
specifically for `ASP.NET` so it's DI-free. Create the configuration objects
directly as singletons. It works on Windows, Linux, FreeBSD and MacOS in all
types of projects.

## Usage

Create a configuration structure as an object containing public properties.
The properties can be also the objects containing public properties.
Such properties can be called sections. Each property can also be an array
or a list. The collection element can either be a direct value, or a
configuration section. For constant elements number use arrays, otherwise use
lists. A `HashSet<T>` can be used instead of a `List<T>` when needed.
Use the appropriate strong types instead of strings for all types supported.

When the configuration structure is done, it should just extend the class
`JsonSettings<T>`. That will provide `Load`, `LoadAsync`, `Save` and
`SaveAsync` methods.

**The settings are NOT loaded in the constructor. `Load` or `LoadAsync` must
be called in order to do so.**

The `T` type is the type of the configuration object itself. Non public and
static properties of the object are ignored when loading and saving the
state.

The settings are located in the application folder (primary location)
and in the user folder (secondary location).

When not found in either folder, default values are used if defined in code.

For exact location of the application and user folders with settings refer to
- `JsonSettingsLocator.LocalAppDataTarget` for Windows,
- `JsonSettingsLocator.HomeDirectoryTarget` for Linux.

For more implementation details see the included XML documentation and
source code.

## Release notes

This is a descendant of the old `Woof.Config` package.
It's a replacement for it. It drops the `IConfiguration` interface as
obsolete and unnecessary. It provides much simpler interface to reduce the
boilerplate code to practically zero.

It also has no dependencies, using only the `.NET 6` core classes.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.