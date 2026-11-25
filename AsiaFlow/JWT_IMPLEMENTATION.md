# JWT Login Endpoint Implementation

This document describes the JWT authentication implementation for the AsiaFlow API.

## Overview

A complete JWT (JSON Web Token) authentication system has been implemented that allows users to securely log in and receive a token for authenticated requests.

## Files Created

### Controllers
- **`Controllers/AuthController.cs`** - Contains the login endpoint `/api/auth/login`

### Services
- **`Services/JwtTokenService.cs`** - Handles JWT token generation and validation
- **`Services/AuthenticationService.cs`** - Handles user authentication logic

### Models
- **`Models/Dtos/LoginRequest.cs`** - Request DTO with username and password
- **`Models/Dtos/LoginResponse.cs`** - Response DTO with JWT token information

### Testing
- **`Auth.http`** - HTTP test file with sample login requests

## Configuration

JWT settings are configured in `appsettings.json`:

```json
"JwtSettings": {
  "SecretKey": "your-super-secret-key-change-in-production-minimum-32-characters-required",
  "Issuer": "AsiaFlow",
  "Audience": "AsiaFlowClient",
  "ExpiryMinutes": 60
}
```

### Important Security Notes

1. **Secret Key**: Change the default secret key in production to a strong, random value
2. **HTTPS Only**: Always use HTTPS in production to protect tokens in transit
3. **Token Expiry**: Adjust `ExpiryMinutes` based on your security requirements
4. **Authentication Logic**: Update `AuthenticationService.AuthenticateUser()` to validate against your actual user database

## API Endpoints

### POST /api/auth/login

Authenticates a user and returns a JWT token.

**Request Body:**
```json
{
  "username": "testuser",
  "password": "testpassword"
}
```

**Success Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "username": "testuser"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid request format
- `422 Unprocessable Entity` - Missing required fields
- `401 Unauthorized` - Invalid credentials

## Using the JWT Token

After login, include the token in subsequent requests:

```
Authorization: Bearer <token>
```

## Protected Endpoints

To protect an endpoint with JWT authentication, add the `[Authorize]` attribute:

```csharp
[Authorize]
[HttpGet("protected-resource")]
public IActionResult GetProtectedResource()
{
	return Ok("This is a protected resource");
}
```

## Testing

Use the provided `Auth.http` file in Visual Studio to test:

1. Open `Auth.http`
2. Click "Send Request" on any test case
3. View the response in the HTTP response window

Example test cases are included for:
- Successful login
- Missing password
- Empty credentials

## Integration with User Database

The `AuthenticationService.AuthenticateUser()` method is currently a placeholder. To integrate with your user database:

```csharp
public bool AuthenticateUser(string username, string password)
{
	var user = _userRepository.GetByUsername(username);
	if (user == null)
		return false;

	return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
}
```

## Next Steps

1. Replace the placeholder authentication logic with real database validation
2. Implement password hashing (e.g., BCrypt) for password storage
3. Generate a strong secret key for production
4. Consider implementing refresh tokens for enhanced security
5. Add rate limiting to the login endpoint to prevent brute force attacks
6. Add logging for authentication events
