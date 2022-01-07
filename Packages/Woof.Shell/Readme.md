# Woof.Shell

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

Provides fast shell commands execution on both Linux and Windows.

- Windows shell: `cmd.exe`
- Linux shell: `bash`

Commands can be run synchronously or asynchronously.
Asynchronous methods return `ValueTask` types.

Commands can throw exceptions on errors or ignore them silently.

The `standard output` and `standard error` can be either returned or ignored.

The command arguments can be either provided inline with the command or
as separate strings.

In synchronous methods the current thread is blocked until the command
process ends.

The asynchronous methods will return a `ValueTask` that will be completed
when the command process completes or fails.

## Usage

- Create a `ShellCommand` instance like:

  ```cs
  var command = new ShellCommand("ls -l ~");
  ```
  or
  ```cs
  var command = new ShellDommand("dir %LOCALAPPDATA% /O:GN");
  ```
- Execute like:
  ```cs
  var output = command.Exec();
  ```
  or
  ```cs
  var output = await command.ExecAsync();
  ```
- Create and execute inline like:
  ```cs
  var output = await new ShellCommand("ls -l ~").ExecAsync();
  ```
  or
  ```cs
  await new ShellCommand("del *.tmp").ExecAndForgetAsync();
  ```
Refer to the `ShellCommand` class XML documentation for advanced usage.

## Cross-platform use

Like this:
```cs
var output = await ShellCommand(
    OS.IsLinux ? "ls -l ~" :
    OS.IsWindows ? "dir /O:GN %LOCALAPPDATA%" :
    throw new PlatformNotSupportedException()
).ExecAsync();
```

## Also in package

`FileSystem.CopyDirectoryContent()`

Just see the method XML documentation.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.