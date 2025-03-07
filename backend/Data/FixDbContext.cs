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
        public DbSet<User> Users { get; set; }

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

                entity.Property(e => e.FixVersion)
                    .HasColumnName("fix_version")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.MsgTypeName)
                    .HasColumnName("msg_type_name")
                    .HasMaxLength(100);

                // Financial data fields
                entity.Property(e => e.ClOrdID)
                    .HasColumnName("cl_ord_id")
                    .HasMaxLength(100);

                entity.Property(e => e.OrderID)
                    .HasColumnName("order_id")
                    .HasMaxLength(100);

                entity.Property(e => e.ExecID)
                    .HasColumnName("exec_id")
                    .HasMaxLength(100);

                entity.Property(e => e.Symbol)
                    .HasColumnName("symbol")
                    .HasMaxLength(50);

                entity.Property(e => e.SecurityType)
                    .HasColumnName("security_type")
                    .HasMaxLength(50);

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.OrderQty)
                    .HasColumnName("order_qty")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.LastQty)
                    .HasColumnName("last_qty")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.CumQty)
                    .HasColumnName("cum_qty")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.LeavesQty)
                    .HasColumnName("leaves_qty")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.Side)
                    .HasColumnName("side")
                    .HasMaxLength(10);

                entity.Property(e => e.OrdType)
                    .HasColumnName("ord_type")
                    .HasMaxLength(10);

                entity.Property(e => e.TimeInForce)
                    .HasColumnName("time_in_force")
                    .HasMaxLength(10);

                entity.Property(e => e.OrdStatus)
                    .HasColumnName("ord_status")
                    .HasMaxLength(10);

                entity.Property(e => e.Account)
                    .HasColumnName("account")
                    .HasMaxLength(50);

                entity.Property(e => e.TransactTime)
                    .HasColumnName("transact_time");

                // Validation status
                entity.Property(e => e.IsValid)
                    .HasColumnName("is_valid")
                    .HasDefaultValue(true);

                entity.Property(e => e.ValidationErrors)
                    .HasColumnName("validation_errors")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                        v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new List<string>()
                    );

                // Additional indexes
                entity.HasIndex(e => e.Symbol)
                    .HasDatabaseName("idx_messages_symbol");

                entity.HasIndex(e => e.ClOrdID)
                    .HasDatabaseName("idx_messages_cl_ord_id");

                entity.HasIndex(e => e.Side)
                    .HasDatabaseName("idx_messages_side");

                entity.HasIndex(e => e.OrdStatus)
                    .HasDatabaseName("idx_messages_ord_status");

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
                entity.HasIndex(e => e.Symbol)
                    .HasDatabaseName("idx_messages_symbol");
                entity.HasIndex(e => e.ClOrdID)
                    .HasDatabaseName("idx_messages_cl_ord_id");
                entity.HasIndex(e => e.Side)
                    .HasDatabaseName("idx_messages_side");
                entity.HasIndex(e => e.OrdStatus)
                    .HasDatabaseName("idx_messages_ord_status");

                entity.Property<string>("SessionId")
                    .HasComputedColumnSql("sender_comp_id || '-' || target_comp_id", true)
                    .HasColumnName("session_id")
                    .HasMaxLength(201);  // 100 (sender) + 1 (hyphen) + 100 (target)

                // Add an index on SessionId for faster session-based queries
                entity.HasIndex(e => e.SessionId)
                    .HasDatabaseName("idx_messages_session_id");

                // Add a foreign key to the Users table
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Messages)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
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

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.Property(e => e.LastLogin)
                    .HasColumnName("last_login");

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("idx_users_email");
            });
        }
    }
}