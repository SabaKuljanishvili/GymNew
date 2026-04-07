# Role-Based Access Control (RBAC) Implementation Guide

## 📋 Overview

This document outlines the role-based access control system for the Gym Membership Management API. The system ensures that:
1. **Only authenticated users** can perform any operations
2. **Different roles have different permissions** (Admin, Trainer, Member/Customer)
3. **Each endpoint is protected** with appropriate authorization

---

## 🔐 Role Definitions

### 1. **Admin**
- **Permissions:**
  - View all users
  - Add/Update/Delete users
  - Assign roles to users
  - Create/Edit/Delete gym classes
  - Assign trainers to schedules
  - View all schedules
  - Manage reservations
  - View all memberships
  - Full system access

### 2. **Trainer**
- **Permissions:**
  - View own profile
  - View own schedules
  - Update own schedules
  - View gym classes
  - View members in their classes
  - Cannot delete or create users
  - Cannot access admin functions

### 3. **Member/Customer**
- **Permissions:**
  - View own profile
  - Update own profile
  - View available gym classes
  - Enroll in gym classes
  - Cancel enrollment from gym classes
  - View own reservations
  - View own memberships
  - Cannot access other users' data
  - Cannot access admin/trainer functions

---

## 🛡️ Authorization Implementation

### Authentication Requirement
All endpoints (except `/Register` and `/Login`) require a valid JWT token with `Authorization: Bearer {token}`

### Authorization Attributes

#### Using Built-in `[Authorize]` attribute:
```csharp
[Authorize]  // Requires authentication only
public async Task<IActionResult> GetProfile(int userId) { }
```

#### Using Role-based `[Authorize(Roles = "...")]` attribute:
```csharp
[Authorize(Roles = "Admin")]  // Only Admin can access
public async Task<IActionResult> DeleteUser(int userId) { }

[Authorize(Roles = "Admin,Trainer")]  // Admin and Trainer can access
public async Task<IActionResult> GetSchedules() { }

[Authorize(Roles = "Member,Trainer,Admin")]  // Any authenticated user
public async Task<IActionResult> GetProfile(int userId) { }
```

---

## 📊 API Endpoint Authorization Matrix

### UserController

| Method | Endpoint | Public | Auth Required | Required Role | Purpose |
|--------|----------|--------|---------------|---------------|---------|
| POST | `/Register` | ✅ | ❌ | - | User self-registration |
| POST | `/Login` | ✅ | ❌ | - | User login |
| GET | `/GetProfile/{id}` | ❌ | ✅ | Any | View profile |
| GET | `/GetAllUsers` | ❌ | ✅ | Admin | View all users |
| PUT | `/UpdateProfile/{id}` | ❌ | ✅ | Any | Update own profile |
| DELETE | `/DeleteProfile/{id}` | ❌ | ✅ | Any | Delete own profile |
| POST | `/EnrollInGymClass` | ❌ | ✅ | Member | Enroll in class |
| DELETE | `/RemoveFromGymClass` | ❌ | ✅ | Member | Cancel enrollment |

### AdminController

| Method | Endpoint | Public | Auth Required | Required Role | Purpose |
|--------|----------|--------|---------------|---------------|---------|
| POST | `/AddUser` | ❌ | ✅ | Admin | Create new user |
| GET | `/GetUserById/{id}` | ❌ | ✅ | Admin | Get user details |
| PUT | `/UpdateUser/{id}` | ❌ | ✅ | Admin | Update user details |
| DELETE | `/RemoveUser/{id}` | ❌ | ✅ | Admin | Delete user |
| POST | `/AssignRole` | ❌ | ✅ | Admin | Assign role to user |

### TrainerController

| Method | Endpoint | Public | Auth Required | Required Role | Purpose |
|--------|----------|--------|---------------|---------------|---------|
| POST | `/AssignSchedule` | ❌ | ✅ | Admin | Assign trainer to schedule |
| GET | `/Schedules/{trainerId}` | ❌ | ✅ | Admin,Trainer | View trainer schedules |
| GET | `/AllSchedules` | ❌ | ✅ | Admin | View all schedules |
| PUT | `/UpdateSchedule` | ❌ | ✅ | Admin,Trainer | Update schedule |

### GymClassController

| Method | Endpoint | Public | Auth Required | Required Role | Purpose |
|--------|----------|--------|---------------|---------------|---------|
| GET | `/GetAll` | ✅ | ❌ | - | View all classes (public) |
| GET | `/{id}` | ✅ | ❌ | - | View class details (public) |
| POST | `/Create` | ❌ | ✅ | Admin | Create class |
| PUT | `/Update/{id}` | ❌ | ✅ | Admin | Update class |
| DELETE | `/Delete/{id}` | ❌ | ✅ | Admin | Delete class |

---

## 🔑 JWT Token Claims

The JWT token includes role information:
```csharp
{
  "sub": "5",  // UserId
  "email": "user@example.com",
  "role": "Admin",  // Role name
  "exp": 1234567890,
  "iat": 1234567800
}
```

---

## 📝 Implementation Checklist

### Controllers to Update:
- [ ] UserController - Add `[Authorize]` and `[Authorize(Roles = "...")]`
- [ ] AdminController - Add `[Authorize(Roles = "Admin")]`
- [ ] TrainerController - Add `[Authorize(Roles = "Admin,Trainer")]`
- [ ] GymClassController - Add `[Authorize(Roles = "Admin")]` for write operations
- [ ] MembershipController - Add `[Authorize(Roles = "Admin")]` for write operations
- [ ] ReservationController - Add role-based authorization

### Program.cs Configuration:
- [x] JWT Authentication configured
- [x] Authorization middleware added
- [x] Role claims mapping configured

---

## ⚠️ Security Notes

1. **Never expose user data without authorization check**
2. **Always validate the userId matches the authenticated user** (unless Admin)
3. **Trainer can only access their own schedules**
4. **Member can only access their own profile and enrollments**
5. **Admin has full access but should audit sensitive operations**

---

## 🧪 Testing Role-Based Access

### Test with curl:

**Get admin token:**
```bash
curl -X POST https://localhost:5001/api/user/Login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@gym.com","password":"Admin123!"}'
```

**Access admin endpoint:**
```bash
curl -X GET https://localhost:5001/api/admin/GetUserById/5 \
  -H "Authorization: Bearer {token}"
```

**Try with member token (should fail):**
```bash
# This will return 403 Forbidden if user is not Admin
curl -X GET https://localhost:5001/api/admin/GetUserById/5 \
  -H "Authorization: Bearer {member_token}"
```

---

## 📌 Future Enhancements

1. **Policy-based Authorization** - More granular permission control
2. **Claims-based Authorization** - Additional custom claims
3. **Permission Logging** - Audit trail for sensitive operations
4. **Rate Limiting** - Prevent abuse per role
5. **Dynamic Permissions** - Role permissions managed in database
