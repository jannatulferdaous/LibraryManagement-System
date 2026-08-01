using Domain.Common;

namespace Domain.Entities;

public class Branch : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public string Phone { get; private set; } = default!;

    private Branch() { } // EF Core

    public static Branch Create(string name, string address, string phone)
        => new() { Name = name, Address = address, Phone = phone };

    public void UpdateDetails(string name, string address, string phone)
    {
        Name = name;
        Address = address;
        Phone = phone;
    }
}
