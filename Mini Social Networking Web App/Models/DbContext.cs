﻿using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Mini_Social_Networking_Web_App.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Gig> Gigs { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Followings> Followings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }


        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attendance>()
                .HasRequired(a => a.Gig)
                .WithMany(g => g.Attendances)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Followees)
                .WithRequired(f => f.Follower)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                 .HasMany(u => u.Followers)
                 .WithRequired(f => f.Followee)
                 .WillCascadeOnDelete(false);


            modelBuilder.Entity<UserNotification>()
                .HasRequired(u => u.User)
                .WithMany(u => u.UserNotifications)
                .WillCascadeOnDelete(false);




            base.OnModelCreating(modelBuilder);
        }
    }
}