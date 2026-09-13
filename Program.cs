using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/users", async (NpgsqlDataSource dataSource) =>
{
    try
    {
        var items = new List<NameRecord>();
        await using var command = dataSource.CreateCommand("SELECT id, name FROM users ORDER BY id DESC");
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new NameRecord(reader.GetInt32(0), reader.GetString(1)));
        }
        return Results.Ok(items);
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = "db_error", message = ex.Message }, statusCode: 503);
    }
});

app.Run();

record NameRecord(int Id, string Name);
