using FluentValidation;

namespace Application.Branches.Commands.CreateBranch;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^[\d\+\-\(\)\s]{7,20}$")
            .WithMessage("Phone must be 7-20 characters, digits and +()- allowed.");
    }
}
