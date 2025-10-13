using LibraryApp.Business.DTOs;
using LibraryApp.Business.Services;
using LibraryApp.Data.Models;
using LibraryApp.Data.Repositories;

namespace LibaryApp;

internal class Program
{
    private static readonly LibraryService LibraryService = new(new LibraryRepository());

    private static readonly Dictionary<string, Action> MenuActions = new()
    {
        { "1", AddBook },
        { "2", DeleteBook },
        { "3", SearchBookByAuthor },
        { "4", SearchBookByTitle },
        { "5", ShowAllBooks },
        { "6", BorrowBook },
        { "7", ReturnBook },
        { "8", GetAllBorrowedBooks },
        { "9", GetAllAvailableBooks },
        { "10", Exit }
    };


    private static void Main(string[] args)
    {
        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine();
            if (choice != null && MenuActions.TryGetValue(choice, out var action))
            {
                action.Invoke();
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
            { "10", "Exit" }
        };
        Console.WriteLine("Library Management System");
        foreach (var option in menuOptions)
        {
            Console.WriteLine($"{option.Key}. {option.Value}");
        }

        Console.Write("Select an option: ");
    }

    private static void AddBook()
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
            LibraryService.AddBook(book);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding book: {ex.Message}");
        }
    }


    private static void DeleteBook()
    {
        Console.Write("Enter book ID to delete: ");
        var input = Console.ReadLine();
        if (Guid.TryParse(input, out var bookId))
        {
            var success = LibraryService.DeleteBook(bookId);
            Console.WriteLine(success ? "Book deleted successfully." : "Book not found.");
        }
        else
        {
            Console.WriteLine("Invalid ID format.");
        }
    }

    private static void SearchBookByAuthor()
    {
        Console.Write("Enter author name to search: ");
        var author = Console.ReadLine();
        if (string.IsNullOrEmpty(author))
        {
            Console.WriteLine("Author name cannot be empty.");
            return;
        }

        var books = LibraryService.SearchBookByAuthor(author);
        ShowBooksCollection(books);
    }

    private static void SearchBookByTitle()
    {
        Console.Write("Enter book title to search: ");
        var title = Console.ReadLine();
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Book title cannot be empty.");
            return;
        }

        var books = LibraryService.SearchBookByTitle(title);
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

    private static void GetAllAvailableBooks()
    {
        var books = LibraryService.GetAvailableBooks();
        ShowBooksStatusCollection(books);
    }

    private static void GetAllBorrowedBooks()
    {
        var books = LibraryService.GetBorrowedBooks();
        ShowBooksStatusCollection(books);
    }

    private static void ShowAllBooks()
    {
        var books = LibraryService.GetAllBooks();
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

    private static void BorrowBook()
    {
        Console.Write("Enter book ID to borrow: ");
        var input = Console.ReadLine();
        if (Guid.TryParse(input, out var bookId))
        {
            var success = LibraryService.BorrowBook(bookId);
            Console.WriteLine(success ? "Book borrowed successfully." : "Book is already borrowed or not found.");
        }
        else
        {
            Console.WriteLine("Invalid ID format.");
        }
    }

    private static void ReturnBook()
    {
        Console.Write("Enter book ID to return: ");
        var input = Console.ReadLine();
        if (Guid.TryParse(input, out var bookId))
        {
            var success = LibraryService.ReturnBook(bookId);
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