# 🔧 Swagger Token Authorization Fix & Debugging Guide

## 📋 პრობლემა

Swagger-ში login კეთების შემდეგ, token აბრუნდება, მაგრამ **protected endpoints არ მუშაობს** (403 ან 401).

```json
{
  "token": "eyJ...",
  "roleId": 1,
  "roles": ["Admin"]
}
```

მაგრამ როდესაც ამ token-ით GET აკეთებ `/api/admin/GetAllUsers` → ❌ დაუშვებელი

---

## ✅ სამი ნაბიჯი გამოსწორებაზე

### 1️⃣ Swagger-ში Authorize ღილაკი გამოჩნდება

**პირველი:** დააწკაპეთ მწვანე **"Authorize"** ღილაკი (დაემატა Program.cs-ში)

```
🔒 Authorize
Enter your token here...
```

---

### 2️⃣ Token-ი Paste გააკეთეთ

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**ან მხოლოდ token-ი (უსაფრთხოდ):**

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

დააწკაპეთ **Authorize** ✅

---

### 3️⃣ ახლა გამოსცადეთ Protected Endpoint

```
GET /api/admin/GetAllUsers

✅ უნდა დაბრუნდეს: 200 OK
❌ თუ 403 → role არ ემთხვევა
❌ თუ 401 → token invalid
```

---

## 🔍 Debugging - თუ არ მუშაობს

### ✓ Step 1: Token Decode

1. გადაიტანეთ token [jwt.io](https://jwt.io)-ზე
2. **Payload** ნაწილში შეამოწმეთ:

```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "11",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin"
}
```

**თუ `role` claim არ იყო:**
- ❌ TokenService არ დამატებს role claim-ს
- ❌ User-ს UserRole არ აქვს database-ში

---

### ✓ Step 2: Database Check

```sql
-- User 11-ს role აქვს?
SELECT * FROM UserRoles WHERE UserId = 11;

-- უნდა აბრუნდეს:
UserId | RoleId
-------|-------
11     | 1
```

**თუ result empty:**
```sql
-- Admin role ID ის რისი?
SELECT RoleId FROM Roles WHERE RoleName = 'Admin';

-- შემდეგ დააემატო role User-ს:
INSERT INTO UserRoles (UserId, RoleId) VALUES (11, 1);
```

---

### ✓ Step 3: Login ხელმიდან გაკეთეთ

```bash
curl -X POST http://localhost:5001/api/user/Login \
  -H "Content-Type: application/json" \
  -d '{"email":"kuljanisaba@gmail.com","password":"..."}'
```

**Response უნდა იყოს:**
```json
{
  "token": "eyJ...",
  "roles": ["Admin"],
  "roleId": 1
}
```

---

### ✓ Step 4: Token გამოიყენეთ Request-ში

```bash
curl -X GET http://localhost:5001/api/admin/GetAllUsers \
  -H "Authorization: Bearer eyJ..."
```

**თუ 200 OK → ✅ Success!**

**თუ 403 Forbidden:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

მიზეზი: Token-ში role claim არ არის სწორი

---

## 🛠️ Swagger UI გამოყენება (ახალი!)

**Program.cs-ში დამატო:**

```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT"
});

c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme { Reference = new OpenApiReference { ... } },
        new string[] { }
    }
});
```

**ახლა Swagger-ში:**

1. მწვანე 🔒 **"Authorize"** ღილაკი დაგეხილება
2. Paste token-ი
3. დააწკაპეთ Authorize
4. ყველა request-ი header-ში token-ი მოიტანს

---

## 📋 Checklist - თუ არ მუშაობს

- [ ] Database-ში User 11-ს RoleId = 1 აქვს
- [ ] Token decode-ში role claim არის ჩაწერილი
- [ ] Program.cs-ში Swagger JWT configuration დამატებულია
- [ ] Swagger-ში Authorize ღილაკი დაჩნდა
- [ ] Token Swagger-ში დაემატო (ან curl header-ში)
- [ ] Authorization header format: `Bearer {token}`

---

## 🎯 ხშირი შეცდომები

### ❌ შეცდომა 1: Token-ი არ გადაეცემა header-ში

```bash
# ❌ რა არ არის
GET /api/admin/GetAllUsers
Body: { "token": "eyJ..." }

# ✅ სწორია
GET /api/admin/GetAllUsers
Header: Authorization: Bearer eyJ...
```

---

### ❌ შეცდომა 2: "Bearer " წინწინ გამოტოვებული

```bash
# ❌ რა არ არის
Authorization: eyJ...

# ✅ სწორია
Authorization: Bearer eyJ...
```

---

### ❌ შეცდომა 3: Token მეორე დროს იყენებთ (ვადა გასული)

Token ვადა: **1 საათი**

თუ 1 საათზე მეტი გაიარა:
```bash
POST /api/user/Login  # ხელმიდან ხელმის token-ი!
```

---

## ✅ სწორი ფორმატი - Swagger Authorize

```
Dialog: "Authorize"
Input field:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eye...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Button: [Authorize]
```

---

## 🔄 Complete Workflow

```
1. Login
   POST /api/user/Login
   { "email": "...", "password": "..." }
   ↓
   Response: { "token": "eyJ...", "roles": ["Admin"] }

2. Copy Token

3. Swagger Authorize
   🔒 Click "Authorize" → Paste token → Click "Authorize"

4. Test Protected Endpoint
   GET /api/admin/GetAllUsers
   ↓
   ✅ 200 OK → User მოიხმარს!
   ❌ 403 Forbidden → Role არ ემთხვევა
   ❌ 401 Unauthorized → Token invalid
```

---

## 📞 თუ მაინც არ მუშაობს

**Database check script:**

```sql
-- 1. User არსებობს?
SELECT * FROM Users WHERE UserId = 11;

-- 2. User-ს role აქვს?
SELECT ur.*, r.RoleName 
FROM UserRoles ur
JOIN Roles r ON ur.RoleId = r.RoleId
WHERE ur.UserId = 11;

-- 3. Role სწორი აქვს?
SELECT * FROM Roles WHERE RoleId = 1;

-- 4. ხელმის fix - role დამატო:
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = 11 AND RoleId = 1)
    INSERT INTO UserRoles (UserId, RoleId) VALUES (11, 1);
```

**თუ script successfully დაბრუნდა, ხელმიდან login მოკეთო და token გამოიყენო!**

---

## ✨ Final Summary

| Element | Before | After |
|---------|--------|-------|
| Swagger Auth | ❌ No button | ✅ "Authorize" button |
| Token Claim | ❌ Maybe missing role | ✅ Role claim verified |
| Database | ? | ✅ User has role |
| Header Format | Manual | ✅ Swagger handles it |

**Your API is now fully Swagger-compatible with JWT authentication!** 🎉
