using Microsoft.EntityFrameworkCore;

namespace FinanceDataTool
{
    public class FinanceContext : DbContext
    {
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<Holding> Holdings => Set<Holding>();
        public DbSet<SystemInfo> SystemInfo => Set<SystemInfo>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=stocks.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Stocks: Symbol is the primary key rather than a conventional Id.
            modelBuilder.Entity<Stock>()
                .HasKey(s => s.Symbol);

            // Holding: table stays singular to match the existing schema.
            modelBuilder.Entity<Holding>()
                .ToTable("Holding");

            modelBuilder.Entity<Holding>()
                .HasKey(h => h.StockRecnum);

            // CurrentPrice is populated in memory, not stored in the Holding table.
            modelBuilder.Entity<Holding>()
                .Ignore(h => h.CurrentPrice);

            // FOREIGN KEY (Symbol) REFERENCES Stocks(Symbol).
            // Restrict matches the old default (NO ACTION); EF would otherwise cascade.
            modelBuilder.Entity<Holding>()
                .HasOne<Stock>()
                .WithMany()
                .HasForeignKey(h => h.Symbol)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SystemInfo>()
                .ToTable("System");

            // Replaces the INSERT OR REPLACE INTO System (Id, DbVersion) VALUES (1, 0)
            // that CreateSchema() used to run.
            modelBuilder.Entity<SystemInfo>()
                .HasData(new SystemInfo { Id = 1, DbVersion = 0 });
        }
    }
}
