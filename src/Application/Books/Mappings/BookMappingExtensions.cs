using Application.Books.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Books.Mappings;

public static class BookMappingExtensions
{
    public static BookDto ToDto(this Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Author = book.Author,
        Isbn = book.Isbn,
        TotalCopies = book.Copies.Count,
        AvailableCopies = book.Copies.Count(c => c.Status == CopyStatus.Available)
    };
}
