# JWT Authentication Usage Guide

## Overview
Your application now supports JWT-based authentication for API endpoints. The `ClaimsPrincipal` is properly populated from JWT tokens, and `ICurrentUserService` can read the user information from `HttpContextAccessor`.

## How It Works

### 1. Login and Get JWT Token

**Endpoint:** `POST /api/illegal-login`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "your-password"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Use the Token in API Requests

Include the JWT token in the `Authorization` header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. JWT Token Contains These Claims

- `ClaimTypes.NameIdentifier`: User's GUID
- `ClaimTypes.Email`: User's email
- `ClaimTypes.Name`: Username
- `ClaimTypes.Role`: User's role(s) - Teacher, Student, etc.

## Protected Endpoints

All API endpoints now require authentication:
- `GET /api/slots/available` - Requires authentication
- `GET /api/rooms` - Requires authentication
- `POST /api/slots{id:guid}/book` - Requires authentication

The login endpoint is publicly accessible:
- `POST /api/illegal-login` - Anonymous access allowed

## Using CurrentUserService

The `ICurrentUserService` now works correctly with JWT tokens:

```csharp
public class MyService
{
    private readonly ICurrentUserService _currentUserService;

    public MyService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public void DoSomething()
    {
        var userId = _currentUserService.UserId;      // Gets from ClaimTypes.NameIdentifier
        var userRole = _currentUserService.UserRole;  // Gets from ClaimTypes.Role
    }
}
```

## Testing with cURL

### 1. Login
```bash
curl -X POST https://localhost:5273/api/illegal-login \
  -H "Content-Type: application/json" \
  -d '{"email":"teacher@test.com","password":"YourPassword123!"}'
```

### 2. Use the token
```bash
curl -X GET https://localhost:5273/api/slots/available \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## Testing with Postman

1. **Login:**
   - Method: POST
   - URL: `https://localhost:5273/api/illegal-login`
   - Body (raw JSON):
     ```json
     {
       "email": "teacher@test.com",
       "password": "YourPassword123!"
     }
     ```
   - Copy the token from response

2. **Make authenticated request:**
   - Method: GET
   - URL: `https://localhost:5273/api/slots/available`
   - Headers: Add `Authorization` with value `Bearer YOUR_TOKEN`

## Security Configuration

- **JWT Secret Key:** Stored in code (should be moved to configuration)
- **Token Expiration:** 1 hour
- **Algorithm:** HMAC-SHA256
- **Clock Skew:** 0 (no tolerance for expired tokens)

## Important Notes

⚠️ **Security Recommendations:**

1. **Move the secret key to configuration:**
   ```csharp
   var jwtKey = builder.Configuration["Jwt:SecretKey"]
       ?? throw new InvalidOperationException("JWT secret key not configured");
   ```

2. **Add to appsettings.json:**
   ```json
   {
     "Jwt": {
       "SecretKey": "your-super-secret-key-minimum-32-chars-required!"
     }
   }
   ```

3. **For production:**
   - Use environment variables or Azure Key Vault
   - Enable HTTPS only
   - Consider adding issuer and audience validation
   - Implement refresh tokens for better security
