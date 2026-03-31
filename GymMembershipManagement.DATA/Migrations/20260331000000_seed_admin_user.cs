using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymMembershipManagement.DATA.Migrations
{
    /// <inheritdoc />
    public partial class seed_admin_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Admin role if it doesn't exist
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin')
                  BEGIN
                    INSERT INTO Roles (RoleName) VALUES ('Admin')
                  END");

            // Insert Person for admin
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM Persons WHERE FirstName = 'Admin' AND LastName = 'User')
                  BEGIN
                    INSERT INTO Persons (FirstName, LastName, Phone, Address) 
                    VALUES ('Admin', 'User', '+995555000000', 'Tbilisi, Georgia')
                  END");

            // Insert Admin User
            // Password: Admin@123 (hashed with BCrypt)
            // Hash: $2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86AGR57XO1e
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@gym.com')
                  BEGIN
                    DECLARE @PersonId INT = (SELECT PersonId FROM Persons WHERE FirstName = 'Admin' AND LastName = 'User')
                    
                    INSERT INTO Users (Username, PasswordHash, Email, RegistrationDate, PersonId)
                    VALUES ('admin', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86AGR57XO1e', 'admin@gym.com', GETUTCDATE(), @PersonId)
                  END");

            // Assign Admin role to the admin user
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = (SELECT UserId FROM Users WHERE Email = 'admin@gym.com') AND RoleId = (SELECT RoleId FROM Roles WHERE RoleName = 'Admin'))
                  BEGIN
                    DECLARE @UserId INT = (SELECT UserId FROM Users WHERE Email = 'admin@gym.com')
                    DECLARE @RoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Admin')
                    
                    INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)
                  END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove admin user and related data
            migrationBuilder.Sql(
                @"DELETE FROM UserRoles WHERE UserId = (SELECT UserId FROM Users WHERE Email = 'admin@gym.com');
                  DELETE FROM Users WHERE Email = 'admin@gym.com';
                  DELETE FROM Persons WHERE FirstName = 'Admin' AND LastName = 'User';");
        }
    }
}
