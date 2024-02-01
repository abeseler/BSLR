namespace Beseler.Infrastructure.Data.Models;

public sealed record OutboxMessage
{
    public int OutboxMessageId { get; init; }
    public Guid ServiceId { get; init; }
    public required string MessageType { get; init; }
    public required string Payload { get; init; }
    public DateTime CreatedOn { get; init; }
    public int RetriesRemaining { get; init; }
}
