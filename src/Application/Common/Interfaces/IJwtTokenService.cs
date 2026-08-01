using Application.Common.Models;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(AppUser user);
}
