# Woof.Windows.Controls

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

This package contains some tiny but useful WPF controls.
- **Checks**:
  *A kind of `Menu` having a label containing a list of selected options or
  a fallback text for nothing selected or available*.
- **Spinner**:
  *A Windows 8 / 10 style loading animation with optional percentage label*,
- **TextBoxEx**:
  *A `TextBox` extension diplaying a label placeholder when it's empty*.

## Usage

This controls uses `Woof.Windows.MVM` because it's designed for MVVM.
The control's `ItemsSource` should be bound to an observable collection,
the `ObservableList<T>` is preferred.

The only acceptable item type for that control is
`Woof.Windows.Mvvm.Check`.

The item contains the `Value` property (typically - the item text) and
the `IsChecked` property that is true when the item is checked.

When the user checks or unchecks an item in the control -
the `PropertyChanged` event is triggered on the view model collection
if it implements `INotifyPropertyChanged` interface.

Handle the event in order to apply changes to your view model depending
on the items selection state.

See the provided demo project and refer to built-in XML documentation
for more details.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.