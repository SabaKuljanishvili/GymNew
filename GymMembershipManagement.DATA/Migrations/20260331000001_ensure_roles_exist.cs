using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymMembershipManagement.DATA.Migrations
{
    /// <inheritdoc />
    public partial class ensure_roles_exist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure Admin role exists
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin')
                  BEGIN
                    INSERT INTO Roles (RoleName) VALUES ('Admin')
                  END");

            // Ensure Customer role exists
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Customer')
                  BEGIN
                    INSERT INTO Roles (RoleName) VALUES ('Customer')
                  END");

            // Ensure Trainer role exists
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Trainer')
                  BEGIN
                    INSERT INTO Roles (RoleName) VALUES ('Trainer')
                  END");

            // Assign Customer role to users who don't have any role yet
            migrationBuilder.Sql(
                @"DECLARE @CustomerId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Customer')
                  
                  INSERT INTO UserRoles (UserId, RoleId)
                  SELECT DISTINCT u.UserId, @CustomerId
                  FROM Users u
                  WHERE NOT EXISTS (
                    SELECT 1 FROM UserRoles ur WHERE ur.UserId = u.UserId
                  )");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
