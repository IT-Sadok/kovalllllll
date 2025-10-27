using LibraryApp.Data.Models;

namespace LibraryApp.Data.Repositories;

public interface ILibraryRepository
{
    Book GetBookById(Guid bookId);
    Task<bool> DeleteBookByIdAsync(Guid bookId, CancellationToken cancellationToken = default);
    IEnumerable<Book> GetBooksByAuthor(string author);
    IEnumerable<Book> GetBooksByTitle(string title);
    Task AddBookAsync(Book book, CancellationToken cancellationToken = default);
    Task UpdateBookAsync(Book book, CancellationToken cancellationToken = default);
    IEnumerable<Book> GetAllBooks();
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task LoadAsync(CancellationToken cancellationToken = default);
}