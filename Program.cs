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

app.MapGet("/health", async (NpgsqlDataSource dataSource) =>
{
    try
    {
        await using var command = dataSource.CreateCommand("SELECT 1");
        var result = await command.ExecuteScalarAsync();
        return Results.Ok(new { status = "ok", db = result });
    }
    catch (Exception ex)
    {
        return Results.Json(new { status = "error", error = ex.Message }, statusCode: 503);
    }
});

app.MapGet("/api/names", async (NpgsqlDataSource dataSource) =>
{
    try
    {
        var items = new List<NameRecord>();
        await using var command = dataSource.CreateCommand("SELECT id, name FROM names ORDER BY id");
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

app.MapGet("/api/names/{id:int}", async (int id, NpgsqlDataSource dataSource) =>
{
    try
    {
        await using var command = dataSource.CreateCommand("SELECT id, name FROM names WHERE id = @id");
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return Results.NotFound(new { error = "not_found", id });
        }
        return Results.Ok(new NameRecord(reader.GetInt32(0), reader.GetString(1)));
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = "db_error", message = ex.Message }, statusCode: 503);
    }
});

app.Run();

record NameRecord(int Id, string Name);
