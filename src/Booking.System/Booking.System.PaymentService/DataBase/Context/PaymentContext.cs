using Booking.System.PaymentService.DataBase.Models;
using Booking.System.PaymentService.DataBase.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.PaymentService.DataBase.Context;

public class PaymentContext(DbContextOptions<PaymentContext> options) : DbContext(options)
{
    public DbSet<DbPayment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbPayment>(entity =>
        {
            entity.ToTable("payment"); 

            entity.Property(p => p.Id)
                .HasColumnName("id"); 
            
            entity.Property(p => p.PaymentUid)
                .HasColumnName("payment_uid");
            
            entity.Property(p => p.PaymentStatus)
                .HasConversion<string>()
                .HasDefaultValue(DbPaymentStatus.PAID)
                .HasColumnName("status");
            
            entity.Property(p => p.Price)
                .HasColumnName("price");
        });
        
        base.OnModelCreating(modelBuilder);
    }
}
