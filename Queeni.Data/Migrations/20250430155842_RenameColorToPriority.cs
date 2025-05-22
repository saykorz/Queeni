using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Queeni.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameColorToPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Color",
                table: "Tasks",
                newName: "Priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "Tasks",
                newName: "Color");
        }
    }
}
