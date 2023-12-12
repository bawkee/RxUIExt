namespace RxUIExt;

using ReactiveUI;

public class ViewModel : ReactiveObject
{
    protected ViewModelChangeTracker TrackChanges(params string[] properties) => new(this, properties);
}