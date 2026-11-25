using AsiaFlow.Models.Dtos;
using AsiaFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsiaFlow.Controllers;

/// <summary>
/// Controller for handling authentication operations including login.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    JwtTokenService jwtTokenService,
    AuthenticationService authenticationService) : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService = jwtTokenService;
    private readonly AuthenticationService _authenticationService = authenticationService;

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <returns>A JWT token if authentication is successful.</returns>
    /// <response code="200">Login successful, JWT token returned.</response>
    /// <response code="400">Invalid username or password.</response>
    /// <response code="422">Invalid request data.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Login request cannot be null." });
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return UnprocessableEntity(new { message = "Username and password are required." });
        }

        if (!_authenticationService.AuthenticateUser(request.Username, request.Password))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var token = _jwtTokenService.GenerateToken(request.Username);
        var expirySeconds = _jwtTokenService.GetTokenExpirySeconds();

        var response = new LoginResponse(
            Token: token,
            TokenType: "Bearer",
            ExpiresIn: expirySeconds,
            Username: request.Username
        );

        return Ok(response);
    }
}
