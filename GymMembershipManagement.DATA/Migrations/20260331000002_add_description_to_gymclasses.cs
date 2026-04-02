using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymMembershipManagement.DATA.Migrations
{
    /// <inheritdoc />
    public partial class add_description_to_gymclasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "GymClasses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "GymClasses");
        }
    }
}
