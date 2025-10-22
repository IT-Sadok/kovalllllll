using System.Text.Json;
using LibraryApp.Data.Models;

namespace LibraryApp.Data.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private static readonly string FilePath = Path.Combine("Data", "books.json");
    private readonly List<Book> _books;

    private LibraryRepository()
    {
        _books = [];
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static async Task<LibraryRepository> CreateAsync()
    {
        var repository = new LibraryRepository();
        await repository.LoadAsync();
        return repository;
    }

    public async Task LoadAsync()
    {
        if (File.Exists(FilePath))
        {
            await using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var books = await JsonSerializer.DeserializeAsync<List<Book>>(fs);
            if (books != null)
            {
                _books.Clear();
                _books.AddRange(books);
            }
        }
    }

    public Book? GetBookById(Guid bookId)
    {
        return _books.FirstOrDefault(b => b.Id == bookId);
    }

    public async Task<bool> DeleteBookByIdAsync(Guid bookId)
    {
        var book = GetBookById(bookId);
        if (book != null) _books.Remove(book);
        await SaveAsync();
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

    public async Task AddBookAsync(Book book)
    {
        _books.Add(book);
        await SaveAsync();
    }

    public async Task UpdateBookAsync(Book book)
    {
        var existingBook = GetBookById(book.Id);
        if (existingBook != null)
        {
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.YearOfPublication = book.YearOfPublication;
            existingBook.Status = book.Status;
        }

        await SaveAsync();
    }

    public IEnumerable<Book> GetAllBooks()
    {
        return _books;
    }

    public async Task SaveAsync()
    {
        await using var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(fs, _books, new JsonSerializerOptions { WriteIndented = true });
    }
}