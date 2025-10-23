using LibraryApp.Business.DTOs;
using LibraryApp.Data.Models;
using LibraryApp.Data.Repositories;

namespace LibraryApp.Business.Services;

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

    public async Task<bool> BorrowBookAsync(Guid bookId)
    {
        var book = libraryRepository.GetBookById(bookId);
        if (book == null)
            return false;
        
        if (book.Status == BookStatus.Borrowed)
            return false;

        book.Status = BookStatus.Borrowed;
        await libraryRepository.UpdateBookAsync(book);

        return true;
    }

    public async Task<bool> ReturnBookAsync(Guid bookId)
    {
        var book = libraryRepository.GetBookById(bookId);
        if (book == null)
            return false;
        
        if (book.Status == BookStatus.Available)
            return false;

        book.Status = BookStatus.Available;
        await libraryRepository.UpdateBookAsync(book);

        return true;
    }

    public async Task AddBookAsync(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        await libraryRepository.AddBookAsync(book);
    }

    public async Task<bool> DeleteBookAsync(Guid bookId)
    {
        if (bookId == Guid.Empty)
        {
            return false;
        }

        return await libraryRepository.DeleteBookByIdAsync(bookId);
    }

    public IEnumerable<Book> SearchBookByAuthor(string author)
    {
        return libraryRepository.GetBooksByAuthor(author);
    }

    public IEnumerable<Book> SearchBookByTitle(string title)
    {
        return libraryRepository.GetBooksByTitle(title);
    }

    public async Task DeleteBookByIdAsync(Guid bookId)
    {
        await libraryRepository.DeleteBookByIdAsync(bookId);
    }

    public async Task UpdateBookAsync(Book book)
    {
        await libraryRepository.UpdateBookAsync(book);
    }
}