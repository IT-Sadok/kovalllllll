using System.Diagnostics;
using System.Text;
using System.Text.Json;
using LibraryApp.Data.Models;

namespace LibraryApp.Data.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private static readonly string FilePath = Path.Combine("Data", "books.json");
    private readonly List<Book> _books;

    public LibraryRepository()
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(FilePath))
        {
            using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _books = JsonSerializer.Deserialize<List<Book>>(fs) ?? new List<Book>();
        }
        else
        {
            _books = new List<Book>();
        }
    }

    public Book GetBookById(Guid bookId)
    {
        var book = _books.FirstOrDefault(b => b.Id == bookId);
        return book ?? throw new Exception($"Book with ID: {bookId} not found.");
    }

    public bool DeleteBookById(Guid bookId)
    {
        var book = GetBookById(bookId);
        _books.Remove(book);
        Save();
        return true;
    }

    public IEnumerable<Book> GetBooksByAuthor(string author)
    {
        return _books.Where(b => b.Author.Equals(author, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public IEnumerable<Book> GetBooksByTitle(string title)
    {
        var books = _books.Where(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase)).ToList();
        return books;
    }

    public void AddBook(Book book)
    {
        _books.Add(book);
        Save();
    }

    public void UpdateBook(Book book)
    {
        var existingBook = GetBookById(book.Id);
        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.YearOfPublication = book.YearOfPublication;
        existingBook.Status = book.Status;
        Save();
    }

    public IEnumerable<Book> GetAllBooks()
    {
        return _books;
    }

    public void Save()
    {
        using var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(fs, _books, new JsonSerializerOptions { WriteIndented = true });
    }
}