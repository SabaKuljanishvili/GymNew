# Role-Based Authorization Flow Diagrams

## 📊 1. User Login & Token Generation Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    AUTHENTICATION FLOW                      │
└─────────────────────────────────────────────────────────────┘

┌──────────────┐
│   User      │
│ (Browser)   │
└──────┬───────┘
       │
       │ 1. POST /api/user/Login
       │    { email, password }
       ▼
┌──────────────────────────────┐
│   ASP.NET Core API           │
│   (UserController.Login)     │
└──────┬───────────────────────┘
       │
       │ 2. Validate credentials
       │    in database
       │
       ▼
┌──────────────────────────────┐
│   Create JWT Token with:     │
│   - UserId                   │
│   - Email                    │
│   - Role (from UserRoles)    │
│   - Expiration time          │
└──────┬───────────────────────┘
       │
       │ 3. Return token
       │
       ▼
┌──────────────────────────────┐
│   Token stored in client     │
│   (LocalStorage/Cookie)      │
└─────────────────────────────┘
```

---

## 🔐 2. Authorization Flow for Protected Endpoint

```
┌─────────────────────────────────────────────────────────────┐
│              AUTHORIZATION FLOW (Protected)                 │
└─────────────────────────────────────────────────────────────┘

┌──────────────┐
│   Client    │
│   Request   │
└──────┬───────┘
       │
       │ GET /api/admin/GetAllUsers
       │ Authorization: Bearer {token}
       ▼
┌──────────────────────────────────┐
│   ASP.NET Core Middleware        │
│   - Validate Token Signature     │
│   - Check Expiration             │
│   - Extract Claims               │
└──────┬───────────────────────────┘
       │
       ├─────────────────────┐
       │                     │
    NO │                     │ YES
       │                     │
       ▼                     ▼
   ┌────────┐        ┌──────────────┐
   │ 401    │        │ Check Role   │
   │ Unauth │        │ Required:    │
   │ Error  │        │ "Admin"      │
   └────────┘        │              │
                     └──────┬───────┘
                            │
                ┌───────────┴───────────┐
                │                       │
             NO │                       │ YES
                │                       │
                ▼                       ▼
            ┌──────┐            ┌──────────────┐
            │ 403  │            │ 200 OK       │
            │ Forb │            │ Process      │
            │ idden│            │ Request      │
            └──────┘            └──────────────┘
```

---

## 📋 3. Controller Authorization Matrix

```
┌─────────────────────────────────────────────────────────────┐
│                    AUTHORIZATION MATRIX                     │
└─────────────────────────────────────────────────────────────┘

USER CONTROLLER
  ├─ POST   /Register           [AllowAnonymous]  ✅ Anyone
  ├─ POST   /Login              [AllowAnonymous]  ✅ Anyone
  ├─ GET    /GetProfile         [Authorize:M,T,A] ✅ Authenticated
  ├─ GET    /GetAllUsers        [Authorize:Admin] ✅ Admin Only
  ├─ PUT    /UpdateProfile      [Authorize:M,T,A] ✅ Authenticated
  ├─ DELETE /DeleteProfile      [Authorize:M,T,A] ✅ Authenticated
  └─ POST   /Logout             [Authorize:M,T,A] ✅ Authenticated

ADMIN CONTROLLER (Entire controller [Authorize:Admin])
  ├─ POST   /AddUser                              ✅ Admin Only
  ├─ GET    /GetUserById                          ✅ Admin Only
  ├─ PUT    /UpdateUser                           ✅ Admin Only
  ├─ DELETE /RemoveUser                           ✅ Admin Only
  └─ POST   /AssignRole                           ✅ Admin Only

TRAINER CONTROLLER
  ├─ POST   /AssignSchedule     [Authorize:Admin]     ✅ Admin Only
  ├─ GET    /Schedules          [Authorize:A,T]       ✅ Admin/Trainer
  ├─ GET    /AllSchedules       [Authorize:Admin]     ✅ Admin Only
  ├─ PUT    /UpdateSchedule     [Authorize:A,T]       ✅ Admin/Trainer
  └─ DELETE /DeleteSchedule     [Authorize:Admin]     ✅ Admin Only

