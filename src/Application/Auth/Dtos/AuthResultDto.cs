namespace Application.Auth.Dtos;

public record AuthResultDto(string Token, DateTime ExpiresAt, string FullName, string Email, string Role);
