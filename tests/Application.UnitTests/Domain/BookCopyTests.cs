using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;
using Domain.Events;

namespace Application.UnitTests.Domain;

public class BookCopyTests
{
    [Fact]
    public void Borrow_WhenAvailable_SetsStatusToBorrowed()
    {
        var copy = BookCopy.Create(Guid.NewGuid(), Guid.NewGuid());

        copy.Borrow();

        copy.Status.Should().Be(CopyStatus.Borrowed);
    }

    [Fact]
    public void Borrow_WhenAlreadyBorrowed_ThrowsBusinessRuleException()
    {
        var copy = BookCopy.Create(Guid.NewGuid(), Guid.NewGuid());
        copy.Borrow();

        var act = () => copy.Borrow();

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Return_WhenBorrowed_SetsStatusToAvailable()
    {
        var copy = BookCopy.Create(Guid.NewGuid(), Guid.NewGuid());
        copy.Borrow();

        copy.Return();

        copy.Status.Should().Be(CopyStatus.Available);
    }

    [Fact]
    public void Return_WhenNotBorrowed_ThrowsBusinessRuleException()
    {
        var copy = BookCopy.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => copy.Return();

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Return_WhenBorrowed_RaisesBookReturnedDomainEvent()
    {
        var copy = BookCopy.Create(Guid.NewGuid(), Guid.NewGuid());
        copy.Borrow();

        copy.Return();

        copy.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BookReturnedEvent>();
    }
}
