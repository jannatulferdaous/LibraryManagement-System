using Application.Branches.Dtos;
using Domain.Entities;

namespace Application.Branches.Mappings;

public static class BranchMappingExtensions
{
    public static BranchDto ToDto(this Branch branch) => new()
    {
        Id = branch.Id,
        Name = branch.Name,
        Address = branch.Address,
        Phone = branch.Phone
    };
}
