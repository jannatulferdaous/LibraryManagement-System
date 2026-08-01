using Application.Books.Commands.CreateBook;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.UnitTests.Books.Commands;

public class CreateBookCommandValidatorTests
{
    private readonly CreateBookCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyTitle_HasValidationErrorForTitle()
    {
        var command = new CreateBookCommand("", "Author", "9780132350884", Guid.NewGuid(), 1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("9780132350884")] // valid ISBN-13
    [InlineData("0132350882")]    // valid ISBN-10 (last digit variant)
    public void Validate_ValidIsbnFormats_HasNoValidationErrorForIsbn(string isbn)
    {
        var command = new CreateBookCommand("Title", "Author", isbn, Guid.NewGuid(), 1);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Isbn);
    }

    [Fact]
    public void Validate_MalformedIsbn_HasValidationErrorForIsbn()
    {
        var command = new CreateBookCommand("Title", "Author", "not-an-isbn", Guid.NewGuid(), 1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Isbn);
    }

    [Fact]
    public void Validate_NegativeInitialCopies_HasValidationErrorForInitialCopies()
    {
        var command = new CreateBookCommand("Title", "Author", "9780132350884", Guid.NewGuid(), -1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InitialCopies);
    }

    [Fact]
    public void Validate_EmptyBranchId_HasValidationErrorForBranchId()
    {
        var command = new CreateBookCommand("Title", "Author", "9780132350884", Guid.Empty, 1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BranchId);
    }

    [Fact]
    public void Validate_AllFieldsValid_HasNoValidationErrors()
    {
        var command = new CreateBookCommand("Clean Code", "Robert C. Martin", "9780132350884", Guid.NewGuid(), 3);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
