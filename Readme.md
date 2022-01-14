# Woof Toolkit

**.NET** programming toolkit by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## Purpose

A set of `.NET` programming tools used by CodeDog company for rapid development
of enterprise grade software using the cutting edge technologies in following
fields:

- Server services
- APIs
- Communication
- Process automation
- IoT
- System administration
- Cross-platform
- HFT

## Reason

Although a large number of solutions in fields mentioned already exist,
each good software house needs its own tech tailored for more specific
tasks and production process.

It is also crucial to avoid code duplication. If two projects share some
similar code, it should be a true sharing, not duplication.

`.NET 6.0` already contains powerful tools for most of the tasks itself, but
it's designed for maximum versatility, and it's the only way it can be.
`.NET 6.0` exists to make complex tasks solvable using high level programming.

Woof Toolkit in designed for **more focused use** and **development speed**.

**The key reasons:**
- Each reusable component of the program must be fully tested
- Majority of CodeDog's production must be Open Source (MIT License)
- Upgraded features in one program should benefit all programs using
  the specific features.

A software house is as good as its tooling.

## Solution

NuGet packages. Each logical component is contained in a NuGet package
designed for minimal dependencies and focused on solving just one specific
problem. The dependency between packages is unavoidable, but it's reduced
to the absolute minimum.

Each package has the git version control and its sources are available on
GitHub.

Each package contains a demo application, optional unit tests or both.

Each package contains built in XML documentation, all public members of
all public classes are fully documented.

The solution file contains all current toolkit packages.

## Versioning

The current toolkit version is 6.2.0-beta.1.

That means the toolkit is in major redesign.

The major version number matches the current `.NET` framework version.
The minor version is the API version.
The subsequent revision numbers contain added features and fixed bugs.

Alpha versions contain work in progress.
Beta versions contain finished packages in testing.
RC versions contain finished and tested packages before integration tests,
if such tests are necessary for the specific packages.
Released versions are fully tested, however - expect the unexpected.

## Dependencies (internal)

Use the `Publish` tool to show detailed package dependencies.

## Building system

All packages are built to a common directory that is used to build the local
package repository.

To publish to external feeds like nuget.org use the `Publish` tool.

## Template

There is a special template project `Woof.Template` used as a template
for all packages. It contains all preset metadata and common project settings.
It contains no code.

## Coding style

- Opening braces in line
- `else`, `catch`, `finally` in new lines
- Type names in Pascal case
- Member names in Pascal case
- Backing fields names start with underscore
- File-scoped namespaces
- Implicit `using` statements
- Common `global using` statements
- Nullable feture
- Nullable rules violations cause warnings in `Debug` configurations,
  errors in `Release` configurations
- Default code analysis on
- Suppressed warnings must be justified (except CS8618 for DTO / records)
- Each public member must have XML documentation
- Each package must have `Readme.md` file
- Each package must have an example code using all package features

## History

`Woof` was started as a personal toolkit. It evolved into a truly reusable
library. Packages before version 6.2 were developed as separate solutions.

The versioning didn't followed the strict guidelines defined in this
document.

Also the strict separate of concerns principle is implemented since version 6.2.

Historically `Woof` supported various frameworks:
 - `.NET Framework` from  4.5 to 4.8
 - `.NET Standard 2.0`
 - `.NET 5.0`.

Supporting the legacy frameworks is in conflict with the development speed rule
defined, so the latest `Woof` supports

**the current `.NET 6.0` only.**

However, older versions of `Woof` were often Windows OS only,
the **current `Woof` is multiplatform**.

Each package is tested on both Windows and Linux.
(For now: Windows 11 and Ubuntu 20.)

## Future

`Woof` is here to stay. In 2022 you can count on very intense active
development. The 6.2.0 version will be rock solid, as the `.NET 6.0`
itself.

The whole toolkit is developed by just one person by now.
Join me any time.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.