using FixMessageAnalyzer.Data.Entities;
using FixMessageAnalyzer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FixMessageAnalyzer.Data
{
    public class FixDbContext : DbContext
    {
        public FixDbContext(DbContextOptions<FixDbContext> options)
            : base(options)
        {
        }

        public DbSet<FixMessage> Messages { get; set; }
        public DbSet<Connector> Connectors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("fix");

            modelBuilder.Entity<FixMessage>(entity =>
            {
                entity.ToTable("messages");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp")
                    .IsRequired();

                entity.Property(e => e.MsgType)
                    .HasColumnName("msg_type")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(e => e.SequenceNumber)
                    .HasColumnName("sequence_number")
                    .IsRequired();

                entity.Property(e => e.SenderCompID)
                    .HasColumnName("sender_comp_id")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.TargetCompID)
                    .HasColumnName("target_comp_id")
                    .HasMaxLength(100)
                    .IsRequired();

                //entity.Property(e => e.Fields)
                //    .HasColumnName("fields")
                //    .HasColumnType("jsonb")
                //    .IsRequired();

                entity.Property(e => e.Fields)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new Dictionary<string, string>()
                );

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();
                               

                // Add indexes for common queries
                entity.HasIndex(e => e.Timestamp)
                    .HasDatabaseName("idx_messages_timestamp");
                entity.HasIndex(e => e.MsgType)
                    .HasDatabaseName("idx_messages_msg_type");
                entity.HasIndex(e => e.SenderCompID)
                    .HasDatabaseName("idx_messages_sender");
                entity.HasIndex(e => e.TargetCompID)
                    .HasDatabaseName("idx_messages_target");

                entity.Property<string>("SessionId")
                    .HasComputedColumnSql("sender_comp_id || '-' || target_comp_id", true)
                    .HasColumnName("session_id")
                    .HasMaxLength(201);  // 100 (sender) + 1 (hyphen) + 100 (target)

                // Add an index on SessionId for faster session-based queries
                entity.HasIndex(e => e.SessionId)
                    .HasDatabaseName("idx_messages_session_id");
            });

            modelBuilder.Entity<Connector>(entity =>
            {
                entity.ToTable("connectors");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.Property(e => e.Configuration)
                    .HasColumnName("configuration")
                    .HasColumnType("jsonb")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.Property(e => e.LastModifiedAt)
                    .HasColumnName("last_modified_at");

                entity.Property(e => e.LastConnectedAt)
                    .HasColumnName("last_connected_at");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(50);

                entity.Property(e => e.ErrorMessage)
                    .HasColumnName("error_message")
                    .HasMaxLength(500);

                // Add indexes
                entity.HasIndex(e => e.Type)
                    .HasDatabaseName("idx_connectors_type");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("idx_connectors_is_active");
            });
        }
    }
}