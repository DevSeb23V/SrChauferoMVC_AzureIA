using Microsoft.EntityFrameworkCore;
using SrChauferoMVC_AzureIA.Data;
using SrChauferoMVC_AzureIA.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// SERVICIOS MVC
// ==========================================
builder.Services.AddControllersWithViews();

// ==========================================
// CONFIGURACIÓN DE BASE DE DATOS
// ==========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ==========================================
// CONFIGURACIÓN DE SESSION
// ==========================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================
// SERVICIO DE INTELIGENCIA ARTIFICIAL
// ==========================================
builder.Services.AddHttpClient<IIAService, AzureOpenAIService>();

var app = builder.Build();

// ==========================================
// CREACIÓN Y CARGA INICIAL DE BASE DE DATOS
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        db.Database.EnsureCreated();
        DbSeeder.Seed(db);
    }
    catch
    {
        // Evita que el sistema se detenga si hay error de conexión inicial
    }
}

// ==========================================
// CONFIGURACIÓN DEL PIPELINE HTTP
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// ==========================================
// RUTA PRINCIPAL MVC
// ==========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();