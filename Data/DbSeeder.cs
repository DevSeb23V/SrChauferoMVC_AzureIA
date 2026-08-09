using SrChauferoMVC_AzureIA.Models;

namespace SrChauferoMVC_AzureIA.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            // ==========================
            // CREAR ROLES
            // ==========================

            if (!db.Roles.Any())
            {
                db.Roles.AddRange(

                    new Rol
                    {
                        Nombre = "Administrador"
                    },

                    new Rol
                    {
                        Nombre = "Cocinero"
                    },

                    new Rol
                    {
                        Nombre = "Mozo"
                    },

                    new Rol
                    {
                        Nombre = "Cliente"
                    }

                );


                db.SaveChanges();
            }



            // ==========================
            // CREAR ADMIN
            // ==========================

            if (!db.Usuarios.Any())
            {

                var rolAdmin =
                    db.Roles
                    .First(x => x.Nombre == "Administrador");


                db.Usuarios.Add(
                    new Usuario
                    {
                        Nombre = "Administrador Sistema",
                        NombreUsuario = "admin",
                        Password = "Admin123",
                        Correo = "sebastianvalverde002@gmail.com",
                        RolId = rolAdmin.RolId,
                        Activo = true
                    }
                );


                db.SaveChanges();

            }

            if (!db.Platos.Any())
            {
                db.Platos.AddRange(
                    new Plato
                    {
                        Nombre = "Chaufa especial",
                        Categoria = "Arroces",
                        Precio = 18,
                        ImagenUrl = "https://images.unsplash.com/photo-1603133872878-684f208fb84b?q=80&w=800",
                        Disponible = true
                    },
                    new Plato
                    {
                        Nombre = "Aeropuerto",
                        Categoria = "Criollo",
                        Precio = 20,
                        ImagenUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19?q=80&w=800",
                        Disponible = true
                    },
                    new Plato
                    {
                        Nombre = "Pollo saltado",
                        Categoria = "Saltados",
                        Precio = 17,
                        ImagenUrl = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?q=80&w=800",
                        Disponible = true
                    }
                );
            }

            if (!db.Mesas.Any())
            {
                for (int i = 1; i <= 8; i++)
                {
                    db.Mesas.Add(new Mesa
                    {
                        Numero = i,
                        Estado = "Disponible",
                        Capacidad = 4,
                        Cliente = null,
                        Personas = null,
                        HoraIngreso = null
                    });
                }
            }

            db.SaveChanges();
        }
    }
}