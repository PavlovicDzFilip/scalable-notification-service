using DbUp.Engine.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Infrastructure;

public class NotificationContext(DbContextOptions<NotificationContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationLog> NotificationLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var notification = modelBuilder.Entity<Notification>();
        notification.ToTable("Notifications");
        notification.HasKey(x => x.Id);
        notification.HasIndex(x => x.SendDate, "IX_SendDate");
        notification.Property(x => x.Payload)
            .HasMaxLength(1000);

        base.OnModelCreating(modelBuilder);
    }
}
