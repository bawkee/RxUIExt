namespace RxUIExt.Windsor;

using System;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Context;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ReactiveUI;

/// <summary>
/// Various ReactiveUI extensions for Windsor Castle.
/// </summary>
public static class CastleExtensions
{
    /// <summary>
    /// Finds all <see cref="ViewModel"/> derived types and adds them to the provided container. Attributes are also available
    /// which can help you customize this behavior.
    /// </summary>
    /// <param name="container">Your DI container</param>
    /// <param name="assemblyDescriptor">Descriptor which tells us where to start looking.</param>
    public static void RegisterViewModels(this IWindsorContainer container, FromAssemblyDescriptor assemblyDescriptor) =>
        container.Register(assemblyDescriptor
                           .BasedOn<ViewModel>()
                           .Unless(t => t.GetCustomAttributes(typeof(DoNotRegisterAttribute), false).Any() ||
                                        t.IsAbstract ||
                                        container.Kernel.HasComponent(t))
                           .WithService.Self()
                           .WithService.DefaultInterfaces()
                           .Configure(c =>
                           {
                               var isSingleton = c.Implementation.GetCustomAttribute(typeof(SingletonAttribute), false) != null;
                               if (isSingleton)
                                   c.LifestyleSingleton();
                               else
                                   c.LifestyleTransient();
                           }));

    /// <summary>
    /// Finds all <see cref="IViewFor{T}"/> derived types and adds them to the provided container.Attributes are also 
    /// available which can help you customize this behavior.
    /// </summary>
    /// <param name="container">Your DI container</param>
    /// <param name="assemblyDescriptor">Descriptor which tells us where to start looking.</param>
    public static void RegisterViews(this IWindsorContainer container, FromAssemblyDescriptor assemblyDescriptor) =>
        container.Register(assemblyDescriptor
                           .BasedOn(typeof(IViewFor<>))
                           .Unless(t => t.GetCustomAttributes(typeof(DoNotRegisterAttribute), false).Any())
                           .WithService.FromInterface()
                           .LifestyleTransient());

    /// <summary>
    /// Finds all <see cref="IViewFor{T}"/> and <see cref="ViewModel"/> derived types and adds them to the provided container.
    /// Attributes are also available which can help you customize this behavior.
    /// </summary>
    /// <param name="container">Your DI container</param>
    /// <param name="assemblyDescriptor">Descriptor which tells us where to start looking.</param>
    public static void RegisterViewModelsAndViews(this IWindsorContainer container, FromAssemblyDescriptor assemblyDescriptor)
    {
        container.RegisterViewModels(assemblyDescriptor);
        container.RegisterViews(assemblyDescriptor);
    }

    /// <summary>
    /// Finds all <see cref="IViewFor{T}"/> and <see cref="ViewModel"/> derived types and adds them to the provided container.
    /// Attributes are also available which can help you customize this behavior.
    /// </summary>
    /// <param name="container">Your DI container</param>
    /// <param name="asmName">Name of the assembly to start from.</param>
    public static void RegisterViewModelsAndViews(this IWindsorContainer container, string asmName) =>
        container.RegisterViewModelsAndViews(Types.FromAssemblyNamed(asmName));

