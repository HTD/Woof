# Woof.LinuxAdmin

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

Some advanced application features require some system
administration tasks to be performed before they can be used.

The common tasks are:
 - Installing and controlling system services,
 - System users and groups management,
 - File system permission management.

When the code is executed with administrative privileges, it makes sense
that it could configure the target system automatically without requiring
much additional system administration.

As the `.NET` platform already contains most of the tools itself,
they are currently available for Windows OS only.
`Woof.LinuxAdmin` provides unified API for both Linux and Windows.

Linux user information, group information, file system entry information:
they are all supported by `.NET` only partially, translated to their
Windows equivalents.

`Woof.LinuxAdmin` provides native Linux types:
 - `UserInfo`,
 - `GroupInfo`,
 - `StatInfo`.

The types are fully managed ones, not directly compatible with the corresponding
Linux kernel structures.

Linux file system entry binary mode **IS** identical with the `uint` type used
in the kernel.

Of course all file system manipulation tools can operate both on single entries
and entire directory trees.

To register, unregister or control system services use `Woof.ServiceInstaller`
module.

## Design

The core of the library is Linux native `Syscall` class.
It contains the signatures for `libc` system calls. There is a minimalistic
set of features used, just to enable basic Linux file system operations.

Original naming convention is dropped for .NET naming and signatures style.

The managed functions sit in `Linux` static class.

The functions mostly return `bool` values indicating that the operation
was successful.

## AddSystemUserAsync(), AddSystemGroupAsync() methods

The only asynchronous methods in `Linux` static class.
They are asynchronous since they use shell to call `useradd` and `groupadd`.

## Chmod(), ChmodR(), Chown(), ChownR()

`Chmod` changes permissions on a file system entry.
You can provide permissions as follows:

- octal string, like "664" (absolute read/write for user and group, read for others)
- relative string, like "ug+rw,o-w", "+x" and so on,
- binary - as `uint` Linux mode.

Conversions between `string`, `uint` and `Permissions` are made implicitly.

Special permission "X" also works, setting executable bit if the entry is
a directory or has at least one executable bit set.

It is important, that the execute permissions cannot be set on unreadable entries.
When the entry doesn't have a read permission for the specified target
it's execute bit will not be set for that target.

`Chown` changes an owner of a file system entry.

To change permissions and / or owner for all entries in a directory including
the directory itself use `ChmodR()` and `ChownR()` methods.

## Other tools

For the administration it's useful to check if the program has administrative
privileges. To do so use `CurrentUser.IsAdmin` property.

You can even check if the program was run on Linux with `sudo` command,
and not from the root account, to do so get the `CurrentUser.IsSudo` property.

To get detailed information about the current Linux user use:
 - `Linux.CurrentProcessUser`,
 - `CurrentUser.BehindSudo`

It can be useful to access the user's `Directory` (Linux home).

Use `StatInfo` for detailed Linux file system entry properties,
use `UserInfo` for detailed user information,
user `GroupInfo` for detailed group information including its members.

`Linux.HomeDirectory` contains home directory of the current user,
also in case if the user run the program with `sudo`.

You can also resolve paths containing `~` using the `HomeDirectory` property
with `Linux.ResolveUserPath()`.

## Performance hint

To set absolute permissions use octal strings (or binary) to skip calling
`stat` on each entry.

## Release notes

This version is a part of the 6.2, new top quality branch.
As the strict separation of concerns rule is applied, shell access
and data protection features were moved to the separate packages.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.