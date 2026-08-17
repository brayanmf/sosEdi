using Scalar.AspNetCore;
using SOS.Data;
using SOS.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// 2. Configuración de Controladores y OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// 3. Configuración de Servicios de Logging
builder.Services.AddScoped<LoggerService>();

// 4. Configuración de Repositorios
builder.Services.AddTransient<IConexion>(_ => new SqlConexion(builder.Configuration.GetConnectionString("bd")));
builder.Services.AddScoped<AlertasRepository>(_ => new AlertasRepository(_.GetRequiredService<IConexion>()));
builder.Services.AddScoped<ConfirmacionesRepository>(_ => new ConfirmacionesRepository(_.GetRequiredService<IConexion>()));

// 5. Configuración del Servicio de Notificaciones OneSignal
builder.Services.AddScoped<OneSignalNotificationService>();

var app = builder.Build();

// 6. Configuración del Middleware para Desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => 
    {
        options.WithTitle("SOS API Reference");
        options.WithTheme(ScalarTheme.Mars);
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
