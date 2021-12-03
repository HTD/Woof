# Woof.Config.AKV.Protected

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

## About

An extension of the  [`Woof.Config`](../Woof.Config/Readme.md) and
 [`Woof.Config.AKV`](../Woof.Config.AKV/Readme.md) packages that
allows some configuration values to be not read directly from JSON file,
but from the Azure Key Vault secrets.

This achieves following important goals:
- sensitive data are not distributed with the program or its configuration,
- the access to the vault secrets can be revoked at any time,
- the data can be changed remotely at any time,
- the access to the vault can be limited to the configured end points,
- the access to the data from the code is trivial,
- the AKV configuration is trivial,
- sensitive data stored by the application can be easily encrypted and
  decrypted using a key stored remotely on AKV.

**Security considerations:**

Anyone having the appropriate `.access.json` file can access all vault secretes.
That file **CANNOT** be made public or accessible to anyone
but intended user.

To mitigate the vulnerability this package uses `Woof.DataProtection`
to protect the vault access configuration, so the file is only readable
either to the user who run the program the first time, or only on the
local machine (by all users).

**On Linux also `Woof.DataProtection.Linux` package must be referenced!**

**To use `DataProtectionScope.LocalMachine` the root permissions are required.**

## Usage

See the [`Woof.Config.AKV`](../Woof.Config.AKV/Readme.md) documentation first.

Example use:

```cs
var config = 
    new AkvJsonConfigProtected(DataProtectionScope.LocalMachine)
    .Resolve<AppConfiguration>();
```

When the application is built / installed, the vault configuration is
unprotected in a `*.access.json` file.

After the application is run for the first time, the vault access file
is encrypted using either the current user's key, or the local machine's key.
Then it is only usable on that machine, and optionally - to the original user.

In order to migrate application to another machine an original, unprotected
vault access configuration file must be provided.

**CAUTION:** If the data protection API is not available for the target OS,
or the data protection API is not operational because insufficient permissions,
system misconfiguration - the vault access file won't be protected.

If that happens on Linux system, double chceck if the `Woof.DataProtection.Linux`
package is referenced by the project.
Then check if the program user has appropriate permission for the key
directories specified in [`Woof.DataProtection.Linux`](../Woof.DataProtection.Linux/Readme.md) package documentation.

The protected files have the `.data` extensions and are unreadable as plain text.

It's worth noting that the main configuration file is NOT encrypted.
The sensitive data are stored in AKV anyway, so the access file is the
only file that needs to be encrypted.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.