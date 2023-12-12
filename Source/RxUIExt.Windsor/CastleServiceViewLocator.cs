namespace RxUIExt.Windsor;

using Castle.Windsor;
using ReactiveUI;
using Splat;
using System;

public class CastleServiceViewLocator : IViewLocator
{
    private static IWindsorContainer _container;
    private static Type _windowType;

    /// <summary>
    /// Let Castle manage ReactiveUI ViewLocator. The Window type argument is used to discern
    /// Window from non-Window views (pages, user controls, etc).
    /// </summary>
    public static void Register(IWindsorContainer container, Type windowType)
    {
        _container = container;
        _windowType = windowType;

        Locator.CurrentMutable
               .Register<IViewLocator>(() => new CastleServiceViewLocator());
    }

    public virtual IViewFor ResolveView<T>(T viewModel, string contract = null) =>
        _container?.GetView(viewModel.GetType(), _windowType, contract);
}