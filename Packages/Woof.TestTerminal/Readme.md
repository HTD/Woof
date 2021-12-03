# Woof.TestTerminal

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

## About

Displays a Windows Terminal window with optional split panels.
It chagnes the current directory to the current configuration output directory.
It sets the window title and prompt. To force administrator access, add the
`app.manifest` file to the terminal project.

The package is made to save you opening the terminal window yourself, then changing
the directory to the project's output.
And you don't see this annoying too long path to your project output directory
with no room to enter commands.

Important option is to test more than one project in one window using WT's
split panel feature.

This is a life saver when testing client-server scenarios, when you have both
in one solution.

## Usage

Start a new empty Windows application. It's important to change
`Console Application` to `Windows Application`. You don't need a console to use
`Windows Terminal`.

Download `Woof.TestTerminal` NuGet package.

Make `Program.cs` file like this:

```cs
using System;
using Woof.TestTerminal;

new Terminal {
    IsMaximized = true,
    Projects = new() {
        ["Tab Title"] = "ProjectName",
    }
}.Start(asAdministrator: false, "dir");
```

Project is a dictionary whose keys are user defined names and the values are
project names. Start method will start the Windows Terminal.

First parameter tells it to run the terminal as administrator.
The second parapeter contains additional commands to be run for each configured project
on terminal start.

If multiple projects added, they will be displayed side by side.

The terminal uses cmd, not PowerShell to make it simpler.
Changing the prompt for `PowerShell` is kind of "rocket-science" complex.
Also, if running or configuring your app requires `PowerShell` it's probably
done wrong and its command line interface is too complex.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.