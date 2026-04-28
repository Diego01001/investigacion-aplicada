using Microsoft.EntityFrameworkCore;
using GestionReservas.Data;
using GestionReservas.Services;

var builder = WebApplication.CreateBuilder(args);

// Agrega soporte para controladores y vistas MVC.
builder.Services.AddControllersWithViews();

// Configura Entity Framework Core para usar SQLite como base de datos.
// Toma la cadena de conexión desde appsettings.json.
// Si no existe, usa por defecto el archivo local reservas.db.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=reservas.db"));

// Registra la capa de servicios.
// Cuando el sistema necesite IReservaService, usará ReservaService.
builder.Services.AddScoped<IReservaService, ReservaService>();

var app = builder.Build();

// Configura el manejo de errores y seguridad cuando la aplicación no está en modo desarrollo.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Crea la base de datos si no existe.
// Esto permite que el sistema tenga las tablas necesarias al ejecutar la aplicación.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Crea la base de datos usando la configuración definida en AppDbContext.
    context.Database.EnsureCreated();
}

// Redirige las solicitudes HTTP a HTTPS.
app.UseHttpsRedirection();

// Permite servir archivos estáticos como CSS, JavaScript, imágenes y librerías de wwwroot.
app.UseStaticFiles();

// Activa el sistema de enrutamiento de ASP.NET Core.
app.UseRouting();

// Activa la autorización.
// En este proyecto no hay login, pero se mantiene como parte de la configuración estándar.
app.UseAuthorization();

// Define la ruta principal de la aplicación.
// Si el usuario entra a la raíz del sitio, se abrirá Reservas/Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reservas}/{action=Index}/{id?}");

// Inicia la aplicación web.
app.Run();