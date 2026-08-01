using Application.Borrowing.Commands.BorrowBook;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Application.UnitTests.Borrowing.Commands;

public class BorrowBookCommandHandlerTests
{
    private readonly Mock<IRepository<BookCopy>> _bookCopyRepositoryMock = new();
    private readonly Mock<IRepository<Member>> _memberRepositoryMock = new();
    private readonly Mock<IRepository<Loan>> _loanRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();

    private BorrowBookCommandHandler CreateHandler() => new(
        _bookCopyRepositoryMock.Object,
        _memberRepositoryMock.Object,
        _loanRepositoryMock.Object,
        _unitOfWorkMock.Object,
        _dateTimeProviderMock.Object,
        _cacheMock.Object);

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsNotFoundException()
    {
        _memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Member?)null);

        var handler = CreateHandler();
        var command = new BorrowBookCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_MemberAtLoanLimit_ThrowsBusinessRuleException()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        for (var i = 0; i < 5; i++) member.IncrementActiveLoans(); // hits the limit

        _memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var handler = CreateHandler();
        var command = new BorrowBookCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();

        // Should never even look up the copy if the member is ineligible - fail fast.
        _bookCopyRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BookCopyNotFound_ThrowsNotFoundException()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        _memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookCopy?)null);

        var handler = CreateHandler();
        var command = new BorrowBookCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_BorrowsCopyCreatesLoanAndIncrementsMemberLoanCount()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        var bookCopy = BookCopy.Create(Guid.NewGuid(), Guid.NewGuid());
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        _memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);
        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(now);

        var handler = CreateHandler();
        var command = new BorrowBookCommand(bookCopy.Id, member.Id);

        var loanId = await handler.Handle(command, CancellationToken.None);

        loanId.Should().NotBeEmpty();
        bookCopy.Status.Should().Be(CopyStatus.Borrowed);
        member.ActiveLoanCount.Should().Be(1);

        _loanRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
