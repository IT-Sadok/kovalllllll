using LibraryApp.Business.DTOs;
using LibraryApp.Data.Models;

namespace LibraryApp.Business.Services;

public interface ILibraryService
{
    List<BookListModel> GetAllBooks();
    List<BookStatusModel> GetAvailableBooks();
    List<BookStatusModel> GetBorrowedBooks();
    bool BorrowBook(Guid bookId);
    bool ReturnBook(Guid bookId);
}