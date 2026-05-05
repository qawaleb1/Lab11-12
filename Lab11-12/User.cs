using Lab11_12;

public class User
{
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    public List<Note> Notes { get; set; } = new();
}