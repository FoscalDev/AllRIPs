using AllRIPs.FEV;
using AllRIPs.INTERFACES;
using AllRIPs.SERVICES;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

var configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

Log.Logger = new LoggerConfiguration()
           .ReadFrom.Configuration(configuration)
           .CreateLogger();

var misReglasCors = "ReglasCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: misReglasCors,
        builder =>
        {
            builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        }
    );
});

builder.Services.AddScoped<RipsService>();
builder.Services.AddScoped<ServicesFEV>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<MongoService>();
builder.Services.AddScoped<SapService>();

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueueService>();
builder.Services.AddHostedService<QueuedHostedService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rips",
        Version = "v1",
        Description = "EndPoints relacionados con RIPs & SAP",
        Contact = new OpenApiContact
        {
            Name = "Ing. Robinson E. Gomez",
            Email = "desarrollador.senior4@foscal.com.co"
        }
    });
});

builder.Configuration.AddJsonFile("appsettings.json");

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2147483647; // 2GB
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rips v1");
        c.RoutePrefix = string.Empty;
    });
}

// Middleware para capturar métricas HTTP
app.UseHttpMetrics();

// Endpoint para Prometheus
app.MapMetrics();
app.UseCors(misReglasCors);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

