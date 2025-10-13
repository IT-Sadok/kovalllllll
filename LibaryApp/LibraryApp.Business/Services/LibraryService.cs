using LibraryApp.Business.DTOs;
using LibraryApp.Data.Models;
using LibraryApp.Data.Repositories;

namespace LibraryApp.Business.Services;

// Consider returning separate models instead of book to reduce coupling between layers
public class LibraryService(LibraryRepository libraryRepository) : ILibraryService
{
    public List<BookListModel> GetAllBooks()
    {
        return libraryRepository.GetAllBooks()
            .Select(b => new BookListModel
            {
                Id = b.Id.ToString(),
                Title = b.Title,
                Author = b.Author,
                YearOfPublication = b.YearOfPublication,
            })
            .ToList();
    }

    public List<BookStatusModel> GetAvailableBooks()
    {
        return libraryRepository.GetAllBooks()
            .Where(b => b.Status == BookStatus.Available)
            .Select(b => new BookStatusModel
            {
                Id = b.Id.ToString(),
                Title = b.Title,
                Status = b.Status.ToString()
            })
            .ToList();
    }

    public List<BookStatusModel> GetBorrowedBooks()
    {
        return libraryRepository.GetAllBooks()
            .Where(b => b.Status == BookStatus.Borrowed)
            .Select(b => new BookStatusModel
            {
                Id = b.Id.ToString(),
                Title = b.Title,
                Status = b.Status.ToString()
            })
            .ToList();
    }

    public bool BorrowBook(Guid bookId)
    {
        var book = libraryRepository.GetBookById(bookId);
        if (book.Status == BookStatus.Borrowed)
        {
            return false; // Book is already borrowed
        }

        book.Status = BookStatus.Borrowed;
        libraryRepository.UpdateBook(book);
        libraryRepository.Save();
        return true;
    }

    public bool ReturnBook(Guid bookId)
    {
        var book = libraryRepository.GetBookById(bookId);
        if (book.Status == BookStatus.Available)
        {
            return false; // Book is already available
        }

        book.Status = BookStatus.Available;
        libraryRepository.UpdateBook(book);
        libraryRepository.Save();
        return true;
    }

    public void AddBook(Book book)
    {
        libraryRepository.AddBook(book);
        libraryRepository.Save();
    }

    public bool DeleteBook(Guid bookId)
    {
        try
        {
            return libraryRepository.DeleteBookById(bookId);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public IEnumerable<Book> SearchBookByAuthor(string author)
    {
        return libraryRepository.GetBooksByAuthor(author);
    }

    public IEnumerable<Book> SearchBookByTitle(string title)
    {
        return libraryRepository.GetBooksByTitle(title);
    }
}