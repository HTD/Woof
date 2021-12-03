# Woof.Internals

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

## About

Contains some basic tools shared by many `Woof Toolkit` packages.

1. `ApiResolver`
   A static class used to load implementations from other referenced modules
   that are optional for the package.
2. `Application`
   A static class providing information about the current executable file,
   including its location and file name.

This dependency is made not to repeat the same blocks of code in packages
that often are used together.

Typical usage scenario:
The `Woof.DataProtection` can be used with `Woof.DataProtection.Linux` but
the Linux package is not a dependency of `Woof.DataProtection`.
However, when both packages are included in the project, the
`Woof.DataProtection` `DP` module will properly initialize the Linux module.

In order to do so it uses the `ApiResolver` class.

Many packages use access to file paths relative to the main executable file.
They also use application executable name, with or without extension.
In order to make it simple, the `Application` class exists.

It is important that the `Assembly.Location` property does not work with
builds published as a single file. The `Application` class solves this problem.

It doesn't do much, but it replaces really next to impossible to remember 
invocations with simple, obvious names that doesn't require refering
to documentation each time the property is needed.

Woof Toolkit is focussed on compact IoT apps inteneded to be built as
single files. On Linux their configuratons are placed in the same directory,
just with specific extensions. So the `main executable` term is occuring
in the Toolkit quite often.

Configuration files in the same directory as the main executable is a very
basic and default concept. Of course the location can be changed, but
the default, the simplest option is this.

BTW, main executable directory and the current directory are not the same things.
They are usually equal to each other, but it is not always the case.

## Usage

See the XML documentation provided in sources.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.