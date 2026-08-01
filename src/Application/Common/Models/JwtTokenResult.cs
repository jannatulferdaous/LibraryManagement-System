namespace Application.Common.Models;

public record JwtTokenResult(string Token, DateTime ExpiresAt);
