﻿using Grains.Models;
using Microsoft.EntityFrameworkCore;

namespace Grains
{
    public class RegistryContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserInfo>().HasKey(_ => _.Id);
            modelBuilder.Entity<UserInfo>().HasIndex(_ => _.Handle).IsUnique();

            modelBuilder.Entity<Message>().HasKey(_ => _.Id);
            modelBuilder.Entity<Message>().HasIndex(_ => new { _.PublisherId, _.Timestamp });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<UserInfo> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}