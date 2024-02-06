using Beseler.Domain.Common;

namespace Beseler.Domain.Accounts.Events;

public sealed record AccountLockedDomainEvent(string Email, string Reason) : DomainEvent;
