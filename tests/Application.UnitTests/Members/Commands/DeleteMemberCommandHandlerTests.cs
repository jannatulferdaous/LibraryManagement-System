using Application.Common.Interfaces;
using Application.Members.Commands.DeleteMember;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.UnitTests.Members.Commands;

public class DeleteMemberCommandHandlerTests
{
    private readonly Mock<IRepository<Member>> _memberRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private DeleteMemberCommandHandler CreateHandler()
        => new(_memberRepositoryMock.Object, _unitOfWorkMock.Object);

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsNotFoundException()
    {
        _memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Member?)null);

        var act = async () => await CreateHandler().Handle(new DeleteMemberCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_MemberHasActiveLoans_ThrowsBusinessRuleException()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        member.IncrementActiveLoans();

        _memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var act = async () => await CreateHandler().Handle(new DeleteMemberCommand(member.Id), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleException>())
            .WithMessage("*active loans*");

        _memberRepositoryMock.Verify(r => r.Remove(It.IsAny<Member>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MemberHasOutstandingFines_ThrowsBusinessRuleException()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        member.AddFine(10m);

        _memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var act = async () => await CreateHandler().Handle(new DeleteMemberCommand(member.Id), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleException>())
            .WithMessage("*outstanding fines*");
    }

    [Fact]
    public async Task Handle_EligibleMember_RemovesAndSaves()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);

        _memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        await CreateHandler().Handle(new DeleteMemberCommand(member.Id), CancellationToken.None);

        _memberRepositoryMock.Verify(r => r.Remove(member), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
