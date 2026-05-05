using Microsoft.EntityFrameworkCore;

namespace Lab11_12;

public class Crud
{
    // для заметок
    public static async Task<Note> Create(int userId, string text, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var note = new Note
        {
            Text = text,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = userId
        };
        
        db.Notes.Add(note);
        await db.SaveChangesAsync(ct);
        return note;
    }
    
    public static async Task<List<Note>> Read(string search, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var result = await db.Notes
            .Where(x => EF.Functions.Like(x.Text, $"%{search}%"))
            .ToListAsync(ct);
        return result;
    }
    
    public static async Task<Note?> Read(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Notes.FirstOrDefaultAsync(x => x.Id == id, ct);
    }
    
    public static async Task Update(Note note, string newText, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var noteToUpdate = await db.Notes.FindAsync(new object[] { note.Id }, ct);
        if (noteToUpdate != null)
        {
            noteToUpdate.Text = newText;
            await db.SaveChangesAsync(ct);
        }
    }
    
    public static async Task Delete(Note note, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var noteToDelete = await db.Notes.FindAsync(new object[] { note.Id }, ct);
        if (noteToDelete != null)
        {
            db.Notes.Remove(noteToDelete);
            await db.SaveChangesAsync(ct);
        }
    }
    
    public static async Task<List<Note>> GetUserNotes(int userId, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Notes
            .Where(n => n.UserId == userId)
            .ToListAsync(ct);
    }
    
    // для пользователей
    public static async Task<User> CreateUser(string name, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var user = new User { Name = name };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }
    
    public static async Task<List<User>> GetAllUsers(CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Users.ToListAsync(ct);
    }
    
    public static async Task DeleteUser(int userId, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var user = await db.Users.FindAsync(new object[] { userId }, ct);
        if (user != null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync(ct);
        }
    }
    
    
}