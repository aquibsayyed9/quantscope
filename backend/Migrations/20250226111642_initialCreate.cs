using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FixMessageAnalyzer.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fix");

            migrationBuilder.CreateTable(
                name: "connectors",
                schema: "fix",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    configuration = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_connected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connectors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "fix",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    msg_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    sender_comp_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    target_comp_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    session_id = table.Column<string>(type: "character varying(201)", maxLength: 201, nullable: false, computedColumnSql: "sender_comp_id || '-' || target_comp_id", stored: true),
                    fields = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExecType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_connectors_is_active",
                schema: "fix",
                table: "connectors",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_connectors_type",
                schema: "fix",
                table: "connectors",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "idx_messages_msg_type",
                schema: "fix",
                table: "messages",
                column: "msg_type");

            migrationBuilder.CreateIndex(
                name: "idx_messages_sender",
                schema: "fix",
                table: "messages",
                column: "sender_comp_id");

            migrationBuilder.CreateIndex(
                name: "idx_messages_session_id",
                schema: "fix",
                table: "messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "idx_messages_target",
                schema: "fix",
                table: "messages",
                column: "target_comp_id");

            migrationBuilder.CreateIndex(
                name: "idx_messages_timestamp",
                schema: "fix",
                table: "messages",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "connectors",
                schema: "fix");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "fix");
        }
    }
}
