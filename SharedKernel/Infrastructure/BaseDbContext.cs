using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedKernel.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Infrastructure;

public abstract class BaseDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _schema;

    protected BaseDbContext(
        DbContextOptions options,
         IHttpContextAccessor httpContextAccessor,
         string schema = "dbo"
    ) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _schema = schema;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(_schema);
        base.OnModelCreating(builder);
        var entityTypes = builder.Model
       .GetEntityTypes()
       .Where(t => typeof(AuditableBaseEntity)
           .IsAssignableFrom(t.ClrType));

        foreach (var entityType in entityTypes)
        {
            builder.Entity(entityType.ClrType)
                .HasIndex(nameof(AuditableBaseEntity.created));

            builder.Entity(entityType.ClrType)
                .HasIndex(nameof(AuditableBaseEntity.updated));

            builder.Entity(entityType.ClrType)
                .HasIndex(nameof(AuditableBaseEntity.created_by));

            builder.Entity(entityType.ClrType)
                .HasIndex(nameof(AuditableBaseEntity.updated_by));
        }
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAudit()
    {
        var userName = GetUserId();

        foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.created = DateTime.UtcNow;
                entry.Entity.updated = DateTime.UtcNow;
                entry.Entity.created_by = userName;
                entry.Entity.updated_by = userName;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.updated = DateTime.UtcNow;
                entry.Entity.updated_by = userName; 
            }
        }
    }
    private Guid? GetUserId()
    {
        var userId = _httpContextAccessor?
            .HttpContext?
            .User?
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        return Guid.TryParse(userId, out var id) ? id : null;
    }
}