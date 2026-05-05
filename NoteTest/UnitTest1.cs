using Lab11_12;
using Microsoft.EntityFrameworkCore;

namespace Lab11_12.Tests;

public class CrudTests
{
    private async Task<int> GetTestUserId()
    {
        var user = await Crud.CreateUser("Test User");
        return user.Id;
    }
    private async Task CreateTestDatabase()
    {
        await using var db = new DataContext();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
    
    [Fact]
    public async Task Test_Create_AddsNewNote()
    {
        await CreateTestDatabase();
        int userId = await GetTestUserId();
        var testText = "Моя первая заметка";
        var note = await Crud.Create(userId, testText);
        
        Assert.NotNull(note);
        Assert.Equal(testText, note.Text);
        Assert.Equal(userId, note.UserId);
        Assert.True(note.Id > 0);
    }

    [Fact]
    public async Task Test_Search_Note_ById()
    {
        await CreateTestDatabase();
        var testText = "Заметка для поиска";
        int userId = await GetTestUserId();
        var created = await Crud.Create(userId, testText);
        var found = await Crud.Read(created.Id);

        Assert.NotNull(found);
        Assert.Equal(testText, found.Text);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public async Task Test_Read_ById_ReturnsNullForNonExistentId()
    {
        await CreateTestDatabase();
        var nonExistentId = 999;
        var result = await Crud.Read(nonExistentId);
        
        Assert.Null(result);
    }

    [Fact]
    public async Task Test_Checks_Search_Notes_ByText()
    {
        await CreateTestDatabase();
        int userId = await GetTestUserId();
        
        await Crud.Create(userId,"Яблочный пирог");
        await Crud.Create(userId,"Банановый коктейль");
        await Crud.Create(userId,"Яблочное варенье");
        await Crud.Create(userId,"Шоколадный торт");
        
        var result = await Crud.Read("Ябло");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Test_Read_BySearch_ReturnsEmptyWhenNothingFound()
    {
        await CreateTestDatabase();
        int userId = await GetTestUserId();
        
        await Crud.Create(userId, "Первая заметка");
        await Crud.Create(userId, "Вторая заметка");
        
        var result = await Crud.Read("НесуществующийТекст");
        
        Assert.Empty(result);
    }

    [Fact]
    public async Task Test_Update_ChangesNoteText()
    {
        await CreateTestDatabase();
        int userId = await GetTestUserId();
        var note = await Crud.Create(userId, "Старый текст");
        var newText = "Новый текст";
        
        await Crud.Update(note, newText);
        
        var updated = await Crud.Read(note.Id);
        Assert.Equal(newText, updated.Text);
    }

    [Fact]
    public async Task Test_Delete_RemovesNote()
    {
        await CreateTestDatabase();
        int userId = await GetTestUserId();
        var note = await Crud.Create(userId, "Заметка для удаления");
        var noteId = note.Id;
        
        await Crud.Delete(note);
        
        var deleted = await Crud.Read(noteId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Test_FullWorkflow_CreateReadUpdateDelete()
    {
        await CreateTestDatabase();
        int userId = await GetTestUserId();
        var note = await Crud.Create(userId,"Начальная заметка"); // создал заметку
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
        await CreateTestDatabase();
        int userId = await GetTestUserId();
        var note = await Crud.Create(userId, "");
        
        Assert.NotNull(note);
        Assert.Equal("", note.Text);
    }

    [Fact]
    public async Task Test_Create_MultipleNotes_HaveDifferentIds()
    {
        await CreateTestDatabase();
        int userId = await GetTestUserId();
        var note1 = await Crud.Create(userId, "Заметка 1");
        var note2 = await Crud.Create(userId, "Заметка 2");
        var note3 = await Crud.Create(userId, "Заметка 3");
        
        Assert.NotEqual(note1.Id, note2.Id);
        Assert.NotEqual(note2.Id, note3.Id);
        Assert.NotEqual(note1.Id, note3.Id);
    }

    [Fact]
    public async Task Test_Read_WithEmptySearch_ReturnsAllNotes()
    {
        await CreateTestDatabase();
        int userId = await GetTestUserId();
        
        await Crud.Create(userId, "Заметка A");
        await Crud.Create(userId, "Заметка B");
        await Crud.Create(userId, "Заметка C");
        
        var allNotes = await Crud.Read("");
        
        Assert.Equal(3, allNotes.Count);
    }
    [Fact]
    public async Task CreateUser_ShouldSaveToDatabase()
    {
        await CreateTestDatabase();
        string userName = "Тестовый пирожочек";
        
        var user = await Crud.CreateUser(userName);
        
        Assert.NotNull(user);
        Assert.True(user.Id > 0);
        Assert.Equal(userName, user.Name);
    }
    
    [Fact]
    public async Task CreateNote_ShouldBeLinkedToUser()
    {
        await CreateTestDatabase();
        var user = await Crud.CreateUser("Человечек");
        string noteText = "Заметка этого челочевка";
        
        var note = await Crud.Create(user.Id, noteText);
        var userNotes = await Crud.GetUserNotes(user.Id);
        
        Assert.Single(userNotes);
        Assert.Equal(user.Id, userNotes[0].UserId);
        Assert.Equal(noteText, userNotes[0].Text);
    }
    

    [Fact]
    public async Task DeleteUser_ShouldDeleteAllHisNotes()
    {
        await CreateTestDatabase();
        var user = await Crud.CreateUser("Удаляемый");
        await Crud.Create(user.Id, "Заметка 1");
        await Crud.Create(user.Id, "Заметка 2");
        await Crud.DeleteUser(user.Id);
        var allNotes = await Crud.Read(""); 
        var orphanNotes = allNotes.Where(n => n.UserId == user.Id).ToList();
        
        Assert.Empty(orphanNotes);
    }
}