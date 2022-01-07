# Woof.DataProtection

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

Provides a basic abstraction for the DataProtection API and data protection
methods for Windows.

Although the most of the Woof Toolkit is multi-platform, the Windows
`DataProtection` API is built in both in the framework and the operating
system. For other operating systems additional coding and setup is required.

The goal of this module is to relay all the OS specific operations to the
extending packages, like `Woof.DataProtection.Linux`.

If the OS specific extension package is present in the project, it will be used
to provide the data protection API.

### IMPORTANT NOTICE:

As I needed only the Linux version and I don't even have any OSX or FreeBSD
machines, if you need DataProtection support for those systems - please
either create an issue on GitHub, or feel free to implement it yourself
using `Woof.DataProtection.Linux` as an example. Don't forget about creating
a pull request when it's done, so it will be properly signed and released.

## Usage

Binary data protection:
```cs
var encrypted = DP.Protect(sensitiveData);
var decrypted = DP.Unprotect(encrypted);
```
String data protection:
```cs
var encryptedText = DP.Protect(plainText);
var decryptedText = DP.Unprotect(encryptedText);
```
To test if the data protection is available on target system:
```cs
if (DP.IsAvailable) {  
    // use the data protection
}
```

An optional parameter of type `DataProtectionScope` can be used.
If this parameter is not specified, the default protection scope is
`DataProtectionScope.CurrentUser`.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.