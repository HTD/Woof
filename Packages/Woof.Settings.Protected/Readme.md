# Woof.Settings.Protected

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

The package adds a data protection to the `Woof.Settings` settings files.
The `JsonSettingsProtected` class has one additional method - `Protect()`.
When it's called the settings file gets encrypted and the file is only
readable to the user who first executed the application, or for all users on
the same computer. The original, unencrypted settings file is deleeted.

If the settings is already protected - call to `Protect()` is ignored.

To use this package on `Linux` additional dependency of
`Woof.DataProtection.Linux` is required.

Also, **using `DataProtectionScope.LocalMachine`
on Linux requires root permissions.**

In order for the protection to work, the settings directory must be
writeable to the user. If the user doesn't have write permission to the
settings directory or the original file can't be deleted, the `Protect()`
call will throw `UnauthorizedAccessException`.

## Usage

To create singleton (configured for the current user):
```cs
class Settings : JsonSettingsProtected<Settings> {

    public static Settings Default { get; } = new();

    private Settings() : JsonSettingsProtected(DataProtectionScope.CurrentUser) { }

    // Settings properties go here...

}
```


To load and protect the settings:

```cs
Settings.Default.Load().Protect();
```
or
```cs
await (await Settings.Default.LoadAsync()).ProtectAsync();
```

Chaining the asynchronous methods looks not great, but still works.

As for the main `JsonSettings` module, it will not throw when the settings
file doesn't exist. The properties would just have their default values.

It is the user's code responsibility either to provide sane defaults or
just test whether they are set with the settings file.

For more details use the XML documentation, read the documentation for the
`Woof.DataProtection`, `Woof.DataProtectionLinux` packages, check the package
source code, tests and examples.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.