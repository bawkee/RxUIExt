namespace RxUIExt;

using ReactiveUI;

/// <summary>
/// View model that supports activation (IActivatableViewModel) and exposes the Activator object.
/// </summary>
public abstract class ActivatableViewModel : ViewModel, IActivatableViewModel
{
    /// <summary>
    /// <para>Activator helper object for activatable view models that you can use to 
    /// subscribe to activations.</para>
    /// <para>
    /// ViewModelActivator is a helper class that you instantiate in your
    /// ViewModel classes in order to help with Activation. Views will internally
    /// call this class when the corresponding View comes on screen. This means
    /// you can set up resources such as subscriptions to global objects that
    /// should be cleaned up on exit. Once you instantiate this class, use the
    /// WhenActivated method to register what to do when activated.
    /// </para>
    /// </summary>
    /// <remarks>
    /// More documentation here: https://reactiveui.net/docs/handbook/when-activated/
    /// </remarks>
    public ViewModelActivator Activator { get; } = new();
}