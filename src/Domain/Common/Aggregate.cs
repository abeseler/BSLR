namespace Beseler.Domain.Common;

public abstract class Aggregate
{
    private List<DomainEvent>? _domainEvents;

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents ??= [];
    public bool HasUnsavedChanges { get; private set; }

    public void SavedChanges()
    {
        _domainEvents?.Clear();
        HasUnsavedChanges = false;
    }

    protected void HasUnsavedChange() => HasUnsavedChanges = true;
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents ??= [];
        _domainEvents.Add(domainEvent);
    }
}
