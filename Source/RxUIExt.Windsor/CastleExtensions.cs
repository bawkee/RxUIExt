namespace RxUIExt.Windsor;

using System;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Context;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ReactiveUI;

public static class CastleExtensions
{
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

    public static void RegisterViews(this IWindsorContainer container, FromAssemblyDescriptor assemblyDescriptor) =>
        container.Register(assemblyDescriptor
                           .BasedOn(typeof(IViewFor<>))
                           .Unless(t => t.GetCustomAttributes(typeof(DoNotRegisterAttribute), false).Any())
                           .WithService.FromInterface()
                           .LifestyleTransient());

    public static void RegisterViewModelsAndViews(this IWindsorContainer container, string asmName) =>
        container.RegisterViewModelsAndViews(Types.FromAssemblyNamed(asmName));

    public static void RegisterViewModelsAndViews(this IWindsorContainer container, FromAssemblyDescriptor assemblyDescriptor)
    {
        container.RegisterViewModels(assemblyDescriptor);
        container.RegisterViews(assemblyDescriptor);
    }

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

    public static IViewFor GetView<TVm, TWnd>(
        this IWindsorContainer container,
        TVm viewModel,
        string viewName = null,
        bool window = false) =>
        container.GetView(typeof(TVm), typeof(TWnd), viewName, window);

    public static IViewFor GetView<TWnd>(
        this IWindsorContainer container,
        object viewModel,
        string viewName = null,
        bool window = false) =>
        container.GetView(viewModel.GetType(), typeof(TWnd), viewName, window);
}

public class ViewResolveException : Exception
{
    public ViewResolveException()
        : base("Failed to resolve view dependency.") { }
}

public class DoNotRegisterAttribute : Attribute { }

public class SingletonAttribute : Attribute { }