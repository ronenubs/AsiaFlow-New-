# JWT Implementation - Practical Examples

## Example 1: Basic Login Request

**Request:**
```http
POST https://localhost:7123/api/auth/login
Content-Type: application/json

{
  "username": "john.doe",
  "password": "securepassword123"
}
```

**Successful Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqb2huLmRvZSIsIm5hbWUiOiJqb2huLmRvZSIsImlhdCI6MTcwNDEwMDAwMH0.abc123...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "username": "john.doe"
}
```

## Example 2: Access Protected Resource

**Request with Token:**
```http
GET https://localhost:7123/api/example/protected
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqb2huLmRvZSIsIm5hbWUiOiJqb2huLmRvZSIsImlhdCI6MTcwNDEwMDAwMH0.abc123...
```

**Response:**
```json
{
  "message": "Hello john.doe, this is a protected resource"
}
```

## Example 3: Decode JWT Token

Use https://jwt.io to decode and inspect your token:

```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "john.doe",
  "name": "john.doe",
  "iat": 1704100000,
  "exp": 1704103600,
  "iss": "AsiaFlow",
  "aud": "AsiaFlowClient"
}

Signature:
HMACSHA256(base64UrlEncode(header) + "." + base64UrlEncode(payload), secret_key)
```

## Example 4: Error Responses

### Invalid Credentials
```http
POST https://localhost:7123/api/auth/login
Content-Type: application/json

{
  "username": "invalid",
  "password": "wrong"
}
```

Response (401 Unauthorized):
```json
{
  "message": "Invalid username or password."
}
```

### Missing Fields
```http
POST https://localhost:7123/api/auth/login
Content-Type: application/json

{
  "username": "john.doe"
}
```

Response (422 Unprocessable Entity):
```json
{
  "message": "Username and password are required."
}
```

### Expired Token
```http
GET https://localhost:7123/api/example/protected
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqb2huLmRvZSIsImV4cCI6MTcwNDA5OTk5OX0.expired...
```

Response (401 Unauthorized):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Unauthorized",
  "status": 401
}
```

## Example 5: Protected Controller Method

```csharp
[Authorize]
[HttpGet("user-profile")]
public IActionResult GetUserProfile()
{
	var username = User.Identity?.Name;
	var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

	return Ok(new
	{
		username,
		userId,
		roles = User.FindAll(ClaimTypes.Role)
	});
}
```

## Example 6: Role-Based Authorization

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("users/{id}")]
public IActionResult DeleteUser(int id)
{
	// Only admins can delete users
	return Ok(new { message = $"User {id} deleted" });
}
```

## Example 7: Claims-Based Authorization

```csharp
[Authorize]
[HttpGet("my-data")]
public IActionResult GetMyData()
{
	var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

	if (string.IsNullOrEmpty(userId))
		return Unauthorized();

	return Ok(new { userId, data = "user-specific-data" });
}
```

## Example 8: Testing Multiple Endpoints

```http
### 1. Login to get token
@baseUrl = https://localhost:7123
@token = 

POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "testpass123"
}

### 2. Save token from response above and test protected endpoint
GET {{baseUrl}}/api/example/protected
Authorization: Bearer {{token}}

### 3. Test role-based endpoint
GET {{baseUrl}}/api/admin/dashboard
Authorization: Bearer {{token}}

### 4. Test public endpoint (no auth needed)
GET {{baseUrl}}/api/example/public
```

## Curl Examples

### Login
```bash
curl -X POST https://localhost:7123/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"john.doe","password":"password123"}'
```

### Access Protected Resource
```bash
curl -X GET https://localhost:7123/api/example/protected \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## JavaScript/Fetch Examples

### Login
```javascript
async function login(username, password) {
  const response = await fetch('https://localhost:7123/api/auth/login', {
	method: 'POST',
	headers: {
	  'Content-Type': 'application/json'
	},
	body: JSON.stringify({ username, password })
  });

  const data = await response.json();
  localStorage.setItem('token', data.token);
  return data;
}
```

### Access Protected Resource
```javascript
async function getProtectedData() {
  const token = localStorage.getItem('token');

  const response = await fetch('https://localhost:7123/api/example/protected', {
	headers: {
	  'Authorization': `Bearer ${token}`
	}
  });

  return response.json();
}
```

## Real-World Integration Checklist

- [ ] Replace placeholder authentication with database lookup
- [ ] Implement password hashing (BCrypt, Argon2)
- [ ] Add refresh token support
- [ ] Implement claims/roles for authorization
- [ ] Add rate limiting to login endpoint
- [ ] Configure CORS for frontend clients
- [ ] Add logging for audit trail
- [ ] Set secure JWT settings for production
- [ ] Use environment variables for secrets
- [ ] Add two-factor authentication (optional)
- [ ] Implement token revocation/blacklisting
- [ ] Add API versioning with auth
