using Application.Common.Specifications;
using Domain.Entities;

namespace Application.Auth.Specifications;

public class AppUserByEmailSpecification : BaseSpecification<AppUser>
{
    public AppUserByEmailSpecification(string email)
    {
        AddCriteria(u => u.Email == email);
    }
}