    /// <summary>
    /// Resolves a view from the container according to the provided view model type, and window type.
    /// </summary>
    /// <param name="container">Your DI container</param>
    /// <param name="viewModelType">Type of the view model that you're looking a view for</param>
    /// <param name="windowType">Type of the Window class that's used by your framework, different types are used by
    /// different frameworks (WPF, UWP, WinUI, Uno, Avalonia, etc.)</param>
    /// <param name="viewName">Name of the view in case this view model contains multiple views</param>
    /// <param name="window">Set to true, if you are looking for a Window type rather than Page or UserControl.</param>
    /// <returns>Instance of type <c>IViewFor</c></returns>
    /// <exception cref="ArgumentException">No view could be found or multiple views found.</exception>
    /// <exception cref="ViewResolveException">The view could not be resolved by Windsor (it was not registered).</exception>
    public static IViewFor GetView(
        this IWindsorContainer container,
        Type viewModelType,
        Type windowType,
        string viewName = null,
        bool window = false)
    {
        IHandler[] GetAppropriateHandlers(Type vmt)
        {
            var viewHandlersAttempt =
                container.Kernel
                         .GetAssignableHandlers(typeof(IViewFor<>).MakeGenericType(vmt))
                         .Where(h => windowType.IsAssignableFrom(h.ComponentModel.Implementation) ? window : !window)
                         .ToArray();

            // If no handler is found, try base type - multiple vms may use the same view if it's set to their base type. It'd be
            // nice if this also supported interfaces.
            if (viewHandlersAttempt.Length == 0 && vmt.BaseType is { } baseType)
                return GetAppropriateHandlers(baseType);

            return viewHandlersAttempt;
        }

        var viewHandlers = GetAppropriateHandlers(viewModelType);

        if (viewHandlers.Length == 0)
            throw new ArgumentException("No view found for specified view model.");

        if (viewHandlers.Length > 1)
            throw new ArgumentException("Multiple views found for specified view model, " +
                                        "this is currently not supported.");

        if (viewHandlers.First().Resolve(CreationContext.CreateEmpty()) is IViewFor view)
            return view;

        throw new ViewResolveException();
    }

    /// <summary>
    /// Resolves a view from the container according to the provided view model type, and window type.
    /// </summary>
    /// <typeparam name="TVm">Type of the view model that you're looking a view for</typeparam>
    /// <typeparam name="TWnd">Type of the Window class that's used by your framework, different types are used by
    /// different frameworks (WPF, UWP, WinUI, Uno, Avalonia, etc.)</typeparam>
    /// <param name="container">Your DI container</param>
    /// <param name="viewModel">Type of the view model that you're looking a view for</param>
    /// <param name="viewName">Name of the view in case this view model contains multiple views</param>
    /// <param name="window">Set to true, if you are looking for a Window type rather than Page or UserControl.</param>
    /// <returns>Instance of type <c>IViewFor</c></returns>
    /// <exception cref="ArgumentException">No view could be found or multiple views found.</exception>
    /// <exception cref="ViewResolveException">The view could not be resolved by Windsor (it was not registered).</exception>
    public static IViewFor GetView<TVm, TWnd>(
        this IWindsorContainer container,
        TVm viewModel,
        string viewName = null,
        bool window = false) =>
        container.GetView(typeof(TVm), typeof(TWnd), viewName, window);

    /// <summary>
    /// Resolves a view from the container according to the provided view model instance, and window type.
    /// </summary>
    /// <typeparam name="TWnd">Type of the Window class that's used by your framework, different types are used by
    /// different frameworks (WPF, UWP, WinUI, Uno, Avalonia, etc.)</typeparam>
    /// <param name="container">Your DI container</param>
    /// <param name="viewModel">Instance of the view model that you're looking a view for.</param>
    /// <param name="viewName">Name of the view in case this view model contains multiple views</param>
    /// <param name="window">Set to true, if you are looking for a Window type rather than Page or UserControl.</param>
    /// <returns>Instance of type <c>IViewFor</c></returns>
    /// <exception cref="ArgumentException">No view could be found or multiple views found.</exception>
    /// <exception cref="ViewResolveException">The view could not be resolved by Windsor (it was not registered).</exception>
    public static IViewFor GetView<TWnd>(
        this IWindsorContainer container,
        object viewModel,
        string viewName = null,
        bool window = false) =>
        container.GetView(viewModel.GetType(), typeof(TWnd), viewName, window);
}

/// <summary>
/// Thrown when <c>GetView</c> fails to resolve a view.
/// </summary>
public class ViewResolveException : Exception
{
    internal ViewResolveException()
        : base("Failed to resolve view dependency.") { }
}

/// <summary>
/// Instructs the <see cref="CastleExtensions"/> class to not register the given type.
/// </summary>
public class DoNotRegisterAttribute : Attribute { }

/// <summary>
/// Instructs the <see cref="CastleExtensions"/> class to register the given type as a singleton.
/// </summary>
public class SingletonAttribute : Attribute { }