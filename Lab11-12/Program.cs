using Lab11_12;

await using var db = new DataContext();
await db.Database.EnsureCreatedAsync();