using LibraryApp.Business.DTOs;
using LibraryApp.Data.Models;

namespace LibraryApp.Business.Services;

public interface ILibraryService
{
    List<BookListDTO> GetAllBooks();
    List<BookStatusDto> GetAvailableBooks();
    List<BookStatusDto> GetBorrowedBooks();
    bool BorrowBook(Guid bookId);
    bool ReturnBook(Guid bookId);
}