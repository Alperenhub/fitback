using fitback.Models;
using Microsoft.EntityFrameworkCore;

namespace fitback.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainerCode> TrainerCodes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Unique kod
            modelBuilder.Entity<TrainerCode>()
                .HasIndex(c => c.Code)
                .IsUnique();
            //Trainer-TrainerCode relation (1-1)
            modelBuilder.Entity<Trainer>()
         .HasMany(t => t.TrainerCodes)
         .WithOne(tc => tc.Trainer)
         .HasForeignKey(tc => tc.TrainerId);      //Trainer- Students (1-n)
            modelBuilder.Entity<Trainer>()
                .HasMany(t => t.Students)
                .WithOne(s => s.Trainer)
                .HasForeignKey(s => s.TrainerId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
