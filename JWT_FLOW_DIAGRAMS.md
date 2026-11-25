# JWT Authentication Flow Diagrams

## Sequence Diagram: Login and Access Protected Resource

```
Client                          API Server                 Database
  |                               |                            |
  |--1. POST /api/auth/login----->|                            |
  |   {username, password}        |                            |
  |                               |--2. Validate Credentials-->|
  |                               |<--3. User data-------------|
  |                               |
  |                        4. Generate JWT Token
  |                        (Sign with secret key)
  |                               |
  |<--5. Login Response-----------|
  |    {token, expiresIn, ...}    |
  |                               |
  |--6. GET /api/example/protected|
  |    Authorization: Bearer token|
  |                               |
  |                        7. Validate Token
  |                           - Check signature
  |                           - Check expiry
  |                           - Verify claims
  |                               |
  |<--8. Success Response---------|
  |    {data}                     |
```

## Authentication Middleware Pipeline

```
HTTP Request
	↓
[UseHttpsRedirection]
	↓
[UseAuthentication] ← Validates Bearer token
	├─ Extract token from Authorization header
	├─ Verify signature using secret key
	├─ Check token expiry
	├─ Extract claims (username, roles, etc.)
	└─ Create ClaimsPrincipal
	↓
[UseAuthorization] ← Checks [Authorize] attribute
	├─ Verify user is authenticated
	├─ Check roles/policies if specified
	└─ Allow or deny access
	↓
[MapControllers] → Route to endpoint
	↓
Action Method (Controller)
	├─ Access User.Identity.Name
	├─ Access User.Claims
	├─ Access protected data
	└─ Return response
	↓
HTTP Response (200 OK / 401 Unauthorized / 403 Forbidden)
```

## Token Lifecycle

```
1. GENERATION
   ┌─────────────────────────────────────┐
   │ User submits credentials            │
   │ POST /api/auth/login                │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ Server validates credentials        │
   │ - Check username in database        │
   │ - Hash compare password             │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ Create JWT Claims                   │
   │ - sub: username                     │
   │ - exp: now + 60 minutes             │
   │ - iat: issued at time               │
   │ - iss: issuer (AsiaFlow)            │
   │ - aud: audience (AsiaFlowClient)    │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ Sign with secret key                │
   │ Algorithm: HS256                    │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ Return JWT to client               │
   │ "eyJhbGciOiJIUzI1NiI..."           │
   └─────────────────────────────────────┘

2. TRANSMISSION
   ┌─────────────────────────────────────┐
   │ Client stores token                 │
   │ (localStorage, memory, cookie)      │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ Client includes in requests         │
   │ Authorization: Bearer {token}       │
   └─────────────────────────────────────┘

3. VALIDATION
   ┌─────────────────────────────────────┐
   │ Server receives request             │
   │ Extract token from header           │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ Validate signature                  │
   │ - Decrypt with secret key           │
   │ - Compare header.payload.signature  │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ Check expiry                        │
   │ - Current time < exp claim          │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ Verify claims                       │
   │ - Issuer matches                    │
   │ - Audience matches                  │
   │ - User exists                       │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ Create ClaimsPrincipal              │
   │ Attach to HttpContext.User          │
   └─────────────────────────────────────┘

4. AUTHORIZATION
   ┌─────────────────────────────────────┐
   │ Check [Authorize] attribute         │
   │ Verify user is authenticated        │
   │ Check roles/policies if required    │
   └─────────────────────────────────────┘
					↓
   ┌─────────────────────────────────────┐
   │ GRANTED: Allow access               │
   │ DENIED: Return 401/403              │
   └─────────────────────────────────────┘

5. EXPIRY
   ┌─────────────────────────────────────┐
   │ Token valid: 60 minutes             │
   │ After expiration, must login again  │
   │ (Or use refresh token if impl.)     │
   └─────────────────────────────────────┘
```

## Token Structure (JWT)

