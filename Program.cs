using Scalar.AspNetCore;
using SOS.Data;

var builder = WebApplication.CreateBuilder(args); // Usamos el Builder estándar

// 1. Configuración de Controladores y OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(); 

 
// 3. Configuración de Repositorios
 
builder.Services.AddTransient<IConexion>(_ => new SqlConexion(builder.Configuration.GetConnectionString("bd")));
builder.Services.AddScoped<AlertasRepository>(_ => new AlertasRepository(_.GetRequiredService<IConexion>()));
builder.Services.AddScoped<ConfirmacionesRepository>(_ => new ConfirmacionesRepository(_.GetRequiredService<IConexion>()));

var app = builder.Build();

// 4. Configuración del Middleware para Desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Genera el JSON en /openapi/v1.json
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