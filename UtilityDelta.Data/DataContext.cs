using Microsoft.EntityFrameworkCore;

namespace UtilityDelta.Data
{
    public class DataContext : DbContext
    {
        private readonly string _databaseFilePath;

        public DataContext(string databaseFilePath)
        {
            _databaseFilePath = databaseFilePath;
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //https://docs.microsoft.com/en-us/ef/core/modeling/relationships
            modelBuilder.Entity<Tag>()
                .HasOne(p => p.Blog)
                .WithMany(b => b.Tags)
                .HasForeignKey(p => p.BlogId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=" + _databaseFilePath);
        }
    }
}