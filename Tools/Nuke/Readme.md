# NUKE

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

This program clears the package cache and removes all `bin` and `obj`
directories below its current directory. This is required when a development
package is changed (a bug fixed), the package is used as a dependency for other
packages or projects.

Normally the version of the affected package and all affected packages should
be bumped up to make programs use the fixed version. It's not always desirable,
especially during the development process.

When a bug is fixed and the package is rebuilt, it is not enough.
All projects and packages that use the fixed package will use the cached version.

The cache is scattered all over various directories, it's not only in the common
`%USERPROFILE%\.nuget\packages` directory. All cache directories must be purged
in order to use the recent, fixed version of the package.

You should close the Visual Studio to ensure all target directories will be properly
purged.

After the operation all projects will need to be rebuilt.

The program has no dependencies itself. It should be published with the configuration
provided. Default target directory is main Woof Toolkit directory.

Published file is a single Windows x64 executable file `Nuke.exe`.

## Usage

- Launch `Nuke.exe` in your main source repository. Press any key to proceed,
  press Ctrl+C to cancel.
- Rebuild the Woof Toolit with the `Batch build` / `Select all` / `Rebuild` option.
- Launch `Woof.RepositoryManager` tool to reset the local repository.

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.