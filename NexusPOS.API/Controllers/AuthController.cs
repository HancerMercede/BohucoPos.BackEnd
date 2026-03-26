using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;

namespace NexusPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUnitOfWork unitOfWork, IJwtService jwtService, IPasswordHasher passwordHasher) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var existing = await unitOfWork.Users.GetByUsernameAsync(request.Username, ct);
        if (existing is not null)
            return BadRequest(new ErrorResponse("Username already exists"));

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Role = request.Role ?? "Waiter"
        };

        await unitOfWork.Users.AddAsync(user, ct);
        await unitOfWork.CommitAsync(ct);

        var token = jwtService.GenerateToken(user.Username, user.Role);

        return Ok(new LoginResponse(token, user.Username, user.Role, user.FullName));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await unitOfWork.Users.GetByUsernameAsync(request.Username, ct);
        
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new ErrorResponse("Invalid credentials"));

        if (!user.IsActive)
            return Unauthorized(new ErrorResponse("User is inactive"));

        if (!user.PasswordHash.StartsWith("$2"))
        {
            user.PasswordHash = passwordHasher.Hash(request.Password);
            await unitOfWork.Users.UpdateAsync(user, ct);
        }

        var token = jwtService.GenerateToken(user.Username, user.Role);

        return Ok(new LoginResponse(token, user.Username, user.Role, user.FullName));
    }
}
