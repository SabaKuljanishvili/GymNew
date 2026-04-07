# Quick Reference: Role-Based Authorization

## 🎯 Three Roles

### 👑 Admin
**What they can do:**
- Manage users (create, read, update, delete)
- Assign roles to users
- Create/edit/delete gym classes
- Assign trainers to schedules
- View all system data
- Manage memberships

**API Prefix:** `/api/admin/*`

### 🏋️ Trainer
**What they can do:**
- View their own schedules
- Update their own schedules
- View gym class members
- View membership information

**API Prefix:** `/api/trainer/*`

### 👤 Member/Customer
**What they can do:**
- View own profile
- Update own profile
- View available gym classes (public)
- View own enrollments
- View own memberships

**API Prefix:** `/api/user/*`

---

## 🔓 Public Endpoints (No Login Required)

These endpoints don't need authentication:

```
POST   /api/user/Register           - Create account
POST   /api/user/Login              - Get JWT token
GET    /api/gymclass/GetAllGymClasses    - Browse classes
GET    /api/gymclass/GetGymClassById/{id} - View class details
```

---

## 🔐 Protected Endpoints (Login Required)

All other endpoints require:
1. Valid JWT token
2. Proper role for the endpoint

---

## 📌 Common Authorization Patterns

### Pattern 1: Admin Only
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteUser(int userId) { }
```

✅ Only Admin can call
❌ Trainer gets 403 Forbidden
❌ Member gets 403 Forbidden

### Pattern 2: Admin or Trainer
```csharp
[Authorize(Roles = "Admin,Trainer")]
public async Task<IActionResult> ViewMembers() { }
```

✅ Admin can call
✅ Trainer can call
❌ Member gets 403 Forbidden

### Pattern 3: Any Authenticated User
```csharp
[Authorize(Roles = "Member,Trainer,Admin")]
public async Task<IActionResult> GetProfile(int userId) { }
```

✅ Anyone logged in can call
❌ Anonymous user gets 401 Unauthorized

### Pattern 4: Public Access
```csharp
[AllowAnonymous]
public async Task<IActionResult> GetAllClasses() { }
```

✅ Anyone can call (no token needed)

---

## 🧪 How to Test Authorization

### Step 1: Get User Token

**Admin User:**
```bash
curl -X POST http://localhost:5001/api/user/Login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@gym.com","password":"Admin123!"}'
```

**Trainer User:**
```bash
curl -X POST http://localhost:5001/api/user/Login \
  -H "Content-Type: application/json" \
  -d '{"email":"trainer@gym.com","password":"Trainer123!"}'
```

**Member User:**
```bash
curl -X POST http://localhost:5001/api/user/Login \
  -H "Content-Type: application/json" \
  -d '{"email":"member@gym.com","password":"Member123!"}'
```

### Step 2: Test with Token

**This should work (Admin accessing admin endpoint):**
```bash
curl -X GET http://localhost:5001/api/admin/GetAllUsers \
  -H "Authorization: Bearer {admin_token}"
```

**This should fail (Member accessing admin endpoint):**
```bash
curl -X GET http://localhost:5001/api/admin/GetAllUsers \
  -H "Authorization: Bearer {member_token}"
```

Result: `403 Forbidden`

---

## ⚠️ Common HTTP Status Codes

| Code | Meaning | Reason |
|------|---------|--------|
| 200 | OK | Request successful |
| 201 | Created | Resource created |
| 400 | Bad Request | Invalid input data |
| 401 | Unauthorized | No token or invalid token |
| 403 | Forbidden | Token valid but wrong role |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Server error |

---

## 🔄 User Role Assignment Workflow

### How to Promote Member to Trainer

```bash
POST /api/admin/AssignRole
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "userId": 5,
  "roleId": 2  // Trainer role ID
}
```

**Result:** User's role changes to Trainer
- Previous role removed
- User can now access Trainer endpoints
- User loses access to Admin endpoints

---

## 💾 Database Roles

Check available roles in your database:

```sql
SELECT * FROM Roles;
```

Expected output:
```
RoleId | RoleName
-------|----------
1      | Admin
2      | Trainer
3      | Member
```

---

## 🛑 Troubleshooting

### Getting 401 Unauthorized?
- Check if token is included in header
- Verify token format: `Authorization: Bearer {token}`
- Check if token has expired
- Use correct login credentials

### Getting 403 Forbidden?
- Token is valid but your role doesn't have access
- Check the endpoint's required role
- Request admin to assign correct role

### Getting 404 Not Found?
- Check endpoint URL spelling
- Verify HTTP method (GET, POST, PUT, DELETE)
- Check if resource exists

---

## 📚 Full Documentation Files

For more details, see:
- `RBAC_IMPLEMENTATION_GUIDE.md` - Complete guide
- `RBAC_IMPLEMENTATION_COMPLETE.md` - Implementation summary
- `JWT_IMPLEMENTATION_SUMMARY.md` - Token details
