using LibraryApp.Data.Models;

namespace LibraryApp.Data.Repositories;

public interface ILibraryRepository
{
    Book GetBookById(Guid bookId);
    bool DeleteBookById(Guid bookId);
    IEnumerable<Book> GetBooksByAuthor(string author);
    IEnumerable<Book> GetBooksByTitle(string title);
    void AddBook(Book book);
    void UpdateBook(Book book);
    IEnumerable<Book> GetAllBooks();
    void Save();
}