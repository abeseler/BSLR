using System.Runtime.CompilerServices;

namespace Beseler.Infrastructure.Data.Mappers;

internal static class AccountMapper
{
    public static void SetIdentity(this Account account, int id) => SetAccountId(account, id);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = $"set_{nameof(Account.AccountId)}")]
    static extern void SetAccountId(Account @this, int value);
}
