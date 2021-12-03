# Woof.CommandLine

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

## About

Advanced command line parser using POSIX, PowerShell and DOS command line
arguments formatting guidelines.
Automatically generates console documentation from options and assembly
metadata.

It replaces all previous versions of the package and it's completely
incompatible with them.
Please analyse the test project sources carefuly to understand how it works.
The current module is much more advanced, modern and complete.

### Key features

The command line module is accessible application wide via static class
`CommandLine`.
There's no need to pass the module as service via DI.
It is because command line arguments are given once, on the application start
and will not change.

If for some reasons parsing of various separate command lines is needed, the
`CommandLineParser` instance can be used for that.

The command line arguments are divided into options and parameters.
The option is an argument that starts with a prefix defined for specified
syntax.
The option can have a value either defined in the next parametr, or in the same
parameter after setter separator (default `':'` or `'='`).
The options in POSIX syntax can be also grouped, so `"-abc"` is equivalent to
`"-a -b -c"`, and `"-ab=1"` is equivalent to `"-a -b 1"`.

The options recognized by parser are defined with configured enumerations.
Each configured option should be decorated with `Option` attribute.
The attribute defines the option aliases (names recognized), value placeholder
text and the optional description.
The option attribute can also have `Required` property used for validation.

The option enumerations are mapped to the parser with the `Map<TEnum>()` method.
Multiple enumerations can be mapped to the same parser.

Validation of the arguments given against the options enumeration can produce
detailed error messages.
To get separate validation error messages get the `ValidationErrors` property
of the `CommandLineParser` instance.
To get validation errors from the default instance as a single block of text
use the `ValidationErrors` property of the `CommandLine` static class.

Defined optons can be also mapped to delegates, and the delegates will be run
when matched with `RunDelegates()` or `RunDelegatesAsync()` method.
The delegates can be either synchronous or asynchronous and can accept the
option value if applicable. The delegates can accept `string`, `int`
and `double` types.
The values will be parsed using `InvariantCulture`.

The `CommandLine` class also provide automatic documentation created from
options enumeration and assembly metadata.
The full automatic documentation is available in `CommandLine.Help` property.

## Usage

For basic and advanced usage examples check the test modules sources included
in the GitHub project. Also refer to the XML documentation.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.