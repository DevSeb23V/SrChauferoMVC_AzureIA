using SrChauferoMVC_AzureIA.Models;

namespace SrChauferoMVC_AzureIA.Data
{
    public static class DbSeeder
    {
        // ==========================================
        // CARGA DE DATOS INICIALES
        // ==========================================
        public static void Seed(ApplicationDbContext db)
        {
            // ==========================================
            // PLATOS
            // ==========================================
            if (!db.Platos.Any())
            {
                db.Platos.AddRange(
                    new Plato
                    {
                        Nombre = "Chaufa especial",
                        Categoria = "Arroces",
                        Precio = 18,
                        ImagenUrl = "https://images.unsplash.com/photo-1603133872878-684f208fb84b?q=80&w=800"
                    },
                    new Plato
                    {
                        Nombre = "Aeropuerto",
                        Categoria = "Criollo",
                        Precio = 20,
                        ImagenUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19?q=80&w=800"
                    },
                    new Plato
                    {
                        Nombre = "Pollo saltado",
                        Categoria = "Saltados",
                        Precio = 17,
                        ImagenUrl = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?q=80&w=800"
                    }
                );
            }

            // ==========================================
            // MESAS
            // ==========================================
            if (!db.Mesas.Any())
            {
                for (int i = 1; i <= 8; i++)
                {
                    db.Mesas.Add(new Mesa
                    {
                        Numero = i,
                        Estado = (i == 3 || i == 6)
                            ? "Ocupada"
                            : "Libre",

                        Capacidad = 4,

                        Cliente = i == 3
                            ? "Carlos Ruiz"
                            : i == 6
                                ? "María López"
                                : null,

                        Personas = i == 3
                            ? 4
                            : i == 6
                                ? 2
                                : null,

                        HoraIngreso = (i == 3 || i == 6)
                            ? DateTime.Now.AddMinutes(-35)
                            : null
                    });
                }
            }

            // ==========================================
            // PEDIDOS
            // ==========================================
            if (!db.Pedidos.Any())
            {
                db.Pedidos.AddRange(
                    new Pedido
                    {
                        Cliente = "Mesa 1",
                        Mesa = 1,
                        Total = 38
                    },
                    new Pedido
                    {
                        Cliente = "Mesa 3",
                        Mesa = 3,
                        Total = 20,
                        Estado = "Pagado"
                    }
                );
            }

            // ==========================================
            // GUARDAR CAMBIOS
            // ==========================================
            db.SaveChanges();
        }
    }
}