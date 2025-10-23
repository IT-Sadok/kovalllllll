using LibraryApp.Data.Models;

namespace LibraryApp.Data.Repositories;

public interface ILibraryRepository
{
    Book GetBookById(Guid bookId);
    Task<bool> DeleteBookByIdAsync(Guid bookId);
    IEnumerable<Book> GetBooksByAuthor(string author);
    IEnumerable<Book> GetBooksByTitle(string title);
    Task AddBookAsync(Book book);
    Task UpdateBookAsync(Book book);
    IEnumerable<Book> GetAllBooks();
    Task SaveAsync();
    Task LoadAsync();
}