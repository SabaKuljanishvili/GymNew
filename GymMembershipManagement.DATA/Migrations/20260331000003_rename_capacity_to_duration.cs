using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymMembershipManagement.DATA.Migrations
{
    /// <inheritdoc />
    public partial class rename_capacity_to_duration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "GymClasses",
                newName: "Duration");

            // Update existing data to set Duration values
            migrationBuilder.Sql(
                @"UPDATE GymClasses SET Duration = 40 WHERE GymClassName IN ('Wrestling', 'judo');
                  UPDATE GymClasses SET Duration = 30 WHERE GymClassName IN ('Karate', 'Boxing');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Update duration values back to capacity for rollback
            migrationBuilder.Sql(
                @"UPDATE GymClasses SET Capacity = Duration;");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "GymClasses",
                newName: "Capacity");
        }
    }
}
