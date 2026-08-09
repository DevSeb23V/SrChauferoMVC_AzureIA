using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SrChauferoMVC_AzureIA.Data;
using SrChauferoMVC_AzureIA.Models;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public UsuarioController(ApplicationDbContext db)
        {
            _db = db;
        }


        // ==========================
        // LISTAR
        // ==========================
        public IActionResult Index()
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
                return auth;


            var usuarios = _db.Usuarios
                .Include(x => x.Rol)
                .ToList();


            return View(usuarios);
        }



        // ==========================
        // CREAR GET
        // ==========================
        [HttpGet]
        public IActionResult Crear()
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
                return auth;


            CargarRoles();


            return View(new Usuario
            {
                Activo = true
            });
        }



        // ==========================
        // CREAR POST
        // ==========================
        [HttpPost]
        public IActionResult Crear(Usuario usuario)
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
                return auth;


            ModelState.Remove("Rol");


            if (ModelState.IsValid)
            {
                _db.Usuarios.Add(usuario);

                _db.SaveChanges();

                TempData["Ok"] = "Usuario creado correctamente.";

                return RedirectToAction(nameof(Index));
            }


            CargarRoles();

            return View(usuario);
        }



        // ==========================
        // EDITAR GET
        // ==========================
        [HttpGet]
        public IActionResult Editar(int id)
        {

            var auth = RequireAdmin();

            if (auth is not EmptyResult)
                return auth;



            var usuario =
                _db.Usuarios
                .Include(x => x.Rol)
                .FirstOrDefault(x => x.UsuarioId == id);



            if (usuario == null)
                return NotFound();



            CargarRoles();


            return View(usuario);
        }



        // ==========================
        // EDITAR POST
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Usuario usuario)
        {

            var auth = RequireAdmin();

            if (auth is not EmptyResult)
                return auth;



            if (ModelState.IsValid)
            {

                var usuarioBD = _db.Usuarios
                    .FirstOrDefault(x => x.UsuarioId == usuario.UsuarioId);



                if (usuarioBD == null)
                {
                    return NotFound();
                }



                usuarioBD.Nombre = usuario.Nombre;

                usuarioBD.NombreUsuario = usuario.NombreUsuario;

                usuarioBD.Correo = usuario.Correo;

                usuarioBD.RolId = usuario.RolId;

                usuarioBD.Activo = usuario.Activo;



                // Solo cambia contraseña si escribió una nueva
                if (!string.IsNullOrEmpty(usuario.Password))
                {
                    usuarioBD.Password = usuario.Password;
                }



                _db.SaveChanges();



                TempData["Ok"] =
                    "Usuario actualizado correctamente.";



                return RedirectToAction(nameof(Index));

            }



            CargarRoles();

            return View(usuario);

        }


        // ==========================
        // ELIMINAR POST
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarEliminar(int UsuarioId)
        {

            var auth = RequireAdmin();

            if (auth is not EmptyResult)
                return auth;



            var usuario =
                _db.Usuarios
                .FirstOrDefault(x => x.UsuarioId == UsuarioId);



            if (usuario != null)
            {
                _db.Usuarios.Remove(usuario);

                _db.SaveChanges();


                TempData["Ok"] =
                    "Usuario eliminado correctamente.";
            }



            return RedirectToAction(nameof(Index));
        }





        // ==========================
        // CARGAR ROLES
        // ==========================
        private void CargarRoles()
        {

            ViewBag.Roles =
                new SelectList(
                    _db.Roles.ToList(),
                    "RolId",
                    "Nombre"
                );

        }

    }
}