using Microsoft.EntityFrameworkCore;
using SrChauferoMVC_AzureIA.Models;

namespace SrChauferoMVC_AzureIA.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
        }

        public DbSet<Plato> Platos => Set<Plato>();
        public DbSet<Mesa> Mesas => Set<Mesa>();
        public DbSet<Insumo> Insumos => Set<Insumo>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<DetallePedido> DetallePedidos => Set<DetallePedido>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plato>()
                .Property(x => x.Precio)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Pedido>()
                .Property(x => x.Total)
                .HasPrecision(10, 2);

            modelBuilder.Entity<DetallePedido>()
                .Property(x => x.PrecioUnitario)
                .HasPrecision(10, 2);
        }
    }
}