using Beseler.Domain.Common;

namespace Beseler.Domain.Accounts.Events;

public sealed record AccountCreatedDomainEvent(string Email) : DomainEvent;
