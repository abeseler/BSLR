﻿using Beseler.Domain.Accounts.Events;
using Beseler.Domain.Common;

namespace Beseler.Domain.Accounts;

public sealed class Account : Aggregate
{
    private Account() { }

    public int AccountId { get; private set; }
    public string Email { get; private set; } = default!;
    public string? GivenName { get; private set; }
    public string? FamilyName { get; private set; }
    public string? SecretHash { get; private set; }
    public bool IsLocked { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public DateTime? LastLoginOn { get; private set; }
    public int FailedLoginAttempts { get; private set; }

    public static Account Create(string email, string givenName, string familyName, string secretHash)
    {
        var account =
            new Account()
            {
                Email = email.Trim(),
                GivenName = givenName.Trim(),
                FamilyName = familyName.Trim(),
                SecretHash = secretHash,
                CreatedOn = DateTime.UtcNow
            };

        account.AddDomainEvent(new AccountCreatedDomainEvent(account.Email));

        return account;
    }

    public void Verify()
    {
        LastLoginOn = DateTime.UtcNow;
        IsVerified = true;
    }

    public void Login()
    {
        LastLoginOn = DateTime.UtcNow;
        FailedLoginAttempts = 0;
    }

    public void FailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts < 5)
            return;

        IsLocked = true;
        AddDomainEvent(new AccountLockedDomainEvent(Email, "Too many failed login attempts."));
    }
}
