using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixMessageAnalyzer.Migrations
{
    /// <inheritdoc />
    public partial class FixJsonbMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "fields",
                schema: "fix",
                table: "messages",
                newName: "Fields");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fields",
                schema: "fix",
                table: "messages",
                newName: "fields");
        }
    }
}
