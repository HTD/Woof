# Woof.Config.Protected

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

## About

The package adds a data protection to the `Woof.Config` configuration files.
The `JsonConfigProtected` class has one additional method - `Protect()`.
When it's called the configuration gets encrypted and it's only
readable to the user who executed the application, or for all users on
the same computer. The original, unencrypted configuration is deleeted.

If the configuration is already protected - call to `Protect()` is ignored.

To use this package on `Linux` additional dependency of
`Woof.DataProtection.Linux` is required.

Also, **using `DataProtectionScope.LocalMachine` on Linux requires root permissions.**

In order for the protection to work, the configuration directory must be
writeable to the user. If the user doesn't have write permission to the
configuration directory the `Protect()` call will fail returning false.
Application will still work, but the data will not be protected.

## Usage

To protect the configuration for the current user:

```cs
var config = new JsonConfigProtected().Protect().Get<AppConfiguration>();
```

To protect the configuration for the local machine and throw an exception
if the data protection is not available:

```cs
var json = new JsonConfigProtected(DataProtectionScope.LocalMachine).Protect();
if (!json.IsProtected) throw new InvalidOperationException("Data protection failed");
var config = json.Get<AppConfiguration>();
```

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.