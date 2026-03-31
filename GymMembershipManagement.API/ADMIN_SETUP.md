# Admin User Setup

## Admin Credentials

An admin user has been created and seeded into the database through the migration `20260331000000_seed_admin_user.cs`.

### Login Details:
- **Email**: `admin@gym.com`
- **Password**: `Admin@123`
- **Username**: `admin`
- **Role**: Admin

### How to Login:

1. Make a POST request to the login endpoint:
   ```
   POST /api/user/Login
   ```

2. Send the following JSON body:
   ```json
   {
     "email": "admin@gym.com",
     "password": "Admin@123"
   }
   ```

3. The response will include the user details with their UserId and roles.

### What the Admin Can Do:

- Add/Remove users
- Update user details
- View all members, trainers, and admins
- Assign and remove roles to/from users
- Manage trainer information
- Update and delete trainers

### Admin Endpoints:

- `GET /api/admin/GetAllAdmins` - Get all admin users
- `GET /api/admin/GetAllMembers` - Get all members
- `GET /api/admin/GetAllTrainers` - Get all trainers
- `POST /api/admin/AddUser` - Add a new user
- `PUT /api/admin/UpdateUser/{userId}` - Update user details
- `DELETE /api/admin/RemoveUser/{userId}` - Remove a user
- `POST /api/admin/AssignRole` - Assign a role to a user
- `DELETE /api/admin/RemoveRole` - Remove a role from a user
- And more...

## Migration Applied

When you run the application, the migration will automatically:
1. Create the "Admin" role if it doesn't exist
2. Create a Person record for the admin
3. Create the admin User account with hashed password
4. Assign the Admin role to the user

All operations are idempotent, so running the migration multiple times won't create duplicate records.
