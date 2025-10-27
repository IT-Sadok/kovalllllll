using LibraryApp.Business.Services;
using LibraryApp.Data.Models;

namespace LibraryApp;

public class StressTest(LibraryService libraryService)
{
    private readonly Random _random = new Random();

    public async Task RunStressTest(CancellationToken cancellationToken = default)
    {
        await GenerateRandomData(cancellationToken);
        var tasks = new List<Task>();
        for (var i = 0; i < 100; i++)
        {
            tasks.Add(PickRandomOperation(cancellationToken));
        }

        await Task.WhenAll(tasks);
    }

    private async Task GenerateRandomData(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Generating random data...");
        for (var i = 0; i < 100; i++)
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = $"{_random.Next(1, 10000)}",
                Author = $"{_random.Next(1, 5000)}",
                YearOfPublication = $"{_random.Next(1900, 2024)}",
                Status = (BookStatus)_random.Next(0, 2)
            };
            await libraryService.AddBookAsync(book, cancellationToken);
        }
    }

    private async Task PickRandomOperation(CancellationToken cancellationToken = default)
    {
        try
        {
            var operation = _random.Next(0, 4);
            var allBooks = libraryService.GetAllBooks();
            if (allBooks.Count == 0)
            {
                return;
            }

            var randomBook = allBooks[_random.Next(0, allBooks.Count)];
            if (randomBook == null || string.IsNullOrEmpty(randomBook.Id))
            {
                return;
            }

            var bookId = Guid.Parse(randomBook.Id);

            switch (operation)
            {
                case 0:
                    var borrowed = await libraryService.BorrowBookAsync(bookId, cancellationToken);
                    Console.WriteLine(borrowed
                        ? $"Book {bookId} borrowed successfully."
                        : $"Book {bookId} is already borrowed or not found.");
                    break;
                case 1:
                    var returned = await libraryService.ReturnBookAsync(bookId, cancellationToken);
                    Console.WriteLine(returned
                        ? $"Book {bookId} returned successfully."
                        : $"Book {bookId} was not borrowed or not found.");
                    break;
                case 2:
                    var deleted = await libraryService.DeleteBookAsync(bookId, cancellationToken);
                    Console.WriteLine(deleted
                        ? $"Book {bookId} deleted successfully."
                        : $"Book {bookId} not found.");
                    break;
                case 3:
                    var book = new Book
                    {
                        Id = bookId,
                        Title = $"{_random.Next(1, 10000)}",
                        Author = $"{_random.Next(1, 5000)}",
                        YearOfPublication = $"{_random.Next(1900, 2024)}",
                        Status = (BookStatus)_random.Next(0, 2)
                    };

                    await libraryService.UpdateBookAsync(book, cancellationToken);
                    Console.WriteLine($"Book {bookId} update attempted.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in stress test operation: {ex.Message}");
        }
    }
}