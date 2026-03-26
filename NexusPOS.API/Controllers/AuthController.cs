using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;

namespace NexusPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUnitOfWork unitOfWork, IJwtService jwtService, IPasswordHasher passwordHasher) : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var existing = await _unitOfWork.Users.GetByUsernameAsync(request.Username, ct);
        if (existing is not null)
            return BadRequest(new ErrorResponse("Username already exists"));

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Role = request.Role ?? "Waiter"
        };

        await _unitOfWork.Users.AddAsync(user, ct);
        await _unitOfWork.CommitAsync(ct);

        var token = _jwtService.GenerateToken(user.Username, user.Role);

        return Ok(new LoginResponse(token, user.Username, user.Role, user.FullName));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username, ct);
        
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new ErrorResponse("Invalid credentials"));

        if (!user.IsActive)
            return Unauthorized(new ErrorResponse("User is inactive"));

        var token = _jwtService.GenerateToken(user.Username, user.Role);

        return Ok(new LoginResponse(token, user.Username, user.Role, user.FullName));
    }
}
