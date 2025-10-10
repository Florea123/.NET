//Add coments to explain the code
void DisplayInfo(object obj)//Method to display information based on the type of object
{
    switch (obj)//Pattern matching with switch expression
    {
        case Book book:
            Console.WriteLine($"Book: {book.Title}, Year: {book.YearPublished}");
            break;
        case Borrower borrower:
            Console.WriteLine($"Borrower: {borrower.Name}, Books Borrowed: {borrower.BorrowedBooks.Count}");
            break;
        default:
            Console.WriteLine("Unknown type");
            break;
            
    }
}
List<Book> booksList = new()//List of books
{
    new Book("1984", "George Orwell", 1949),
    new Book("To Kill a Mockingbird", "Harper Lee", 1960),
    new Book("The Great Gatsby", "F. Scott Fitzgerald", 1925),
    new Book("The Martian", "Andy Weir", 2014),
    new Book("Ready Player One", "Ernest Cline", 2011),
    new Book("The Night Circus", "Erin Morgenstern", 2011)
};

//Create borrowers and demonstrate immutability with 'with' expression
var borrower1 = new Borrower(1, "Alice", new List<Book> { booksList[0], booksList[1] });
var borrower2 = borrower1 with { BorrowedBooks = new List<Book>(borrower1.BorrowedBooks) { booksList[3] } };
DisplayInfo(borrower1);
DisplayInfo(borrower2);
var books = new List<string>();

while (true)//Loop to get user input for book titles
{
    Console.WriteLine("Enter a book title (or 'exit' to finish): ");
    Console.WriteLine();
    var input = Console.ReadLine();
    if (input?.ToLower() == "exit")
        break;
    if (!string.IsNullOrWhiteSpace(input))
        books.Add(input);
}

Console.WriteLine("\nYou have entered the following book titles:");

foreach (var book in books)//Display entered book titles
{
    Console.WriteLine(book);
}

// Find and display books published after 2010
var recentBooks = booksList.Where(b => b.YearPublished > 2010);
Console.WriteLine("\nBooks published after 2010:");
foreach (var book in recentBooks)
    Console.WriteLine(book);