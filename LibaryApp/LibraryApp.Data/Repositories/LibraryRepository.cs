using System.Text.Json;
using LibraryApp.Data.Models;

namespace LibraryApp.Data.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private static readonly string FilePath = Path.Combine("Data", "books.json");
    private readonly List<Book> _books;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private LibraryRepository()
    {
        _books = [];
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static async Task<LibraryRepository> CreateAsync(CancellationToken cancellationToken = default)
    {
        var repository = new LibraryRepository();
        await repository.LoadAsync(cancellationToken);
        return repository;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(FilePath))
        {
            await using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var books = await JsonSerializer.DeserializeAsync<List<Book>>(fs, cancellationToken: cancellationToken);
            if (books != null)
            {
                _books.Clear();
                _books.AddRange(books);
            }
        }
    }

    public Book GetBookById(Guid bookId)
    {
        return _books.FirstOrDefault(b => b.Id == bookId);
    }

    public async Task<bool> DeleteBookByIdAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var book = GetBookById(bookId);
            if (book == null)
                return false;
            
            _books.Remove(book);
            await SaveAsync(cancellationToken);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
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

    public async Task AddBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _books.Add(book);
            await SaveAsync(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UpdateBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var existingBook = GetBookById(book.Id);
            if (existingBook == null)
                return;
            
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.YearOfPublication = book.YearOfPublication;
            existingBook.Status = book.Status;

            await SaveAsync(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public IEnumerable<Book> GetAllBooks()
    {
        return _books;
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await using var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(fs, _books, new JsonSerializerOptions { WriteIndented = true }, cancellationToken);
    }
}