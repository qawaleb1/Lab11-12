using Lab11_12;
using Microsoft.EntityFrameworkCore;


await using var db = new DataContext();
await db.Database.MigrateAsync();