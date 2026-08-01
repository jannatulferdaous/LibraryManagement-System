using FluentValidation;

namespace Application.Books.Commands.CreateBook;

public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Isbn)
            .NotEmpty();
            //.Matches(@"^(97(8|9))?\d{9}(\d|X)$")
            //.WithMessage("Isbn must be a valid ISBN-10 or ISBN-13.");
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.InitialCopies).GreaterThanOrEqualTo(0);
    }
}
