using LibraryApp.Business.DTOs;
using LibraryApp.Data.Models;

namespace LibraryApp.Business.Services;

public interface ILibraryService
{
    List<BookListModel> GetAllBooks();
    List<BookStatusModel> GetAvailableBooks();
    List<BookStatusModel> GetBorrowedBooks();
    Task<bool> BorrowBookAsync(Guid bookId);
    Task<bool> ReturnBookAsync(Guid bookId);
    Task AddBookAsync(Book book);
    Task<bool> DeleteBookAsync(Guid bookId);
    IEnumerable<Book> SearchBookByAuthor(string author);
    IEnumerable<Book> SearchBookByTitle(string title);
}