using Microsoft.EntityFrameworkCore;

namespace minesweeperAPI.ApplicationCore.DomModels
{
    public class MinesweeperContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        public MinesweeperContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public DbSet<Game> Games { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(e => e.GameId);

                entity.Property(e => e.Width).IsRequired();
                entity.Property(e => e.Height).IsRequired();
                entity.Property(e => e.MinesCount).IsRequired();
                entity.Property(e => e.Completed).IsRequired();

                // Сохраняем Field и Mines как строки JSON
                entity.Property(e => e.Field)
                    .HasColumnType("nvarchar(max)")
                    .IsRequired();

                entity.Property(e => e.Mines)
                    .HasColumnType("nvarchar(max)")
                    .IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
