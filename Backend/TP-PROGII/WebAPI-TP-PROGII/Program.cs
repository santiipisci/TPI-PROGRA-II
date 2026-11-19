using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using tpRepository.Models;
using tpRepository.Repository.Implementations;
using tpRepository.Repository.Interfaces;
using tpRepository.Security;
using tpRepository.Services.Implementations;
using tpRepository.Services.Interfaces;
using WebAPI_TP_PROGII.Middleware;
using WebAPI_TP_PROGII.Security;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<AutopartesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inyecciones
builder.Services.AddScoped<IAutopartesRepository, AutopartesRepository>();
builder.Services.AddScoped<IAutopartesService, AutopartesService>();

builder.Services.AddScoped<IModelosRepository, ModelosRepository>();
builder.Services.AddScoped<IModelosService, ModelosService>();

builder.Services.AddScoped<ICatalogosRepository, CatalogosRepository>();
builder.Services.AddScoped<ICatalogosService, CatalogosService>();

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IOrdenesRepository, OrdenesRepository>();
builder.Services.AddScoped<IOrdenesService, OrdenesService>();

// CORS: el front (HTML/CSS/JS) se sirve desde otro origen y usa cookies.
// Los orígenes se configuran en appsettings.json (sección "Cors:Origenes").
var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? Array.Empty<string>();
var esDesarrollo = builder.Environment.IsDevelopment();
builder.Services.AddCors(options => options.AddPolicy("Front", policy => policy
    .SetIsOriginAllowed(origen =>
        esDesarrollo ||
        origen == "null" ||
        origenes.Contains(origen, StringComparer.OrdinalIgnoreCase))
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

static void ConfigurarCookie(CookieAuthenticationOptions o, string nombre)
{
    o.Cookie.Name = nombre;
    o.Cookie.HttpOnly = true;
    // SameSite=None + Secure: el front y la API están en orígenes distintos.
    o.Cookie.SameSite = SameSiteMode.None;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.SlidingExpiration = true;
    // La API responde 401/403 en vez de redirigir a una página de login.
    o.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    o.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
}

builder.Services.AddAuthentication(AuthSchemes.Admin)
    .AddCookie(AuthSchemes.Admin, o => ConfigurarCookie(o, "mx.admin"))
    .AddCookie(AuthSchemes.Cliente, o => ConfigurarCookie(o, "mx.cliente"));

builder.Services.AddAuthorization();

var app = builder.Build();

// Errores de negocio → 400/401/404/409 con { mensaje }
app.UseMiddleware<ApiExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
var frontPath = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath, "..", "..", "..",
    "Frontend", "AutopartesFront", "autopartes"));

if (Directory.Exists(frontPath))
{
    var front = new PhysicalFileProvider(frontPath);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = front });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = front });
}

app.UseCors("Front");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Primer administrador: la clave se guarda hasheada, así que no se puede insertar desde SQL.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AutopartesDbContext>();
        if (!await db.UsuarioAdmins.AnyAsync())
        {
            var usuario = builder.Configuration["AdminSeed:Usuario"] ?? "admin";
            var clave = builder.Configuration["AdminSeed:Clave"] ?? "admin123";

            db.UsuarioAdmins.Add(new UsuarioAdmin
            {
                Usuario = usuario,
                ClaveHash = PasswordHasher.Hash(clave),
                Nombre = "Administrador",
                Activo = true,
            });
            await db.SaveChangesAsync();

            app.Logger.LogInformation("Administrador inicial creado: {Usuario}", usuario);
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex,
            "No se pudo verificar/crear el administrador inicial. Revisá que la base exista y la cadena de conexión.");
    }
}

app.Run();
