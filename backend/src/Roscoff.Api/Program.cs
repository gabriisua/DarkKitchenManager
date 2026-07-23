using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Roscoff.Core.Interfaces;
using Roscoff.Infrastructure.Data;
using Roscoff.Infrastructure.Services;
using System.Text;
using Roscoff.Api.Extensions;
using Roscoff.Application.Wrappers;
using Roscoff.Api.HttpService;
using Roscoff.Application.Interfaces;
using Roscoff.Infrastructure.Helpers;
using Hangfire;
using Hangfire.SqlServer;
using Roscoff.Infrastructure.Services.Jobs;
using Roscoff.Core.Entities.Client;

var builder = WebApplication.CreateBuilder(args);

var frontendOrigins = new[] { "http://localhost:4200", "http://localhost" };

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<FattureInCloudSettings>(
    builder.Configuration.GetSection("FattureInCloudSettings"));

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings mancanti");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key)
                    ),
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSwaggerExtension();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IRoscoffDbContext>(provider => 
    provider.GetRequiredService<RoscoffDbContext>());

builder.Services.AddDbContext<RoscoffDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero, 
        UseRecommendedIsolationLevel = true,
    }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2; 
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IPlateService, PlateService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IFoodCostService, FoodCostService>();
builder.Services.AddScoped<INutritionService, NutritionService>();
builder.Services.AddScoped<IAllergenService, AllergenService>();
builder.Services.AddScoped<IDeliveryHubService, DeliveryHubService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IClientDiscountService, ClientDiscountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IWorkingDayCalculator, WorkingDayCalculator>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInvoiceManagerService, InvoiceManagerService>();
builder.Services.AddSingleton<IPdfEngineService, PdfEngineService>();
builder.Services.AddScoped<IZplPrintService, TcpZplPrintService>();
builder.Services.AddScoped<IPrinterService, PrinterService>();
builder.Services.AddScoped<InvoiceProcessingJob>();

builder.Services.AddHttpClient<IFattureInCloudService, FattureInCloudService>();

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Roscoff.Application.DependencyInjection).Assembly));

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(frontendOrigins) 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();         
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Roscoff Background Jobs"
});

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<RoscoffDbContext>();

    int retries = 10;
    while (retries > 0)
    {
        try
        {
            // 1. Creiamo il database e le tabelle della tua app
            await context.Database.MigrateAsync();

            var staffSet = context.Set<Staff>();
            if (!await staffSet.AnyAsync())
            {
                var passwordService = services.GetRequiredService<IPasswordService>();
                var hashedPassword = passwordService.HashPassword("123stella");

                var adminUser = new Staff
                {
                    Username = "StaffManager", 
                    Email = "staff@mail.com",
                    PasswordHash = hashedPassword,
                    Role = StaffRoles.Manager, 
                    IsActive = true
                };

                staffSet.Add(adminUser);
                await context.SaveChangesAsync();
                logger.LogInformation("Seed del database completato: Utente Manager creato.");
            }

            // 2. FORZIAMO HANGFIRE A CREARE LE SUE TABELLE ORA!
            // Inizializzando esplicitamente lo storage qui, Hangfire 
            // eseguirà i suoi script SQL di installazione in modo sincrono.
            JobStorage.Current = new SqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero, 
                UseRecommendedIsolationLevel = true,
                PrepareSchemaIfNecessary = true // Fondamentale: crea lo schema se non esiste
            });

            // 3. Ora che le tabelle di Hangfire esistono, possiamo registrare il job
            RecurringJob.AddOrUpdate<InvoiceProcessingJob>(
                "process-fic-invoices", 
                job => job.ProcessPendingInvoicesAsync(), 
                Cron.Minutely());
                
            logger.LogInformation("Job in background di Hangfire registrati con successo.");

            break; // Boom. Finito. Usciamo dal ciclo.
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning(ex, $"Database o Hangfire non ancora pronti. Tentativi rimasti: {retries}. Attendo 5 secondi...");
            
            await Task.Delay(5000); 
            
            if (retries == 0)
            {
                logger.LogError(ex, "Errore fatale all'avvio. Arresto l'applicazione.");
                throw; 
            }
        }
    }
}

app.Run();

app.Run();