namespace LibraryApp;

internal abstract class Program
{
    public static async Task Main(string[] args)
    {
        var app = new LibraryApp();
        await app.RunAsync(args);
    }
}