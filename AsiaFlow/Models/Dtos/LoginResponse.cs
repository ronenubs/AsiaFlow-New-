namespace AsiaFlow.Models.Dtos;

/// <summary>
/// Response DTO for successful user login containing the JWT token.
/// </summary>
public sealed record LoginResponse(
    /// <summary>
    /// The JWT access token to be used for authenticating subsequent requests.
    /// </summary>
    string Token,

    /// <summary>
    /// The type of token (typically "Bearer").
    /// </summary>
    string TokenType,

    /// <summary>
    /// The number of seconds until the token expires.
    /// </summary>
    int ExpiresIn,

    /// <summary>
    /// The username of the authenticated user.
    /// </summary>
    string Username
);
