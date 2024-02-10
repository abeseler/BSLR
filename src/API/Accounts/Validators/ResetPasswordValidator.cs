using Beseler.Shared.Accounts.Requests;
using FluentValidation;

namespace Beseler.API.Accounts.Validators;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MinimumLength(1);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
