using APIBanca.Data;
using APIBanca.Models;
using System.Data;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(Options =>
{
    Options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue el JWT; Swagger enviara Authorization: Bearer {token}."
    });

    Options.OperationFilter<AuthorizeOperationFilter>();
});

builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetPreflightMaxAge(TimeSpan.FromHours(12));
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}


var app = builder.Build();

if (args.Contains("--baseline-initial-migration", StringComparer.OrdinalIgnoreCase))
{
    await BaselineInitialMigrationHistoryAsync(app.Services);
    return;
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetService<AppDbContext>();

    if (dbContext is not null)
    {
        try
        {
            await BaselineInitialMigrationAsync(dbContext);
            await dbContext.Database.MigrateAsync();
            await SeedDefaultAdminAsync(dbContext);
        }
        catch
        {
            // Keep startup alive so Render can still boot the service when the DB is temporarily unavailable.
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }));

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/db-test", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

    if (!canConnect)
    {
        return Results.Problem("Database connection failed.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    await dbContext.Database.OpenConnectionAsync(cancellationToken);

    try
    {
        var connection = dbContext.Database.GetDbConnection();

        return Results.Ok(new
        {
            Database = connection.Database,
            DataSource = connection.DataSource,
            State = connection.State.ToString()
        });
    }
    finally
    {
        await dbContext.Database.CloseConnectionAsync();
    }
})
.WithName("TestDatabaseConnection");

app.Run();

static async Task BaselineInitialMigrationHistoryAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetService<AppDbContext>();

    if (dbContext is null)
    {
        return;
    }

    await BaselineInitialMigrationAsync(dbContext);
}

static async Task BaselineInitialMigrationAsync(AppDbContext dbContext)
{
    var initialMigration = dbContext.Database.GetMigrations().FirstOrDefault();
    if (string.IsNullOrWhiteSpace(initialMigration))
    {
        return;
    }

    if (dbContext.Database.GetAppliedMigrations().Any())
    {
        return;
    }

    if (!await ClientesTableExistsAsync(dbContext))
    {
        return;
    }

    var productVersion = typeof(DbContext).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion?
        .Split('+')[0] ?? "9.0.4";

    await dbContext.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
            "MigrationId" character varying(150) NOT NULL,
            "ProductVersion" character varying(32) NOT NULL,
            CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
        );
        """);

    await dbContext.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT {initialMigration}, {productVersion}
        WHERE NOT EXISTS (
            SELECT 1
            FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = {initialMigration}
        );
        """);
}

static async Task<bool> ClientesTableExistsAsync(AppDbContext dbContext)
{
    var connection = dbContext.Database.GetDbConnection();
    var shouldCloseConnection = connection.State != ConnectionState.Open;

    if (shouldCloseConnection)
    {
        await connection.OpenAsync();
    }

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = 'clientes'
            );
            """;

        return (bool?)await command.ExecuteScalarAsync() ?? false;
    }
    finally
    {
        if (shouldCloseConnection)
        {
            await connection.CloseAsync();
        }
    }
}

static async Task SeedDefaultAdminAsync(AppDbContext dbContext)
{
    const string adminUsername = "allanchopen";

    var adminExists = await dbContext.Usuarios
        .AsNoTracking()
        .AnyAsync(usuario => usuario.Username == adminUsername);

    if (adminExists)
    {
        return;
    }

    dbContext.Usuarios.Add(new Usuario
    {
        Username = adminUsername,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
        Rol = "ADMIN",
        Estado = "ACTIVO"
    });

    await dbContext.SaveChangesAsync();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();
        var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

        if (allowsAnonymous || !requiresAuthorization)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        operation.Responses.TryAdd(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse
        {
            Description = "Unauthorized"
        });

        operation.Responses.TryAdd(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse
        {
            Description = "Forbidden"
        });
    }
}
