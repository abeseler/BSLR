using Beseler.Domain.Common;

namespace Beseler.Domain.Accounts;

public sealed record AccountCreatedDomainEvent(string Email) : DomainEvent;
public sealed record AccountLockedDomainEvent(string Email, string Reason) : DomainEvent;
