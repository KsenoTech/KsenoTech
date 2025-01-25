using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.Json;

namespace minesweeperAPI.ApplicationCore.DomModels
{
    public class MinesweeperContext : DbContext
    {
        public DbSet<Game> Games { get; set; }

        public MinesweeperContext(DbContextOptions<MinesweeperContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(e => e.GameId);

                entity.Property(e => e.Width).IsRequired();
                entity.Property(e => e.Height).IsRequired();
                entity.Property(e => e.MinesCount).IsRequired();
                entity.Property(e => e.Completed).IsRequired();

                // Сериализация Field в JSON
                entity.Property(e => e.Field)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<string[,]>(v, (JsonSerializerOptions)null));

                // Сериализация Mines в JSON
                entity.Property(e => e.Mines)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<bool[,]>(v, (JsonSerializerOptions)null));
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
