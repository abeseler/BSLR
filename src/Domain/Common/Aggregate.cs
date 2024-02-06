namespace Beseler.Domain.Common;

public abstract class Aggregate
{
    private List<DomainEvent>? _domainEvents;

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents ??= [];

    protected void AddDomainEvent<T>(T domainEvent) where T : DomainEvent
    {
        _domainEvents ??= [];
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents?.Clear();
}
