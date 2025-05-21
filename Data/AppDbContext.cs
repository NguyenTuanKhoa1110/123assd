using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using W3_test.Data.Entities;
using W3_test.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace W3_test.Data
{
	public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<BookEntity> Books { get; set; }
		public DbSet<CartEntity> Carts { get; set; }
		public DbSet<CartItemEntity> CartItems { get; set; }
		public DbSet<OrderEntity> Orders { get; set; }
		public DbSet<OrderItemEntity> OrderItems { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder); 

			
			modelBuilder.Entity<CartItemEntity>()
				.HasOne(ci => ci.Cart)
				.WithMany(c => c.Items)
				.HasForeignKey(ci => ci.CartId);

			modelBuilder.Entity<CartItemEntity>()
				.HasOne(ci => ci.Book)
				.WithMany(b => b.CartItems)
				.HasForeignKey(ci => ci.BookId);
			modelBuilder.Entity<CartItemEntity>()
				.Property(oi => oi.Price)
				.HasPrecision(18, 2);

			
			modelBuilder.Entity<OrderItemEntity>()
				.HasOne(oi => oi.Order)
				.WithMany(o => o.Items)
				.HasForeignKey(oi => oi.OrderId);
				
			modelBuilder.Entity<OrderItemEntity>()
				.HasOne(oi => oi.Book)
				.WithMany(b => b.OrderItems)
				.HasForeignKey(oi => oi.BookId);
			modelBuilder.Entity<OrderItemEntity>()
				.Property(oi => oi.Price)
				.HasPrecision(18, 2);
			modelBuilder.Entity<OrderEntity>()
				.Property(o => o.TotalAmount)
				.HasPrecision(18, 2);
			modelBuilder.Entity<BookEntity>()
			.Property(b => b.Price)
			.HasPrecision(18, 2);

            modelBuilder.Entity<RolePermission>()
				.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);


        }
    }
}