GYM CLASS CONTROLLER
  ├─ GET    /GetAllGymClasses   [AllowAnonymous]  ✅ Anyone
  ├─ GET    /GetGymClassById    [AllowAnonymous]  ✅ Anyone
  ├─ POST   /AddGymClass        [Authorize:Admin] ✅ Admin Only
  ├─ PUT    /UpdateGymClass     [Authorize:Admin] ✅ Admin Only
  └─ DELETE /DeleteGymClass     [Authorize:Admin] ✅ Admin Only

MEMBERSHIP CONTROLLER
  ├─ POST   /Register           [Authorize:Admin]     ✅ Admin Only
  ├─ PUT    /Renew              [Authorize:Admin]     ✅ Admin Only
  ├─ PUT    /Update             [Authorize:Admin]     ✅ Admin Only
  ├─ DELETE /Delete             [Authorize:Admin]     ✅ Admin Only
  ├─ GET    /Status             [Authorize:M,T,A]     ✅ Authenticated
  ├─ GET    /ByUser             [Authorize:A,T]       ✅ Admin/Trainer
  └─ GET    /Active             [Authorize:A,T]       ✅ Admin/Trainer

ROLE CONTROLLER (Entire controller [Authorize:Admin])
  ├─ GET    /GetAllRoles                          ✅ Admin Only
  ├─ GET    /GetRoleById                          ✅ Admin Only
  ├─ POST   /CreateRole                           ✅ Admin Only
  ├─ PUT    /UpdateRole                           ✅ Admin Only
  └─ DELETE /DeleteRole                           ✅ Admin Only

Legend:
M = Member (Customer)
T = Trainer
A = Admin
```

---

## 👥 4. Role Permission Tree

```
┌─────────────────────────────────────────────────────────────┐
│                   ROLE PERMISSION TREE                      │
└─────────────────────────────────────────────────────────────┘

Authenticated User (Logged In)
│
├─ Anonymous Access ✓
│   ├─ Browse Gym Classes
│   └─ View Class Details
│
├─ Member (Customer) Role
│   ├─ View own profile
│   ├─ Update own profile
│   ├─ Delete own account
│   ├─ View public gym classes
│   ├─ View own memberships
│   └─ View own enrollments
│
├─ Trainer Role
│   ├─ All Member permissions
│   ├─ View own schedules
│   ├─ Update own schedules
│   ├─ View gym class members
│   └─ View member information
│
└─ Admin Role ⭐
    ├─ All Member permissions
    ├─ All Trainer permissions
    ├─ User management (CRUD)
    ├─ Role assignment/revocation
    ├─ Gym class management (CRUD)
    ├─ Schedule management (CRUD)
    ├─ Membership management (CRUD)
    ├─ View all system data
    └─ System administration
```

---

## 🔄 5. Request Processing Pipeline

```
┌─────────────────────────────────────────────────────────────┐
│           REQUEST PROCESSING PIPELINE                       │
└─────────────────────────────────────────────────────────────┘

Client Request
     │
     ▼
┌──────────────────────────┐
│ 1. Route Matching        │
│    (Find Controller)     │
└──────────┬───────────────┘
           │
           ▼
┌──────────────────────────┐
│ 2. Authentication Check  │
│    (Validate JWT Token)  │
└──────────┬───────────────┘
           │
     ┌─────┴──────┐
     │            │
    NO           YES
     │            │
     │            ▼
     │     ┌──────────────────────────┐
     │     │ 3. Authorization Check   │
     │     │    (Check Role Claims)   │
     │     └──────────┬───────────────┘
     │                │
     │          ┌─────┴─────┐
     │          │           │
     │        NO │           │ YES
     │          │           │
     ▼          ▼           ▼
┌────────┐  ┌──────┐  ┌───────────────┐
│ 401    │  │ 403  │  │ 4. Execute    │
│ Unauth │  │ Forb │  │    Controller │
│        │  │ idden│  │    Method     │
└────────┘  └──────┘  └───────┬───────┘
                               │
                               ▼
                        ┌──────────────┐
                        │ 5. Response  │
                        │ (200, 201...)│
                        └──────────────┘
```

---

## 🏆 6. Role Assignment Workflow

```
┌─────────────────────────────────────────────────────────────┐
│           ROLE ASSIGNMENT WORKFLOW (Admin Only)             │
└─────────────────────────────────────────────────────────────┘

Start
 │
 ▼
