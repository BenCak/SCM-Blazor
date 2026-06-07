using Microsoft.EntityFrameworkCore;
using SCM3.Data.Entities;

namespace SCM3.Data.DbContext;

public class SCM3DbContext(DbContextOptions<SCM3DbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<SystemEntity> Systems => Set<SystemEntity>();
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
    public DbSet<RequestStatus> RequestStatuses => Set<RequestStatus>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<RequestAttributes> RequestAttributes => Set<RequestAttributes>();
    public DbSet<RequestSCMStatus> RequestSCMStatuses => Set<RequestSCMStatus>();
    public DbSet<RequestNote> RequestNotes => Set<RequestNote>();
    public DbSet<RequestAttachment> RequestAttachments => Set<RequestAttachment>();
    public DbSet<RequestHistory> RequestHistory => Set<RequestHistory>();
    public DbSet<RequestChangeLog> RequestChangeLogs => Set<RequestChangeLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // EF Core's PK convention looks for "SystemEntityId"/"Id" — it won't infer SystemId
        // from a class named SystemEntity, so the key must be declared explicitly.
        modelBuilder.Entity<SystemEntity>().HasKey(s => s.SystemId);
        modelBuilder.Entity<SystemEntity>()
            .HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId);
        modelBuilder.Entity<SystemEntity>()
            .HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.RequestType).WithMany().HasForeignKey(r => r.RequestTypeId);
        modelBuilder.Entity<Request>()
            .HasOne(r => r.RequestStatus).WithMany().HasForeignKey(r => r.RequestStatusId);
        modelBuilder.Entity<Request>()
            .HasOne(r => r.System).WithMany().HasForeignKey(r => r.SystemId);
        modelBuilder.Entity<Request>()
            .HasOne(r => r.ParentRequest).WithMany().HasForeignKey(r => r.ParentRequestId);
        modelBuilder.Entity<Request>()
            .HasOne(r => r.Requestor).WithMany().HasForeignKey(r => r.RequestorUserId);

        modelBuilder.Entity<RequestAttributes>()
            .HasOne(ra => ra.Request).WithOne().HasForeignKey<RequestAttributes>(ra => ra.RequestId);

        modelBuilder.Entity<RequestSCMStatus>()
            .HasOne(s => s.Request).WithMany().HasForeignKey(s => s.RequestId);
        modelBuilder.Entity<RequestSCMStatus>()
            .HasOne(s => s.AssignedTo).WithMany().HasForeignKey(s => s.AssignedToUserId);

        modelBuilder.Entity<RequestNote>()
            .HasOne(n => n.Request).WithMany().HasForeignKey(n => n.RequestId);
        modelBuilder.Entity<RequestNote>()
            .HasOne(n => n.Author).WithMany().HasForeignKey(n => n.AuthorUserId);

        modelBuilder.Entity<RequestAttachment>()
            .HasOne(a => a.Request).WithMany().HasForeignKey(a => a.RequestId);
        modelBuilder.Entity<RequestAttachment>()
            .HasOne(a => a.UploadedBy).WithMany().HasForeignKey(a => a.UploadedByUserId);

        modelBuilder.Entity<RequestHistory>()
            .HasOne(h => h.Request).WithMany().HasForeignKey(h => h.RequestId);
        modelBuilder.Entity<RequestHistory>()
            .HasOne(h => h.FromStatus).WithMany().HasForeignKey(h => h.FromStatusId);
        modelBuilder.Entity<RequestHistory>()
            .HasOne(h => h.ToStatus).WithMany().HasForeignKey(h => h.ToStatusId);
        modelBuilder.Entity<RequestHistory>()
            .HasOne(h => h.ActionBy).WithMany().HasForeignKey(h => h.ActionByUserId);

        modelBuilder.Entity<RequestChangeLog>()
            .HasOne(c => c.Request).WithMany().HasForeignKey(c => c.RequestId);
        modelBuilder.Entity<RequestChangeLog>()
            .HasOne(c => c.ChangedBy).WithMany().HasForeignKey(c => c.ChangedByUserId);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Recipient).WithMany().HasForeignKey(n => n.RecipientUserId);
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Request).WithMany().HasForeignKey(n => n.RequestId);

        // Soft delete (IsDeleted) is the only delete path the app uses (root CLAUDE.md §2),
        // so every FK is Restrict — this also avoids "multiple cascade paths" errors that
        // the many converging User/Request relationships below would otherwise trigger.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
