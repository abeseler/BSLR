namespace Beseler.Infrastructure.Data;

public sealed class ConcurrencyException(string Type) : Exception($"{Type} cannot be saved because it's value has changed.");
