using Kalium.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kalium.Server.Context
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUser>().ToTable("User");
            modelBuilder.Entity<IdentityRole>().ToTable("Role");
//            modelBuilder.Entity<Product>().HasIndex(p => p.NameUrl);
//            modelBuilder.Entity<IdentityUser>().HasIndex(u => u.UserName);
//            modelBuilder.Entity<Category>().HasIndex(c => c.Name);
//            modelBuilder.Entity<Category>().HasIndex(c => c.Name);

            /*
            modelBuilder.Ignore<IdentityUserClaim<string>>();
            modelBuilder.Ignore<IdentityUserLogin<string>>();
            modelBuilder.Ignore<IdentityRoleClaim<string>>();
            */
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Auction> Auction { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Coupon> Coupon { get; set; }
        public DbSet<Discussion> Discussion { get; set; }
        public DbSet<Extra> Extra { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<Refund> Refund { get; set; }
        public DbSet<Review> Review { get; set; }
    }
}
