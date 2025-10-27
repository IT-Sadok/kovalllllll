using LibraryApp.Business.DTOs;
using LibraryApp.Data.Models;

namespace LibraryApp.Business.Services;

public interface ILibraryService
{
    List<BookListModel> GetAllBooks();
    List<BookStatusModel> GetAvailableBooks();
    List<BookStatusModel> GetBorrowedBooks();
    Task<bool> BorrowBookAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<bool> ReturnBookAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task AddBookAsync(Book book, CancellationToken cancellationToken = default);
    Task<bool> DeleteBookAsync(Guid bookId, CancellationToken cancellationToken = default);
    IEnumerable<Book> SearchBookByAuthor(string author);
    IEnumerable<Book> SearchBookByTitle(string title);
}