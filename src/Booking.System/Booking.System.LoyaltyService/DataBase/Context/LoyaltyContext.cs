using Booking.System.LoyaltyService.DataBase.Models;
using Booking.System.LoyaltyService.DataBase.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.LoyaltyService.DataBase.Context;


public class LoyaltyContext(DbContextOptions<LoyaltyContext> options) : DbContext(options)
{
    public DbSet<DbLoyalty> Loyalties { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbLoyalty>(entity =>
        {
            entity.ToTable("loyalty"); 
        
            entity.HasIndex(p => p.Username)
                .IsUnique();
            
            entity.Property(p => p.Id)
                .HasColumnName("id"); 
            
            entity.Property(p => p.Username)
                .HasColumnName("username");
            
            entity.Property(p => p.Status)
                .HasConversion<string>()
                .HasDefaultValue(LoyaltyStatus.BRONZE)
                .HasColumnName("status");
            
            entity.Property(p => p.Discount)
                .HasColumnName("discount");
            
            entity.Property(p => p.ReservationCount)
                .HasColumnName("reservation_count");
        });
        
        base.OnModelCreating(modelBuilder);
    }
}