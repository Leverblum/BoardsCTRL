using BoardsProject.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardsProject.Data
{
    public class BoardsContext : DbContext
    {
        public BoardsContext(DbContextOptions<BoardsContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.roleId);

            modelBuilder.Entity<Board>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Boards)
                .HasForeignKey(b => b.categoryId);

            modelBuilder.Entity<Board>()
                .HasMany(b => b.Slides)
                .WithOne(s => s.Board)
                .HasForeignKey(s => s.boardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Slide>()
                .HasOne(s => s.Board)
                .WithMany(b => b.Slides)
                .HasForeignKey(s => s.boardId);
        }
    }
}
