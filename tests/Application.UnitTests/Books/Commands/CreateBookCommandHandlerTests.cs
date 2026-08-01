using Application.Books.Commands.CreateBook;
using Application.Common.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.UnitTests.Books.Commands;

public class CreateBookCommandHandlerTests
{
    private readonly Mock<IRepository<Book>> _bookRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_ValidCommand_AddsBookWithCorrectCopyCountAndSaves()
    {
        var command = new CreateBookCommand("Clean Code", "Robert C. Martin", "9780132350884", Guid.NewGuid(), InitialCopies: 3);
        var handler = new CreateBookCommandHandler(_bookRepositoryMock.Object, _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();

        _bookRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Book>(b => b.Title == "Clean Code" && b.Copies.Count == 3),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ZeroInitialCopies_CreatesBookWithNoCopies()
    {
        var command = new CreateBookCommand("Some Book", "Some Author", "1234567890", Guid.NewGuid(), InitialCopies: 0);
        var handler = new CreateBookCommandHandler(_bookRepositoryMock.Object, _unitOfWorkMock.Object);

        await handler.Handle(command, CancellationToken.None);

        _bookRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Book>(b => b.Copies.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