```
Header.Payload.Signature
   ↓       ↓         ↓
   |       |         └─ HMACSHA256(base64(header.payload), secret)
   |       |
   |       └─ Contains claims (data):
   |          {
   |            "sub": "john.doe",
   |            "name": "john.doe",
   |            "exp": 1704103600,
   |            "iat": 1704100000,
   |            "iss": "AsiaFlow",
   |            "aud": "AsiaFlowClient"
   |          }
   |
   └─ Contains metadata:
	  {
		"alg": "HS256",
		"typ": "JWT"
	  }

Full encoded token:
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiJqb2huLmRvZSIsIm5hbWUiOiJqb2huLmRvZSIsImV4cCI6MTcwNDEwMzYwMH0.
abc123DEF456...
```

## Error Flow Diagram

```
Request with invalid/missing token
			↓
		[UseAuthentication]
			├─ No Authorization header?
			│  └─→ AuthenticationScheme = null
			│
			├─ Invalid token format?
			│  └─→ Return 401 Unauthorized
			│
			├─ Signature invalid?
			│  └─→ Return 401 Unauthorized
			│
			└─ Token expired?
			   └─→ Return 401 Unauthorized
			↓
		[Endpoint with [Authorize]]
			│
			└─→ Check if User.Identity.IsAuthenticated
				├─ False → 401 Unauthorized
				└─ True → Check roles/policies
					├─ Failed → 403 Forbidden
					└─ Passed → Execute action
```

## Database Integration Flow

```
1. User Registration
   ┌──────────────────┐
   │ User submits     │
   │ username         │
   │ password         │
   └────────┬─────────┘
			↓
   ┌──────────────────────┐
   │ Hash password        │
   │ BCrypt.HashPassword()│
   └────────┬─────────────┘
			↓
   ┌──────────────────────┐
   │ Store in database    │
   │ username: john.doe   │
   │ hash: $2b$10$...     │
   └──────────────────────┘

2. User Login
   ┌──────────────────┐
   │ User submits     │
   │ username         │
   │ password         │
   └────────┬─────────┘
			↓
   ┌──────────────────────┐
   │ Query database       │
   │ by username          │
   └────────┬─────────────┘
			↓ (not found)
	  ┌─────────────┐
	  │ Log failed  │
	  │ Return 401  │
	  └─────────────┘

			↓ (found)
   ┌──────────────────────────┐
   │ BCrypt.Verify(           │
   │   submittedPassword,     │
   │   storedHash             │
   │ )                        │
   └────────┬─────────────────┘
			↓ (doesn't match)
	  ┌─────────────┐
	  │ Log failed  │
	  │ Return 401  │
	  └─────────────┘

			↓ (matches)
   ┌──────────────────────┐
   │ Generate JWT token   │
   │ Include user claims  │
   │ Return 200 OK        │
   └──────────────────────┘
```

## Claims in Token

```
JWT Claims Available in Controller:

User.Identity.Name → "john.doe" (ClaimTypes.Name)

User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
  → "john.doe" (sub claim)

User.FindAll(ClaimTypes.Role)
  → ["Admin", "User"] (roles)

User.FindFirst("department")?.Value
  → "engineering" (custom claim)

User.Claims
  → IEnumerable<Claim> (all claims)

Access in code:
  var username = User.Identity?.Name;
  var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
  var roles = User.FindAll(ClaimTypes.Role);
```

## Refresh Token Flow (Optional Enhancement)

```
1. Initial Login
   POST /api/auth/login → {accessToken, refreshToken}

2. Access Resource with Access Token
   GET /api/protected (Authorization: Bearer accessToken)

3. Access Token Expires
   → 401 Unauthorized

4. Refresh Token
   POST /api/auth/refresh (refreshToken) 
   → New {accessToken, refreshToken}

5. Continue with New Token
   GET /api/protected (Authorization: Bearer newAccessToken)
```

These diagrams help visualize how JWT authentication flows through your application and how tokens are created, transmitted, validated, and used.
