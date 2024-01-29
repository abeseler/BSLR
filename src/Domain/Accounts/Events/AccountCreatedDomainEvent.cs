using Beseler.Domain.Common;

namespace Beseler.Domain.Accounts.Events;

internal sealed record AccountCreatedDomainEvent(string Email) : DomainEvent;
