using Npgsql;

var app = WebApplication.CreateBuilder(args).Build();

string DbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "db";
string DbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
string DbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "appdb";
string DbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "appuser";
string DbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "apppass";

var connString = $"Host={DbHost};Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword};Pooling=true;";

async Task EnsureDbAsync()
{
    await using var conn = new NpgsqlConnection(connString);
    await conn.OpenAsync();

    await using (var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS items (id SERIAL PRIMARY KEY, name TEXT NOT NULL);", conn))
        await cmd.ExecuteNonQueryAsync();

    await using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM items;", conn))
    {
        var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
        if (count == 0)
        {
            await using var ins = new NpgsqlCommand("INSERT INTO items(name) VALUES (@n1), (@n2);", conn);
            ins.Parameters.AddWithValue("n1", "Hello from .NET backend");
            ins.Parameters.AddWithValue("n2", "Postgres is connected");
            await ins.ExecuteNonQueryAsync();
        }
    }
}

await EnsureDbAsync();

app.MapGet("/api/healthz", async () =>
{
    await using var conn = new NpgsqlConnection(connString);
    await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand("SELECT 1;", conn);
    var one = await cmd.ExecuteScalarAsync();
    return Results.Ok(new { status = "ok", db = one });
});

app.MapGet("/api/items", async () =>
{
    await using var conn = new NpgsqlConnection(connString);
    await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand("SELECT id, name FROM items ORDER BY id;", conn);
    await using var reader = await cmd.ExecuteReaderAsync();

    var list = new List<object>();
    while (await reader.ReadAsync())
    {
        list.Add(new { id = reader.GetInt32(0), name = reader.GetString(1) });
    }
    return Results.Ok(list);
});

app.MapPost("/api/items", async (ItemIn item) =>
{
    var name = string.IsNullOrWhiteSpace(item.name) ? "Unnamed" : item.name;

    await using var conn = new NpgsqlConnection(connString);
    await conn.OpenAsync();
    await using (var cmd = new NpgsqlCommand("INSERT INTO items(name) VALUES (@n);", conn))
    {
        cmd.Parameters.AddWithValue("n", name);
        await cmd.ExecuteNonQueryAsync();
    }
    return Results.Ok(new { name });
});

app.Run("http://0.0.0.0:8080");

record ItemIn(string name);
