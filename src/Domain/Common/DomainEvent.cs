using Beseler.Domain.Accounts;
using System.Text.Json.Serialization;

namespace Beseler.Domain.Common;

[JsonDerivedType(typeof(AccountCreatedDomainEvent))]
[JsonDerivedType(typeof(AccountLockedDomainEvent))]
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
