using System.Runtime.CompilerServices;

namespace Beseler.Infrastructure.Data.Mappers;

internal static class BudgetMapper
{
    public static void SetLines(this Budget model, IEnumerable<BudgetLine> lines) =>
        GetLines(model) = lines.ToList();

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_lines")]
    static extern ref List<BudgetLine> GetLines(Budget @this);
}
