namespace NexusPOS.Application.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string Username, string Role, string FullName);
public record RegisterRequest(string Username, string Password, string FullName, string? Role);
