using Lab11_12;
using Microsoft.EntityFrameworkCore;

namespace Lab11_12.Tests;

public class CrudTests
{
    private DataContext CreateTestDatabase()
    {
        var db = new DataContext();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return db;
    }
    
    private async Task ClearAllNotes() // удаляет все заметки из базы данных
    {
        using var db = new DataContext();
        var allNotes = await db.Notes.ToListAsync();
        db.Notes.RemoveRange(allNotes);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Test_Create_AddsNewNote()
    {
        var testText = "Моя первая заметка";
        var note = await Crud.Create(testText);
        
        Assert.NotNull(note);
        Assert.Equal(testText, note.Text);
        Assert.True(note.Id > 0);
    }

    [Fact]
    public async Task Test_Search_Note_ById()
    {
        var testText = "Заметка для поиска";
        var created = await Crud.Create(testText);
        var found = await Crud.Read(created.Id);

        Assert.NotNull(found);
        Assert.Equal(testText, found.Text);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public async Task Test_Read_ById_ReturnsNullForNonExistentId()
    {
        var nonExistentId = 999;
        var result = await Crud.Read(nonExistentId);
        
        Assert.Null(result);
    }

    [Fact]
    public async Task Test_Checks_Search_Notes_ByText()
    {
        await ClearAllNotes();
        
        await Crud.Create("Яблочный пирог");
        await Crud.Create("Банановый коктейль");
        await Crud.Create("Яблочное варенье");
        await Crud.Create("Шоколадный торт");
        
        var result = await Crud.Read("Ябло");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Test_Read_BySearch_ReturnsEmptyWhenNothingFound()
    {
        await Crud.Create("Первая заметка");
        await Crud.Create("Вторая заметка");
        
        var result = await Crud.Read("НесуществующийТекст");
        
        Assert.Empty(result);
    }

    [Fact]
    public async Task Test_Update_ChangesNoteText()
    {
        var note = await Crud.Create("Старый текст");
        var newText = "Новый текст";
        
        await Crud.Update(note, newText);
        
        var updated = await Crud.Read(note.Id);
        Assert.Equal(newText, updated.Text);
    }

    [Fact]
    public async Task Test_Delete_RemovesNote()
    {
        var note = await Crud.Create("Заметка для удаления");
        var noteId = note.Id;
        
        await Crud.Delete(note);
        
        var deleted = await Crud.Read(noteId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Test_FullWorkflow_CreateReadUpdateDelete()
    {
        var note = await Crud.Create("Начальная заметка"); // создал заметку
        Assert.NotNull(note);
        
        var found = await Crud.Read(note.Id);
        Assert.Equal("Начальная заметка", found.Text); // нашел её по ID
        
        await Crud.Update(note, "Обновленная заметка"); //обновил
        var updated = await Crud.Read(note.Id);
        Assert.Equal("Обновленная заметка", updated.Text);
        
        var search = await Crud.Read("Обновленная"); // ищем её
        Assert.Single(search);
        
        await Crud.Delete(note);
        var afterDelete = await Crud.Read(note.Id); // удаляем
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Test_Create_WithEmptyText_Works()
    {
        var note = await Crud.Create("");
        
        Assert.NotNull(note);
        Assert.Equal("", note.Text);
    }

    [Fact]
    public async Task Test_Create_MultipleNotes_HaveDifferentIds()
    {
        var note1 = await Crud.Create("Заметка 1");
        var note2 = await Crud.Create("Заметка 2");
        var note3 = await Crud.Create("Заметка 3");
        
        Assert.NotEqual(note1.Id, note2.Id);
        Assert.NotEqual(note2.Id, note3.Id);
        Assert.NotEqual(note1.Id, note3.Id);
    }

    [Fact]
    public async Task Test_Read_WithEmptySearch_ReturnsAllNotes()
    {
        await ClearAllNotes();
        
        await Crud.Create("Заметка A");
        await Crud.Create("Заметка B");
        await Crud.Create("Заметка C");
        
        var allNotes = await Crud.Read("");
        
        Assert.Equal(3, allNotes.Count);
    }
}