namespace RxUIExt;

using ReactiveUI;

/// <inheritdoc />
public class ViewModel : ReactiveObject
{
    /// <summary>
    /// Tracks all the properties which fire change notifications in order to determine whether a property is dirty or not.
    /// </summary>
    protected ViewModelChangeTracker TrackChanges(params string[] properties) => new(this, properties);
}