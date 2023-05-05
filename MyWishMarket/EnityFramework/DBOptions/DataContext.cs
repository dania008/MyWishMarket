using Microsoft.EntityFrameworkCore;
using MyWishMarket.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWishMarket.EnityFramework.DBOptions
{
    public class DataContext : DbContext
    {
        public DataContext() : base() { }

        public DbSet<User> User { get; set; }
        public DbSet<Product> Product { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.ChatId).HasColumnName("ChatId");

                entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("Name");

                entity.Property(e => e.AppMode).HasMaxLength(50).HasColumnName("AppMode");

                entity.Property(e => e.AddWishHandlerMode).HasMaxLength(50).HasColumnName("AddWishHandlerMode");

                entity.Property(e => e.CurrentProductId).HasColumnName("CurrentProductId");

                entity.Property(e => e.Budget).HasColumnName("Budget");

                entity.Property(e => e.ShareCode).HasColumnName("ShareCode");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);

                entity.Property(e => e.Name).HasColumnName("Name");

                entity.Property(e => e.Description).HasColumnName("Description");

                entity.Property(e => e.Url).HasColumnName("Url");

                entity.Property(e => e.Price).HasColumnName("Price").HasColumnType("money");

                entity.Property(e => e.Image).HasColumnName("Image");

                entity.Property(e => e.Priority).HasColumnName("Priority");

                entity.Property(e => e.PurchaseStatus).HasColumnName("PurchaseStatus").HasColumnType("boolean");

                entity.Property(e => e.UserId).HasColumnName("UserId");
            });
        }
    }
}
