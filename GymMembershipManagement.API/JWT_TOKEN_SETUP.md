# JWT Token Authentication Implementation

## Overview
JWT (JSON Web Token) authentication has been implemented for the Gym Membership API. When users log in, they receive a token that they can use to authenticate subsequent requests.

## What Has Been Added

### 1. **TokenService** (`TokenService.cs`)
- Service that generates JWT tokens for authenticated users
- Includes user information and roles as claims
- Configurable token expiration

### 2. **LoginResponseDTO** (`LoginResponseDTO.cs`)
- Extended response model that includes:
  - User details (UserId, Username, Email, FirstName, LastName)
  - Registration date
  - List of roles assigned to the user
  - **JWT Token** (the key addition)

### 3. **JWT Configuration** (`appsettings.json`)
```json
"JwtSettings": {
  "SecretKey": "ThisIsAVerySecretKeyThatShouldBeAtLeast32CharactersLongForSecurityPurposes!",
  "Issuer": "GymMembershipAPI",
  "Audience": "GymMembershipClient",
  "ExpirationMinutes": 60
}
```

### 4. **Authentication Middleware** (`Program.cs`)
- JWT Bearer authentication configured
- Token validation with signature, issuer, audience, and lifetime checks
- Authentication middleware registered in the pipeline

### 5. **NuGet Packages Added**
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `System.IdentityModel.Tokens.Jwt` - JWT token creation and validation

## How It Works

### 1. User Registration/Login Flow:
```
User → POST /api/user/Login → UserService.Login()
       → TokenService.GenerateToken() → JWT Token Generated
       → LoginResponseDTO with Token → User
```

### 2. Token Structure:
```
JWT Token consists of three parts:
- Header: Algorithm and token type
- Payload: Claims (UserId, Email, Username, Roles)
- Signature: HMAC SHA256 signature
```

### 3. Making Authenticated Requests:
```
User sends token in Authorization header:
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Login Response Example

### Request:
```bash
POST /api/user/Login
Content-Type: application/json

{
  "email": "admin@gym.com",
  "password": "Admin@123"
}
```

### Response:
```json
{
  "userId": 1,
  "username": "admin",
  "email": "admin@gym.com",
  "firstName": "Admin",
  "lastName": "User",
  "registrationDate": "2026-03-31T12:00:00Z",
  "roles": ["Admin"],
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJhZG1pbkBneW0uY29tIiwibmFtZSI6ImFkbWluIiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzExODk2ODAwLCJleHAiOjE3MTE5MDA0MDAsImlhdCI6MTcxMTg5NjgwMH0.signature"
}
```

## Token Claims

Each JWT token includes the following claims:
- **NameIdentifier (sub)**: UserId
- **Email**: User's email
- **Name**: Username
- **Role**: Each role the user has

## Token Expiration

- Default expiration: **60 minutes** (configurable in appsettings.json)
- After expiration, user must log in again to get a new token

## Security Configuration

### Current Settings (appsettings.json):
```json
"JwtSettings": {
  "SecretKey": "ThisIsAVerySecretKeyThatShouldBeAtLeast32CharactersLongForSecurityPurposes!",
  "Issuer": "GymMembershipAPI",
  "Audience": "GymMembershipClient",
  "ExpirationMinutes": 60
}
```

⚠️ **Important for Production:**
- Change the `SecretKey` to a strong, unique value
- Use environment variables instead of hardcoding secrets
- Use `appsettings.Production.json` for production configuration
- Consider using Azure Key Vault or similar for secret management

## Using the Token

### 1. After Login:
```javascript
// Store the token
const response = await fetch('/api/user/Login', {
  method: 'POST',
  body: JSON.stringify({ email: 'admin@gym.com', password: 'Admin@123' })
});
const data = await response.json();
const token = data.token;
localStorage.setItem('token', token);
```

### 2. Making Authenticated Requests:
```javascript
// Include token in Authorization header
const headers = {
  'Authorization': `Bearer ${localStorage.getItem('token')}`,
  'Content-Type': 'application/json'
};

const response = await fetch('/api/admin/GetAllAdmins', { headers });
```

## API Endpoints Using JWT

### Protected Endpoints (Require Authentication):
- `GET /api/admin/GetAllAdmins`
- `GET /api/admin/GetAllMembers`
- `GET /api/admin/GetAllTrainers`
- `POST /api/admin/AddUser`
- `PUT /api/admin/UpdateUser/{userId}`
- `DELETE /api/admin/RemoveUser/{userId}`
- And all other admin endpoints...

### Public Endpoints (No Authentication Required):
- `POST /api/user/Register` - User registration
- `POST /api/user/Login` - User login

## Changes Made to Existing Files

1. **UserService.cs**
   - Updated `Login()` method to return `LoginResponseDTO`
   - Added `ITokenService` dependency

2. **IUserService.cs**
   - Changed `Login()` return type from `UserDTO` to `LoginResponseDTO`

3. **UserController.cs**
   - Updated `Login()` endpoint to return `LoginResponseDTO`

4. **Program.cs**
   - Added JWT authentication configuration
   - Registered `ITokenService` and `TokenService`
   - Added authentication middleware to the pipeline

5. **appsettings.json**
   - Added `JwtSettings` configuration section

## Testing with Swagger

1. Navigate to `https://localhost:xxxx/swagger`
2. Click "Authorize" button
3. Paste the token you received from login with format: `Bearer <your_token>`
4. Click "Authorize"
5. Try authenticated endpoints

## Next Steps (Optional Enhancements)

- [ ] Add refresh token functionality
- [ ] Implement token revocation/blacklist
- [ ] Add rate limiting on login endpoint
- [ ] Implement two-factor authentication (2FA)
- [ ] Add role-based authorization attributes on controller methods
