using Application.Auth.Dtos;
using Application.Auth.Specifications;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IRepository<AppUser> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IRepository<AppUser> userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var spec = new AppUserByEmailSpecification(request.Email);
        var user = (await _userRepository.ListAsync(spec, cancellationToken)).SingleOrDefault();

        // Deliberately the same error message whether the email doesn't exist or the
        // password is wrong - never reveal which one it was, that leaks valid emails.
        if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new AuthenticationException("Invalid email or password.");

        var tokenResult = _jwtTokenService.GenerateToken(user);

        return new AuthResultDto(tokenResult.Token, tokenResult.ExpiresAt, user.FullName, user.Email, user.Role.ToString());
    }
}
