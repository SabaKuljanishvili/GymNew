# Role Assignment Logic Update

## 🎯 Change Summary

The role assignment logic has been updated so that when a new role is assigned to a user, **all previous roles are removed** and only the new role remains.

### Before:
```
User: Customer
↓ (Assign Admin role)
Result: Customer + Admin (multiple roles)
```

### After:
```
User: Customer
↓ (Assign Admin role)
Result: Admin only (single role)
```

---

## 📝 Implementation

### Modified Method: `AdminService.AssignRoleToUser()`

**Previous Logic:**
```csharp
await _roleRepository.AssignRoleToUserAsync(dto.UserId, dto.RoleId);
```

**New Logic:**
```csharp
// 1. Remove all existing roles
if (user.UserRoles != null && user.UserRoles.Any())
{
    foreach (var userRole in user.UserRoles.ToList())
    {
        await _roleRepository.RemoveRoleFromUserAsync(dto.UserId, userRole.RoleId);
    }
}

// 2. Assign the new role
await _roleRepository.AssignRoleToUserAsync(dto.UserId, dto.RoleId);
```

---

## 🔄 Workflow Example

### Scenario: Promote Customer to Admin

**Initial State:**
```
UserId: 5
Name: John Doe
Role: Customer
```

**API Call:**
```bash
POST /api/admin/AssignRole
{
  "userId": 5,
  "roleId": 1  // Admin role
}
```

**Result:**
```
UserId: 5
Name: John Doe
Role: Admin  (Customer role is removed)
```

### Scenario: Change Admin to Trainer

**Initial State:**
```
UserId: 5
Name: John Doe
Role: Admin
```

**API Call:**
```bash
POST /api/admin/AssignRole
{
  "userId": 5,
  "roleId": 3  // Trainer role
}
```

**Result:**
```
UserId: 5
Name: John Doe
Role: Trainer  (Admin role is removed)
```

---

## 📊 Role Types in System

1. **Admin** (RoleId: 1)
   - Full system access
   - Can manage users, roles, trainers

2. **Trainer** (RoleId: 2)
   - Can view members
   - Can manage training schedules

3. **Customer** (RoleId: 3)
   - Regular gym member
   - Can view trainers
   - Can book classes

---

## 🧪 Testing

### Test Case 1: Promote Member to Trainer
```bash
# 1. Register a new member
POST /api/user/Register
{
  "firstName": "Test",
  "lastName": "User",
  "email": "test@gym.com",
  "password": "Test@123",
  "username": "testuser",
  "phone": "+995555555555",
  "address": "Tbilisi"
}

# 2. Member is automatically assigned Customer role

# 3. Get member details to see Role
GET /api/user/GetProfile/1

# 4. Assign Trainer role
POST /api/admin/AssignRole
{
  "userId": 1,
  "roleId": 2  // Trainer
}

# 5. Verify member now has Trainer role only
GET /api/user/GetProfile/1
# Result: { "roles": ["Trainer"] }  // No Customer role
```

### Test Case 2: Change Trainer to Admin
```bash
# 1. Assign Admin role to existing trainer
POST /api/admin/AssignRole
{
  "userId": 2,
  "roleId": 1  // Admin
}

# 2. Verify trainer is now admin
GET /api/user/GetProfile/2
# Result: { "roles": ["Admin"] }  // No Trainer role
```

---

## 💡 Key Benefits

1. **Clear Role Definition**
   - Each user has exactly one role at a time
   - No ambiguous multiple roles

2. **Easier Permissions Management**
   - Single role-based authorization
   - Simpler permission checking

3. **Better Audit Trail**
   - Clear when role changes
   - User responsibility is clear

4. **Prevents Role Conflicts**
   - Admin cannot be both Admin and Customer
   - Trainer cannot be both Trainer and Admin

---

## ⚠️ Important Notes

### One Role Per User
- Users now have **exactly one role** at any given time
- Previous role is automatically removed when new one is assigned

### No Downtime
- Change applies immediately
- User must log in again to get updated token with new role

### API Response
When assigning a role, the response remains the same:
```json
{
  "message": "Role assigned successfully."
}
```

But subsequent user queries will show the new role only.

---

## 🔐 Security Implications

1. **Role Removal is Immediate**
   - User loses previous role permissions instantly
   - But existing tokens remain valid until expiration
   - User should log in again to get token with new role

2. **No Partial Roles**
   - User either has Admin role OR Trainer role OR Customer role
   - Cannot be "both admin and trainer"

---

## 📋 Affected Endpoints

- `POST /api/admin/AssignRole` - Now replaces all roles
- `GET /api/admin/GetAllAdmins` - Returns only current admins
- `GET /api/admin/GetAllMembers` - Returns only current members
- `GET /api/admin/GetAllTrainers` - Returns only current trainers
- `GET /api/user/GetProfile/{userId}` - Shows only current role

---

## ✅ Status

✅ **Implementation Complete**
✅ **Build Successful**
✅ **Ready for Testing**

The role assignment logic now works as intended - each user has exactly one role at a time!
