using Microsoft.EntityFrameworkCore;
using SrChauferoMVC_AzureIA.Models;

namespace SrChauferoMVC_AzureIA.Data
{
    public class ApplicationDbContext : DbContext
    {
        // ==========================================
        // CONSTRUCTOR
        // ==========================================
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
        }

        // ==========================================
        // TABLAS DE LA BASE DE DATOS
        // ==========================================

        public DbSet<Plato> Platos => Set<Plato>();

        public DbSet<Mesa> Mesas => Set<Mesa>();

        public DbSet<Insumo> Insumos => Set<Insumo>();

        public DbSet<Pedido> Pedidos => Set<Pedido>();
    }
}