using ApiAzureStorageSecure.Data;
using ApiAzureStorageSecure.Helpers;
using ApiAzureStorageSecure.Repositories;
using ApiAzureStorageSecure.Services;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Scalar.AspNetCore;

// ════════════════════════════════════════════════════════════════════════════
//  PROGRAM.CS  –  ApiAzureStorageSecure
//  Combina los patrones de:
//    • ApiOauthEmpleados    → JWT Bearer Token + EF Core + Key Vault
//    • MvcCoreAzureStorage  → Azure Blob Storage + Azure Files
// ════════════════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// ── 1. Leer configuración base ────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("Hospital");

// ── 2. Helpers de seguridad ──────────────────────────────────────────────────
HelperActionOAuthService helperOAuth = new HelperActionOAuthService(builder.Configuration);
builder.Services.AddSingleton<HelperActionOAuthService>(helperOAuth);
builder.Services.AddSingleton<HelperUsuarioToken>();

// ── 3. Azure Key Vault ───────────────────────────────────────────────────────
// Registra SecretClient con la URI del KeyVault definida en appsettings.json.
// En producción se autentifica con Managed Identity (sin credenciales en código).
// En local: az login  →  usa DefaultAzureCredential automáticamente.
builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(builder.Configuration.GetSection("KeyVault"));
});

// ── 4. Recuperar secretos del Key Vault ──────────────────────────────────────
// Si Key Vault no está disponible (desarrollo sin Azure), se usan los valores
// de appsettings.json como fallback.
string storageConnectionString = builder.Configuration.GetValue<string>("AzureKeys:StorageAccount")!;

try
{
    // Construimos un ServiceProvider temporal para recuperar el cliente de secretos
    using var serviceProvider = builder.Services.BuildServiceProvider();
    SecretClient secretClient = serviceProvider.GetRequiredService<SecretClient>();

    if (secretClient != null)
    {
        // Secreto 1: cadena de conexión SQL Azure
        // Nombre del secreto en Key Vault: "secretsqlazureacm"
        KeyVaultSecret secretSql = await secretClient.GetSecretAsync("secretsqlazureacm");
        connectionString = secretSql.Value;
        Console.WriteLine("[KeyVault] Cadena de conexión SQL cargada desde Key Vault.");

        // Secreto 2: cadena de conexión del Storage Account
        // Usamos el nombre "storagestringacm" que creaste en Azure
        KeyVaultSecret secretStorage = await secretClient.GetSecretAsync("storagestringacm");
        storageConnectionString = secretStorage.Value;
        Console.WriteLine("[KeyVault] Cadena de conexión Storage cargada desde Key Vault.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[KeyVault] No disponible o error, usando appsettings: {ex.Message}");
}

// ── 5. Entity Framework Core → SQL Azure ─────────────────────────────────────
builder.Services.AddDbContext<ComicsContext>(options =>
    options.UseSqlServer(connectionString));

// ── 6. Azure Blob Storage ─────────────────────────────────────────────────────
// Registramos el cliente usando la cadena de conexión recuperada (de Vault o AppSettings)
builder.Services.AddTransient<BlobServiceClient>(_ => new BlobServiceClient(storageConnectionString));

// ── 7. Servicios de Storage ───────────────────────────────────────────────────
builder.Services.AddTransient<ServiceStorageBlobs>();

// ── 8. Repositorios ───────────────────────────────────────────────────────────
builder.Services.AddTransient<RepositoryComics>();

// ── 9. Autenticación JWT Bearer ───────────────────────────────────────────────
builder.Services
    .AddAuthentication(helperOAuth.GetAuthenticationSchema())
    .AddJwtBearer(helperOAuth.GetJwtBearerOptions());

// ── 10. Controladores + OpenAPI ───────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();

// ════════════════════════════════════════════════════════════════════════════
var app = builder.Build();
// ════════════════════════════════════════════════════════════════════════════

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();

// El orden es CRÍTICO: Authentication siempre antes que Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();