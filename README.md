# RxUIExt 🧰🪟🐧🍏
Tools and extensions for ReactiveUI that will boost your productivity and app quality. I use this on daily basis, it includes some critical need-to-have tools when dealing with .NET UI frameworks, be it WPF, UWP, WinUI, Uno or Avalonia.

## Main Package (RxUIExt)

[![NuGet](https://img.shields.io/nuget/v/RxUIExt)](https://www.nuget.org/packages/RxUIExt/1.0.0)

This is the main multi-platform package. You don't need any bait-and-switch techniques for this, it will just work out of the box (like ReactiveUI does). Some of the tools include:

- Shortcuts like `ObserveOnMainThread` and basic `Interaction<,>`  extensions that are short and small but used thousands of times.
- Basic types like `ViewModel` and `ActivatableViewModel` that I use all the time (very little code)
- `SerialReactiveCommand` a command which allows serial execution, it automatically cancels the previous execution and starts a new one. I use this a lot when something needs to happen when a user selects something or when I want to allow users to change their minds in the middle of loading and start over. By default, `ReactiveCommand` has a double-tap protection, so once invoked, you have to either manually cancel it or wait for it to finish.
- `WhenExecuting` which ticks a `Unit` right before the `IReactiveCommand` starts executing.
- Idiomatic `WhenActivated` extension that also provides a `ViewModel` for you so it triggers when your viewmodel changes on the view itself. By default you are supposed to recreate the entire view if you change the viewmodel, which is not so very performant in any of the Microsoft's UI frameworks.
- `SubscribeSafe` which makes sure that all exceptions are propagated to `RxApp.DefaultExceptionHandler`. 
- `ViewModelChangeTracker` is an idiomatic mechanism that you can use on your viewmodels to track changes on properties. Very useful in CRUD operations so you know exactly what's changed.

## Windsor Integration (RxUIExt.Windsor)

[![NuGet](https://img.shields.io/nuget/v/RxUIExt.Windsor)](https://www.nuget.org/packages/RxUIExt.Windsor/1.0.0)

If you, like me, prefer using Castle Windsor for DI, then this will help you easily integrate it with your ReactiveUI app. It offers a built-in `IViewLocator` implementation and extensions that help you automatically resolve views and viewmodels through Castle's very flexible fluent API.

## WinUI Integration (RxUIExt.WinUI)

[![NuGet](https://img.shields.io/nuget/v/RxUIExt.WinUI)](https://www.nuget.org/packages/RxUIExt.WinUI/1.0.0)

Basic tooling for WinUI development such as:

- Flexible `BoolConverter` a `NullConverter`, you can do 99% of the UI with just these two since you can set True/Default or Null/NotNull values yourself in XAML or code-behind and just use them with your RxUI viewmodels. 
- Input dialog extension, similar to the one we had in VB6 days.
- `ViewHost` implementation that *actually* works with WinUI. Sadly, the default RxUI one leaves a bit to be desired.

## Other?

I will make Avalonia and WPF integrations in the future, covering some basic tools that everyone would use on a daily basis.
