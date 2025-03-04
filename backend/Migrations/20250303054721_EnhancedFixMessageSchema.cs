using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FixMessageAnalyzer.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedFixMessageSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "fix",
                table: "messages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "account",
                schema: "fix",
                table: "messages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cl_ord_id",
                schema: "fix",
                table: "messages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cum_qty",
                schema: "fix",
                table: "messages",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "exec_id",
                schema: "fix",
                table: "messages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fix_version",
                schema: "fix",
                table: "messages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_valid",
                schema: "fix",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "last_qty",
                schema: "fix",
                table: "messages",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "leaves_qty",
                schema: "fix",
                table: "messages",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "msg_type_name",
                schema: "fix",
                table: "messages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ord_status",
                schema: "fix",
                table: "messages",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ord_type",
                schema: "fix",
                table: "messages",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "order_id",
                schema: "fix",
                table: "messages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "order_qty",
                schema: "fix",
                table: "messages",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                schema: "fix",
                table: "messages",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "security_type",
                schema: "fix",
                table: "messages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "side",
                schema: "fix",
                table: "messages",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "symbol",
                schema: "fix",
                table: "messages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "time_in_force",
                schema: "fix",
                table: "messages",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "transact_time",
                schema: "fix",
                table: "messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "validation_errors",
                schema: "fix",
                table: "messages",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "session_id",
                schema: "fix",
                table: "messages",
                type: "character varying(201)",
                maxLength: 201,
                nullable: true,
                computedColumnSql: "sender_comp_id || '-' || target_comp_id",
                stored: true,
                oldClrType: typeof(string),
                oldType: "character varying(201)",
                oldMaxLength: 201,
                oldComputedColumnSql: "sender_comp_id || '-' || target_comp_id",
                oldStored: true);

            migrationBuilder.CreateIndex(
                name: "idx_messages_cl_ord_id",
                schema: "fix",
                table: "messages",
                column: "cl_ord_id");

            migrationBuilder.CreateIndex(
                name: "idx_messages_ord_status",
                schema: "fix",
                table: "messages",
                column: "ord_status");

            migrationBuilder.CreateIndex(
                name: "idx_messages_side",
                schema: "fix",
                table: "messages",
                column: "side");

            migrationBuilder.CreateIndex(
                name: "idx_messages_symbol",
                schema: "fix",
                table: "messages",
                column: "symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_messages_cl_ord_id",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "idx_messages_ord_status",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "idx_messages_side",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "idx_messages_symbol",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "account",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "cl_ord_id",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "cum_qty",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "exec_id",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "fix_version",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "is_valid",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "last_qty",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "leaves_qty",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "msg_type_name",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "ord_status",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "ord_type",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "order_id",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "order_qty",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "price",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "security_type",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "side",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "symbol",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "time_in_force",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "transact_time",
                schema: "fix",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "validation_errors",
                schema: "fix",
                table: "messages");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "fix",
                table: "messages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "session_id",
                schema: "fix",
                table: "messages",
                type: "character varying(201)",
                maxLength: 201,
                nullable: false,
                computedColumnSql: "sender_comp_id || '-' || target_comp_id",
                stored: true,
                oldClrType: typeof(string),
                oldType: "character varying(201)",
                oldMaxLength: 201,
                oldNullable: true,
                oldComputedColumnSql: "sender_comp_id || '-' || target_comp_id",
                oldStored: true);
        }
    }
}
