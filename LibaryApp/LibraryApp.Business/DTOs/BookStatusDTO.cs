using LibraryApp.Data.Models;

namespace LibraryApp.Business.DTOs;

public class BookStatusDto
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Status { get; set; }
}