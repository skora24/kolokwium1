using kolos1.Services;

var builder = WebApplication.CreateBuilder(args);

// Rejestracja serwisów w DI
builder.Services.AddScoped<IVisitService, VisitService>();

// Dodanie kontrolerów
builder.Services.AddControllers();

var app = builder.Build();

// Konfiguracja potoku HTTP
app.UseAuthorization();
app.MapControllers();
app.Run();

