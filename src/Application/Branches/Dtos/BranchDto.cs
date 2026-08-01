namespace Application.Branches.Dtos;

public class BranchDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string Phone { get; init; } = default!;
}
