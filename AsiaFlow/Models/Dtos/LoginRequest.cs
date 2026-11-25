namespace AsiaFlow.Models.Dtos;

/// <summary>
/// Request DTO for user login.
/// </summary>
public sealed record LoginRequest(
    /// <summary>
    /// The user's username or email address.
    /// </summary>
    string Username,

    /// <summary>
    /// The user's password.
    /// </summary>
    string Password
);
