namespace LibraryApp.Data.Models;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string YearOfPublication { get; set; }
    public BookStatus Status { get; set; }
}

public enum BookStatus
{
    Available,
    Borrowed
}