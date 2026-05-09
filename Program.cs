using APIBanca.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.OpenConnectionAsync();

    try
    {
        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            select exists (
                select 1
                from pg_tables
                where schemaname = 'public' and tablename = 'clientes'
            )
            """;

        var clientesTableExists = (bool?)await command.ExecuteScalarAsync() ?? false;

        if (!clientesTableExists)
        {
            var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();
            await databaseCreator.CreateTablesAsync();
        }
    }
    finally
    {
        await dbContext.Database.CloseConnectionAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();
app.MapControllers();

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
