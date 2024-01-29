using Beseler.Domain.Accounts.Events;
using System.Text.Json.Serialization;

namespace Beseler.Domain.Common;

[JsonDerivedType(typeof(AccountCreatedDomainEvent))]
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
