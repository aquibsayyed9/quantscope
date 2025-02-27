﻿// <auto-generated />
using System;
using System.Text.Json;
using FixMessageAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FixMessageAnalyzer.Migrations
{
    [DbContext(typeof(FixDbContext))]
    partial class FixDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("fix")
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FixMessageAnalyzer.Data.Entities.Connector", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<JsonDocument>("Configuration")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("configuration");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("ErrorMessage")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("error_message");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("is_active");

                    b.Property<DateTime?>("LastConnectedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_connected_at");

                    b.Property<DateTime?>("LastModifiedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_modified_at");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("status");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.HasIndex("IsActive")
                        .HasDatabaseName("idx_connectors_is_active");

                    b.HasIndex("Type")
                        .HasDatabaseName("idx_connectors_type");

                    b.ToTable("connectors", "fix");
                });

            modelBuilder.Entity("FixMessageAnalyzer.Data.Entities.FixMessage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("ExecType")
                        .HasColumnType("text");

                    b.Property<string>("Fields")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("MsgType")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("msg_type");

                    b.Property<string>("SenderCompID")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("sender_comp_id");

                    b.Property<int>("SequenceNumber")
                        .HasColumnType("integer")
                        .HasColumnName("sequence_number");

                    b.Property<string>("SessionId")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(201)
                        .HasColumnType("character varying(201)")
                        .HasColumnName("session_id")
                        .HasComputedColumnSql("sender_comp_id || '-' || target_comp_id", true);

                    b.Property<string>("TargetCompID")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("target_comp_id");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.HasKey("Id");

                    b.HasIndex("MsgType")
                        .HasDatabaseName("idx_messages_msg_type");

                    b.HasIndex("SenderCompID")
                        .HasDatabaseName("idx_messages_sender");

                    b.HasIndex("SessionId")
                        .HasDatabaseName("idx_messages_session_id");

                    b.HasIndex("TargetCompID")
                        .HasDatabaseName("idx_messages_target");

                    b.HasIndex("Timestamp")
                        .HasDatabaseName("idx_messages_timestamp");

                    b.ToTable("messages", "fix");
                });
#pragma warning restore 612, 618
        }
    }
}
