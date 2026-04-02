using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymMembershipManagement.DATA.Migrations
{
    /// <inheritdoc />
    public partial class migration_v11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "GymClasses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "GymClasses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "a combat sport and ancient martial art involving two unarmed individuals grappling, throwing, and pinning an opponent to the ground to win");

            migrationBuilder.UpdateData(
                table: "GymClasses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "GymClassName" },
                values: new object[] { "a modern Japanese martial art and Olympic sport founded by Jigoro Kano in 1882, focusing on unarmed combat, grappling, and throwing techniques", "judo" });

            migrationBuilder.UpdateData(
                table: "GymClasses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "an Okinawan-originated, unarmed martial art focused on self-defense through striking techniques—including punching, kicking, knee/elbow strikes, and open-hand techniques");

            migrationBuilder.UpdateData(
                table: "GymClasses",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "a combat sport where two athletes, matched by weight, fight by landing punches with gloved fists while avoiding blows, typically in 3-12 rounds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "GymClasses");

            migrationBuilder.UpdateData(
                table: "GymClasses",
                keyColumn: "Id",
                keyValue: 2,
                column: "GymClassName",
                value: "Judo");
        }
    }
}
