namespace RxUIExt;

using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

// Original issue https://github.com/reactiveui/ReactiveUI/issues/1536

/// <summary>
/// A reactive command which isn't restricted by double-tap protection, such that it can be executed multiple times
/// at once. Each new invocation automatically cancels the previous one. Other than that, it works and feels exactly
/// the same as regular reactive command.
/// </summary>
public static class SerialReactiveCommand
{
    public static SerialReactiveCommand<TParam, TResult> CreateFromObservable<TParam, TResult>(
        Func<TParam, ReactiveCommand<Unit, Unit>, IObservable<TResult>> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null)
    {
        if (execute == null)
            throw new ArgumentNullException(nameof(execute));

        var cancelCmd = ReactiveCommand.Create(() => { }, null, Scheduler.CurrentThread);
        var source = ReactiveCommand.CreateFromObservable((TParam p) => execute(p, cancelCmd), canExecute, outputScheduler);

        var cmd = new SerialReactiveCommand<TParam, TResult>(
            p => Observable.Using(
                // Subscribe to this ScheduledSubject to prevent exceptions firing twice to the DefaultExceptionHandler. The
                // .Using will dispose of this when done.
                () => source.ThrownExceptions.Subscribe(),
                _ => cancelCmd.Execute()
                              // Cancel but wait for execution to actually stop, otherwise double-tap lock on reactive command
                              // could prevent the next iteration as cancellation isn't done yet.
                              .SelectMany(_ => source.IsExecuting
                                                     .Where(i => !i)
                                                     .Take(1))
                              // Switch to next iteration
                              .Select(_ => source.Execute(p)
                                                 .TakeUntil(cancelCmd))
                              .Switch()),
            cancelCmd,
            canExecute ?? Observable.Return(true),
            outputScheduler ?? RxApp.MainThreadScheduler);

        return cmd;
    }

    public static SerialReactiveCommand<TParam, TResult> CreateFromObservable<TParam, TResult>(
        Func<TParam, IObservable<TResult>> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable<TParam, TResult>((p, _) => execute(p), canExecute, outputScheduler);

    public static SerialReactiveCommand<Unit, TResult> CreateFromObservable<TResult>(
        Func<IObservable<TResult>> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable<Unit, TResult>(_ => execute(), canExecute, outputScheduler);

    public static SerialReactiveCommand<Unit, TResult> CreateFromTask<TResult>(
        Func<Task<TResult>> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable(
            () => execute().ToObservable(),
            canExecute,
            outputScheduler);

    public static SerialReactiveCommand<Unit, TResult> CreateFromTask<TResult>(
        Func<CancellationToken, Task<TResult>> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable(
            () => Observable.StartAsync(execute),
            canExecute,
            outputScheduler);

    public static SerialReactiveCommand<Unit, Unit> CreateFromTask(
        Func<Task> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable(
            () => execute().ToObservable(),
            canExecute,
            outputScheduler);

    public static SerialReactiveCommand<Unit, Unit> CreateFromTask(
        Func<CancellationToken, Task> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable(
            () => Observable.StartAsync(execute),
            canExecute,
            outputScheduler);

    public static SerialReactiveCommand<TParam, TResult> CreateFromTask<TParam, TResult>(
        Func<TParam, Task<TResult>> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable<TParam, TResult>(
            param => execute(param).ToObservable(),
            canExecute,
            outputScheduler);

    public static SerialReactiveCommand<TParam, TResult> CreateFromTask<TParam, TResult>(
        Func<TParam, CancellationToken, Task<TResult>> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable<TParam, TResult>(
            param => Observable.StartAsync(ct => execute(param, ct)),
            canExecute,
            outputScheduler);

    public static SerialReactiveCommand<TParam, Unit> CreateFromTask<TParam>(
        Func<TParam, Task> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable<TParam, Unit>(
            param => execute(param).ToObservable(),
            canExecute,
            outputScheduler);

    public static SerialReactiveCommand<TParam, Unit> CreateFromTask<TParam>(
        Func<TParam, CancellationToken, Task> execute,
        IObservable<bool> canExecute = null,
        IScheduler outputScheduler = null) =>
        CreateFromObservable<TParam, Unit>(
            param => Observable.StartAsync(ct => execute(param, ct)),
            canExecute,
            outputScheduler);
}

/// <summary>
/// A reactive command which isn't restricted by double-tap protection, such that it can be executed multiple times
/// at once. Each new invocation automatically cancels the previous one. Other than that, it works and feels exactly
/// the same as regular reactive command.
/// </summary>
public sealed class SerialReactiveCommand<TParam, TResult> : ReactiveCommandBase<TParam, TResult>
{
    private readonly ReactiveCommand<TParam, TResult> _impl;
    private readonly ScheduledSubject<Exception> _exceptions;
    private readonly IObservable<bool> _canExecute;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    internal SerialReactiveCommand(Func<TParam, IObservable<TResult>> execute,
                                   ReactiveCommand<Unit, Unit> cancelCmd,
                                   IObservable<bool> canExecute,
                                   IScheduler outputScheduler)
    {
        _impl = ReactiveCommand.CreateFromObservable(execute, canExecute, outputScheduler)
                               .DisposeWith(_disposables);
        _canExecute = canExecute
                      .Catch<bool, Exception>(ex =>
                      {
                          _exceptions.OnNext(ex);
                          return Observable.Return(false);
                      })
                      .StartWith(false)
                      .DistinctUntilChanged()
                      .Replay(1)
                      .RefCount();
        _exceptions = new ScheduledSubject<Exception>(outputScheduler, RxApp.DefaultExceptionHandler)
            .DisposeWith(_disposables);
        _canExecute.Subscribe(OnCanExecuteChanged).DisposeWith(_disposables);
        Cancel = cancelCmd;
    }

    public ReactiveCommand<Unit, Unit> Cancel { get; }

    public override IDisposable Subscribe(IObserver<TResult> observer) =>
        _impl.Subscribe(observer);

    public override IObservable<TResult> Execute(TParam parameter) => _impl.Execute(parameter);

    public override IObservable<TResult> Execute() => _impl.Execute();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _disposables.Dispose();
    }

    public override IObservable<bool> CanExecute => _canExecute;

    public override IObservable<bool> IsExecuting => _impl.IsExecuting;

    public override IObservable<Exception> ThrownExceptions => _impl.ThrownExceptions
                                                                    .Merge(_exceptions);
}