using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FixMessageAnalyzer.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_id",
                schema: "fix",
                table: "messages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "users",
                schema: "fix",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_login = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_messages_user_id",
                schema: "fix",
                table: "messages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                schema: "fix",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_messages_users_user_id",
                schema: "fix",
                table: "messages",
                column: "user_id",
                principalSchema: "fix",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_messages_users_user_id",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropTable(
                name: "users",
                schema: "fix");

            migrationBuilder.DropIndex(
                name: "IX_messages_user_id",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "fix",
                table: "messages");
        }
    }
}
