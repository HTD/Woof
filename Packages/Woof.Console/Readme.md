# Woof.Console

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

Console extensions and filters allowing colored output and using multiple
cursors for advanced layouts.

Using the `Console` class for displaying colored text, updating text that is
already displayed is tedious. It requires a lot of code and repetitions.

Also, configuring the console output correctly for both Windows and Linux
terminal is not trivial.

The package takes care of the proper terminal configuration and allows to
use full potential of the console output with absolute minimal effort.

Just see the demo.

## Usage

To initialize the extended console with the default settings just use:

```cs
ConsoleEx.Init();
```

This will enable the colored output and set the UTF-8 encoding.

Now to output colored text, just use the special hex-codes.
If you're familiar with HTML and CSS colors, the hex-codes are just a type
of 3 characters hexadecimal codes designed to be as close to the RGB
representation as possible. So `f00` is bright red, `0f0` is bright green,
`00f` is bright blue, `777` is gray, `fff` is white, `000` is black and so on.
An optional fourth character `b` means the color should be set as the background
color.

Here's an example making the word "red" red:

```cs
Console.WriteLine("The word `f00`red` should be red.");
```

Here's an example making the word "red" inverse red:

```cs
Console.WriteLine("The word `000``f00b`red` should be red.");
```

OK, it can actually be pink-ish. Never mind.

Use the first '\`' character to start the hex-code. Then goes the hex-code,
then use the next '\`' character to end the hex-code. The following text will
be in selected color, until the next '\`' character is outputed. Then the text
color will be reset to the default value.

The way to achieve the same effect without `Woof.Console` would be like:
```cs
Console.Write("The word ");
var foreground = Console.ForegroundColor;
var background = Console.BackgroundColor;
Console.ForegroundColor = ConsoleColor.Black;
Console.BackgroundColor = ConsoleColor.Red;
Console.Write("red");
Console.ForegroundColor = foreground;
Console.BackgroundColor = background;
Console.WriteLine(" should be red.");
```
Just for the last example.

There's another neat feature, the `Cursor`.
It's basically a kind of a bookmark storing a console cursor position.
You can mark a place with it, and then output some text there later.
What's important, placing the text on the cursor position will not affect
the main cursor position.

It's mostly used by `ConsoleEx.Start()` method, that displays a "starting"
message and returns the cursor pointing to the end of the text.
Then you can start something asynchronous and display the result right
at the cursor position.

The example without `Woof.Console` would be even longer, so...
Just see how it's used in the demo.

The package has also some other cool features, like hex-data dump, of course,
in color. See the demo.

OK, but what about if the terminal doesn't support the colored output?
Well, that would be weird, but in such case the color codes will just be
ignored. See the demo.

But how does it work? See the source. It's not rocket science.
For full list of the color hex-codes, yes, see the demo.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.