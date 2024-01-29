namespace Beseler.Domain.Common;

public abstract class Aggregate
{
    private List<DomainEvent>? _domainEvents;

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents ??= [];

    protected void Raise(DomainEvent domainEvent)
    {
        _domainEvents ??= [];
        _domainEvents.Add(domainEvent);
    }

    public void ClearEvents() => _domainEvents?.Clear();
}
