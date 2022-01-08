# Woof.Windows.MVVM

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

WPF MVVM data binding made easy!

This is model to view model to view binder. Using this you don't mix your data
retrieving code with data processing code and you don't mix both with UI.
In MVVM the view is just XAML that defines the visual part only.
Code behind is not used in XAML views at all.
Models can be any data source. If it provides a collection of items,
they can be fed to DataGrids and what not.

The best thing about this package - it can really make use of asynchronous
streams to make the UIs snappy.

Binding views to data sources is maybe not very hard in WPF,
but it's tedious and time consuming.
Out of the box you get just some interfaces to implement.
Well - they are partially implemented here, in the easiest configuration
possible.

Another thing is we need interaction with the data in UI, we just don't want
to implement it in views.
That's what view models are for. The views should have no logic.
The catch is the views trigger events, and they cannot be easily passed to view
models.

I mean - without `Woof.Windows.MVVM`. With this package it's easy.

## Usage

### Creatig a view model

To implement MVVM you need at least one view model (obviously).
To create a view model just inerit from `Woof.Windows.Mvvm.ViewModelBase`.

If you want some data like in Excel, create `ObservableList<T> Items` in the
view model like this:

```cs
public ObservableList<T> Items { get; } = new();
```

It's important to have it read-only and created in implicit constructor.

You see, the WPF controls observe observables, so when you add or remove items
to the view model, the view will know that and will update automagically.
(The `ObservableList<T>` will just trigger `CollectionChanged` event for you).

Disposable items? No problem, `ObservableList<T>` will dispose items when they
are removed, replaced or just cleared from the list.

Do you want to bind the data "the other way", like using EDITABLE data grid?

Now that you have to implement yourself but it's still easy.
Make each item implement `INotifyPropertyChanged` and just observer
`PropertyChanged` events. You will know that the view changed an item and what
part of the item was changed. It's up to your code to react to the change.

### Creating the view

The view can be a control, page, window. Anything in XAML.
To make the view load the data from the view model automatically first time
it becomes visible use `AutoLoadView` control like this:

```xml
<woof:AutoLoadView x:Class="Your.View.Class.Name"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:woof="clr-namespace:Woof.Windows.Mvvm;assembly=Woof.Windows.Mvvm"
    xmlns:vm="clr-namespace:Your.View.Models.Namespace">
    <woof:AutoLoadView.DataContext>
        <vm:YourViewModelClass/>
    </woof:AutoLoadView.DataContext>
    <!-- Here goes the rest of the view... -->
</woof:AutoLoadView>
```

`AutoLoadView` is a `UserControl`. Just an empty container that provide
the data context to everything that's inside. What data context? Your view
model, the class derived from `ViewModelBase`.

In order for automatic loading to work the view model must implement
`IGetAsync` interface.
Obviously this interface needs only one method to be implemented. Guess its
name. The method, obviously - loads the data for the view from the model.

As it name and return type suggests - this should be done asynchronously,
to prevent the UI from lagging while loading the data.

Here's where `Woof.Windows.Mvvm` shines!

```cs
public ValueTask GetAsync() {
    IAsyncEnumerable<T> data = MyModel.GetDataAsync();
    Items.Clear();
    Items.AddRangeAsync(data);
}
```
Let's say the data must be downloaded and it will take a while.
Let's say you have like 100 rows, but only first 10 fits in the small window.
The asynchronous code will start filling the view IMMEDIATELY, so your view
will be filed with data while still loading the rest of it.

If your data source doesn't support asynchronous streams, just await for the
complete result. The main UI thread won't be blocked so your app at least could
be responsive and display a nice loding animation.

### Passing events as commands

When you need an event to be passed from the view to the view model, here's how:

```xml
<!-- Don't forget about the namespace:
     xmlns:woof="clr-namespace:Woof.Windows.Mvvm;assembly=Woof.Windows.MVVM" -->
<MyControl>
    <woof:Mvvm.Events>
        <woof.MvvmEvent EventName="Loaded" Command="{Binding}">
    </woof.Mvvm.Events>
</MyControl>
```

Now, when you have overriden `CanExecute()` and `Execute()` in your view model,
you will receive a special parameter. It will be of type `MvvmEventData`.

You can check it's name and optional data it provides and handle it in your
view model.

To see how it works "live" just download the included demo project and play
with it.

The data binding in WPF can be automated more than this, but know that it comes
with a cost. You add automatics, but also code dependencies, code complexity
and considerable time to configure the automation helpers.

It can be also automated less, so you don't use this package, but then you have
to write a lot of boilerplate code to do the simplest data binding.

So here's the sweet spot. It can be used as a base for more advanced things.

The goal of this package is to be as simple and easy to use as it gets. If it
still seems complicated - well... It just can't be done easier. Unless you
would use a kind of box-application-generator, but then you will have zero
control over how it works and what it does, 100% dependency on the tool and its
dependencies.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.