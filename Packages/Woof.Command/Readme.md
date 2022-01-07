# Woof.Command

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

A general purpose command line interface.
Mimics some `bash` and `PowerShell` functionality providing persitent history
and advanced auto-complete feature activated with tab key.

Based on `System.Console`, requires `cmd`, `PowerShell` or similar to run.

## Usage

Create an instance of `CommandShell` class.
When started with the `Start` method it will work similarily to `cmd`.
It's not very useful unless you add some custom commands to it.

The `CommandShell` provides `Command` event that is triggered each time
the user press enter. As the event arguments you get the command
entered parsed. Quoting is properly processed using `cmd` syntaxt.

You get separate command name and arguments passed.

The response output can be set by settings the event arguments.

You can also add manual entries for your custom commands.
Just add entries to the instance's `ManPages` dictionary.

They will be shown when `man` commmand is issued without parameters.

The clas also provides auto-complete feature that can be highly custimized.
It was inspired with Linux bash on ubuntu that shows suggestions and
completions for some external programs.

By default it completes available commands and file names.

Just press tab to see available files and directories.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.