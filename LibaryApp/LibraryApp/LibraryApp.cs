using LibraryApp.Business.DTOs;
using LibraryApp.Business.Services;
using LibraryApp.Data.Models;
using LibraryApp.Data.Repositories;

namespace LibraryApp;

public class LibraryApp
{
    private LibraryService? _libraryService;
    private readonly Dictionary<string, Func<Task>> _menuActions;

    public LibraryApp()
    {
        _menuActions = new Dictionary<string, Func<Task>>
        {
            { "1", () => AddBookAsync() },
            { "2", () => DeleteBook() },
            {
                "3", () =>
                {
                    SearchBookByAuthor();
                    return Task.CompletedTask;
                }
            },
            {
                "4", () =>
                {
                    SearchBookByTitle();
                    return Task.CompletedTask;
                }
            },
            {
                "5", () =>
                {
                    ShowAllBooks();
                    return Task.CompletedTask;
                }
            },
            { "6", () => BorrowBookAsync() },
            { "7", () => ReturnBookAsync() },
            {
                "8", () =>
                {
                    GetAllBorrowedBooks();
                    return Task.CompletedTask;
                }
            },
            {
                "9", () =>
                {
                    GetAllAvailableBooks();
                    return Task.CompletedTask;
                }
            },
            {
                "10", async () => await new StressTest(_libraryService!).RunStressTest()
            },
            {
                "11", () =>
                {
                    Exit();
                    return Task.CompletedTask;
                }
            }
        };
    }

    public async Task RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var libraryRepository = await LibraryRepository.CreateAsync(cancellationToken);
        _libraryService = new LibraryService(libraryRepository);

        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine();
            if (choice != null && _menuActions.TryGetValue(choice, out var action))
            {
                await action.Invoke();
            }
            else
            {
                Console.WriteLine("Invalid option. Please try again.");
            }

            Console.WriteLine();
        }
    }

    private static void ShowMenu()
    {
        var menuOptions = new Dictionary<string, string>
        {
            { "1", "Add a new book" },
            { "2", "Delete a book by ID" },
            { "3", "Search books by author" },
            { "4", "Search books by title" },
            { "5", "Show all books" },
            { "6", "Borrow a book by ID" },
            { "7", "Return a book by ID" },
            { "8", "Show all borrowed books" },
            { "9", "GetAllAvailableBooks" },
            { "10", "Run Stress Test" },
            { "11", "Exit" }
        };
        Console.WriteLine("Library Management System");
        foreach (var option in menuOptions)
        {
            Console.WriteLine($"{option.Key}. {option.Value}");
        }

        Console.Write("Select an option: ");
    }

    private async Task AddBookAsync(CancellationToken cancellationToken = default)
    {
        Console.Write("Enter book title: ");
        var title = Console.ReadLine();
        Console.Write("Enter book author: ");
        var author = Console.ReadLine();
        Console.Write("Enter year of publication: ");
        var year = Console.ReadLine();

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(year))
        {
            Console.WriteLine("Title and author cannot be empty.");
            return;
        }

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = title,
            Author = author,
            YearOfPublication = year,
            Status = BookStatus.Available
        };
        try
        {
            await _libraryService!.AddBookAsync(book, cancellationToken);
            Console.WriteLine("Book added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding book: {ex.Message}");
        }
    }

    private async Task DeleteBook(CancellationToken cancellationToken = default)
    {
        Console.Write("Enter book ID to delete: ");
        var input = Console.ReadLine();
        if (Guid.TryParse(input, out var bookId))
        {
            var success = await _libraryService!.DeleteBookAsync(bookId, cancellationToken);
            Console.WriteLine(success ? "Book deleted successfully." : "Book not found.");
        }
        else
        {
            Console.WriteLine("Invalid ID format.");
        }
    }

    private void SearchBookByAuthor()
    {
        Console.Write("Enter author name to search: ");
        var author = Console.ReadLine();
        if (string.IsNullOrEmpty(author))
        {
            Console.WriteLine("Author name cannot be empty.");
            return;
        }

        var books = _libraryService.SearchBookByAuthor(author);
        ShowBooksCollection(books);
    }

    private void SearchBookByTitle()
    {
        Console.Write("Enter book title to search: ");
        var title = Console.ReadLine();
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Book title cannot be empty.");
            return;
        }

        var books = _libraryService.SearchBookByTitle(title);
        ShowBooksCollection(books);
    }

    private static void ShowBooksCollection(IEnumerable<Book> books)
    {
        if (books.Any())
        {
            foreach (var book in books)
            {
                Console.WriteLine(
                    $"ID: {book.Id}, Title: {book.Title}, Author: {book.Author}, Year: {book.YearOfPublication}");
            }
        }
        else
        {
            Console.WriteLine("No books found.");
        }
    }

    private static void ShowBooksStatusCollection(IEnumerable<BookStatusModel> books)
    {
        if (books.Any())
        {
            foreach (var book in books)
            {
                Console.WriteLine(
                    $"ID: {book.Id}, Title: {book.Title},Author: {book.Author}, Status: {book.Status}");
            }
        }
        else
        {
            Console.WriteLine("No books found.");
        }
    }

    private void GetAllAvailableBooks()
    {
        var books = _libraryService.GetAvailableBooks();
        ShowBooksStatusCollection(books);
    }

    private void GetAllBorrowedBooks()
    {
        var books = _libraryService.GetBorrowedBooks();
        ShowBooksStatusCollection(books);
    }

    private void ShowAllBooks()
    {
        var books = _libraryService.GetAllBooks();
        if (books.Any())
        {
            foreach (var book in books)
            {
                Console.WriteLine(
                    $"ID: {book.Id}, Title: {book.Title}, Author: {book.Author}, Year: {book.YearOfPublication}");
            }
        }
        else
        {
            Console.WriteLine("No books found in the library.");
        }
    }

    private async Task BorrowBookAsync(CancellationToken cancellationToken = default)
    {
        Console.Write("Enter book ID to borrow: ");
        var input = Console.ReadLine();
        if (Guid.TryParse(input, out var bookId))
        {
            var success = await _libraryService.BorrowBookAsync(bookId, cancellationToken);
            Console.WriteLine(success ? "Book borrowed successfully." : "Book is already borrowed or not found.");
        }
        else
        {
            Console.WriteLine("Invalid ID format.");
        }
    }

    private async Task ReturnBookAsync(CancellationToken cancellationToken = default)
    {
        Console.Write("Enter book ID to return: ");
        var input = Console.ReadLine();
        if (Guid.TryParse(input, out var bookId))
        {
            var success = await _libraryService.ReturnBookAsync(bookId, cancellationToken);
            Console.WriteLine(success ? "Book returned successfully." : "Book is already available or not found.");
        }
        else
        {
            Console.WriteLine("Invalid ID format.");
        }
    }

    private static void Exit()
    {
        Console.WriteLine("Exiting the application.");
        Environment.Exit(0);
    }
}