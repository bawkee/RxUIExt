namespace RxUIExt.Windsor;

using Castle.Windsor;
using ReactiveUI;
using Splat;
using System;

/// <summary>
/// Use this View Locator in your ReactiveUI projects to hook it up with Windsor Castle.
/// </summary>
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

    /// <summary>
    /// Resolves a view from the container according to the provided view model type, and window type.    
    /// </summary>
    /// <typeparam name="T">Type of the view model that you're looking a view for</typeparam>
    /// <param name="viewModel">Instance of the view model that you're looking a view for.</param>
    /// <param name="contract">Key which can be used when multiple views are found.</param>
    /// <returns>An instance of <c>IViewFor</c></returns>
    public virtual IViewFor ResolveView<T>(T viewModel, string contract = null) =>
        _container?.GetView(viewModel.GetType(), _windowType, contract);
}