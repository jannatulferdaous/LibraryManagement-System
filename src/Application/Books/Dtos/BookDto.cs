namespace Application.Books.Dtos;

public class BookDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string Author { get; init; } = default!;
    public string Isbn { get; init; } = default!;
    public int TotalCopies { get; init; }
    public int AvailableCopies { get; init; }
}