┌──────────────────────────────┐
│ 1. Admin logged in           │
│    (Has Admin role)          │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│ 2. Call AssignRole endpoint  │
│    POST /api/admin/AssignRole│
│    { userId: 5, roleId: 2 }  │
│    Authorization: Bearer...  │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│ 3. Verify Admin Role         │
│    [Authorize(Roles="Admin")]│
└──────────┬───────────────────┘
           │
     ┌─────┴─────┐
     │           │
  YES │           │ NO
     │           │
     ▼           ▼
   ┌──┐      ┌────────┐
   │✓ │      │ 403    │
   │  │      │ Forb   │
   └──┘      │ idden  │
     │       └────────┘
     │
     ▼
┌──────────────────────────────┐
│ 4. Remove old roles          │
│    FROM UserRoles            │
│    WHERE UserId = 5          │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│ 5. Assign new role           │
│    INSERT UserRoles          │
│    (UserId: 5, RoleId: 2)    │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│ 6. Success Response          │
│    "Role assigned"           │
│    User now has Trainer role │
└──────────────────────────────┘
```

---

## 🔐 7. Security Validation Layers

```
┌─────────────────────────────────────────────────────────────┐
│            SECURITY VALIDATION LAYERS                       │
└─────────────────────────────────────────────────────────────┘

Layer 1: HTTPS/TLS
  └─ Encrypts token in transit
     (Authorization: Bearer {encrypted})

Layer 2: Token Signature Validation
  └─ Verifies token wasn't tampered with
     (HMAC-SHA256 signature check)

Layer 3: Token Expiration Check
  └─ Ensures token hasn't expired
     (Comparing exp claim to current time)

Layer 4: Issuer & Audience Validation
  └─ Verifies token is from trusted source
     (iss, aud claims check)

Layer 5: Role Claims Extraction
  └─ Extracts role from token
     (ReadRole claim from JWT payload)

Layer 6: Endpoint Role Requirement
  └─ Compares user role vs required role
     ([Authorize(Roles = "Admin")])

Layer 7: Business Logic Validation
  └─ Additional checks in controller
     (e.g., "Can only edit own profile")

Result: ✅ Request only proceeds if ALL layers pass
```

---

## 📈 8. HTTP Status Codes Flow

```
┌─────────────────────────────────────────────────────────────┐
│           HTTP STATUS CODES & MEANINGS                      │
└─────────────────────────────────────────────────────────────┘

Request
│
├─ Public Endpoint?
│  └─ YES → 200 OK (if request valid)
│
├─ Authorization Header Present?
│  ├─ NO → 401 Unauthorized
│  │
│  └─ YES → Validate Token
│     ├─ Invalid/Expired → 401 Unauthorized
│     │
│     └─ Valid Token → Check Role
│        ├─ Wrong Role → 403 Forbidden
│        │
│        └─ Correct Role → Process Request
│           ├─ Valid Data → 200 OK (or 201 Created)
│           ├─ Invalid Data → 400 Bad Request
│           ├─ Resource Not Found → 404 Not Found
│           └─ Server Error → 500 Internal Server Error

Summary:
  400 = "Your request data is invalid"
  401 = "You need to provide a valid token"
  403 = "You have a valid token but wrong permissions"
  404 = "The resource doesn't exist"
  500 = "Server had an error"
```

---

## 🎓 How Each Role Experiences the API

```
┌─────────────────────────────────────────────────────────────┐
│              ROLE EXPERIENCE MATRIX                         │
└─────────────────────────────────────────────────────────────┘

ANONYMOUS USER (Not Logged In)
├─ Can: Register, Login, Browse Classes, View Class Details
└─ Cannot: Everything else
   Response: 401 Unauthorized

┌─────────────────────────────────────────────────────────────┐

MEMBER (Customer) Role
├─ Can: Personal profile, view classes, view memberships
├─ Try Admin endpoint → 403 Forbidden
├─ Try Trainer endpoint → 403 Forbidden
└─ Try Member endpoint → 200 OK

┌─────────────────────────────────────────────────────────────┐

TRAINER Role
├─ Can: Manage own schedules, view members
├─ Can also: All Member permissions
├─ Try Admin endpoint → 403 Forbidden
├─ Try Trainer endpoint → 200 OK
└─ Try Member endpoint → 200 OK

┌─────────────────────────────────────────────────────────────┐

ADMIN Role ⭐
├─ Can: EVERYTHING!
├─ Admin endpoints → 200 OK
├─ Trainer endpoints → 200 OK
├─ Member endpoints → 200 OK
└─ Public endpoints → 200 OK
```

---

This visualization helps understand the complete authorization flow in your Gym Membership Management API!
