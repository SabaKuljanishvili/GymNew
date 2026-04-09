# 🐛 Token Authorization Debugging Guide

## 🎯 ახლა გაქვთ 3 Debug Endpoints

### 1️⃣ **POST /api/debug/VerifyToken** - Token Decode

**რა აკეთებს:** Token-ის აქაუნთი (claims, expiration, role)

```bash
POST /api/debug/VerifyToken
Content-Type: application/json

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response:**
```json
{
  "success": true,
  "tokenInfo": {
    "issuer": "GymMembershipAPI",
    "audience": "GymMembershipClient",
    "expiresAt": "2026-04-07T15:35:00Z",
    "isExpired": false
  },
  "roleClaimFound": true,
  "roleValue": "Admin",
  "summary": {
    "userId": "11",
    "email": "kuljanisaba@gmail.com",
    "username": "Kuljana",
    "role": "Admin"
  }
}
```

---

### 2️⃣ **GET /api/debug/WhoAmI** - Current User Info (Requires Token)

**რა აკეთებს:** აბრუნდება თქვენი current user info

```bash
GET /api/debug/WhoAmI
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
{
  "isAuthenticated": true,
  "identity": {
    "authenticationType": "Bearer",
    "name": "Kuljana"
  },
  "roleValue": "Admin",
  "hasRole": true,
  "allRoles": ["Admin"]
}
```

---

### 3️⃣ **POST /api/debug/CheckRole** - Role Verification (Requires Token)

**რა აკეთებს:** ამოწმებს თქვენ რო გაქვთ კონკრეტული role

```bash
POST /api/debug/CheckRole
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "roleName": "Admin"
}
```

**Response:**
```json
{
  "requestedRole": "Admin",
  "hasRole": true,
  "userRoles": ["Admin"],
  "userId": "11",
  "email": "kuljanisaba@gmail.com"
}
```

---

## 🧪 Swagger-ში Test ისინი

### Step 1: Login

```
POST /api/user/Login

Body:
{
  "email": "kuljanisaba@gmail.com",
  "password": "your_password"
}
```

**Copy the token from response**

---

### Step 2: Verify Token

```
POST /api/debug/VerifyToken

Body:
{
  "token": "paste_your_token_here"
}
```

✅ ხედავთ token-ის დეტალებს!

---

### Step 3: Authorize in Swagger

```
Click 🔒 "Authorize"
Paste token
Click "Authorize"
```

---

### Step 4: Check Who You Are

```
GET /api/debug/WhoAmI

ხედავთ: {"roleValue": "Admin"}
```

---

### Step 5: Verify Role Access

```
POST /api/debug/CheckRole

Body:
{
  "roleName": "Admin"
}

Response: {"hasRole": true}
```

---

## 🔍 Troubleshooting Using Debug Endpoints

### Problem: Token აბრუნდება მაგრამ protected endpoints არ მუშაობს

**Solution:**

#### Step 1: Decode Token
```
POST /api/debug/VerifyToken
```

**ხედე:**
- ✅ `"roleClaimFound": true` 
- ✅ `"roleValue": "Admin"`

თუ `false` ან null:
```json
❌ "roleClaimFound": false
❌ "roleValue": "NOT FOUND"
```

**მიზეზი:** TokenService არ დამატებს role claim!

---

#### Step 2: Check Current User

```
GET /api/debug/WhoAmI
Authorization: Bearer {token}
```

**ხედე:**
- ✅ `"isAuthenticated": true`
- ✅ `"roleValue": "Admin"`

თუ `false`:
```json
❌ "isAuthenticated": false
❌ "roleValue": "NO ROLE"
```

**მიზეზი:** Token არ არის ვალიდი ან role არ დამატებული!

---

#### Step 3: Verify Role

```
POST /api/debug/CheckRole
Authorization: Bearer {token}

{
  "roleName": "Admin"
}
```

**ხედე:**
- ✅ `"hasRole": true`

თუ `false`:
```json
❌ "hasRole": false
```

**მიზეზი:** Role არ ემთხვევა!

---

## 📋 Debug Checklist

- [ ] Token decode → `"roleClaimFound": true`
- [ ] Token decode → `"roleValue": "Admin"` (არა "NOT FOUND")
- [ ] WhoAmI → `"isAuthenticated": true`
- [ ] WhoAmI → `"roleValue": "Admin"` (არა "NO ROLE")
- [ ] CheckRole Admin → `"hasRole": true`
- [ ] Protected endpoint → `200 OK` (არა 403)

---

## 🎯 სწორი vs არასწორი Token

### ✅ GOOD Token
```json
{
  "success": true,
  "roleClaimFound": true,
  "roleValue": "Admin",
  "summary": {
    "role": "Admin"
  }
}
```

### ❌ BAD Token
```json
{
  "success": true,
  "roleClaimFound": false,
  "roleValue": "NOT FOUND",
  "summary": {
    "role": "NOT FOUND"
  }
}
```

**Fix:** Database-ში User-ს role დამატო!

---

## 💡 Database Fix Script

თუ token არ აქვს role claim:

```sql
-- 1. Admin role ID რამე?
SELECT RoleId FROM Roles WHERE RoleName = 'Admin';

-- 2. User 11-ს არ აქვს role?
SELECT * FROM UserRoles WHERE UserId = 11;

-- 3. დამატო role (if not exists)
INSERT INTO UserRoles (UserId, RoleId) 
VALUES (11, 1);  -- 1 = Admin RoleId

-- 4. Verify
SELECT ur.*, r.RoleName 
FROM UserRoles ur
JOIN Roles r ON ur.RoleId = r.RoleId
WHERE ur.UserId = 11;
```

---

## 🚀 Complete Debug Workflow

```
1. Login
   POST /api/user/Login
   Copy token

2. Decode Token
   POST /api/debug/VerifyToken
   Paste token
   Check: roleClaimFound = true?

3. Check Authentication
   GET /api/debug/WhoAmI
   Header: Authorization: Bearer {token}
   Check: isAuthenticated = true?

4. Verify Role
   POST /api/debug/CheckRole
   Header: Authorization: Bearer {token}
   Body: { "roleName": "Admin" }
   Check: hasRole = true?

5. Try Protected Endpoint
   GET /api/admin/GetAllUsers
   Header: Authorization: Bearer {token}
   
   ✅ 200 OK → SUCCESS!
   ❌ 403 Forbidden → Role claim issue
   ❌ 401 Unauthorized → Token invalid
```

---

## 🔧 Common Fixes

| Issue | Check | Fix |
|-------|-------|-----|
| roleClaimFound = false | Database | `INSERT INTO UserRoles (UserId, RoleId) VALUES (11, 1);` |
| isAuthenticated = false | Token format | `Authorization: Bearer {token}` (not just token) |
| hasRole = false | Token decode | Re-login to get fresh token with role |
| 403 Forbidden | Debug endpoint | Role doesn't match endpoint requirement |

---

**Use these debug endpoints to verify your token and role configuration!** 🎉
