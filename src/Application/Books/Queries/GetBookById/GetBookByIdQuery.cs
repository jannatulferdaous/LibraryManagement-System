using Application.Books.Dtos;
using Application.Books.Mappings;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Books.Queries.GetBookById;

public record GetBookByIdQuery(Guid Id) : IRequest<BookDto>, ICacheableQuery
{
    public string CacheKey => $"book:{Id}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDto>
{
    private readonly IRepository<Book> _bookRepository;

    public GetBookByIdQueryHandler(IRepository<Book> bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<BookDto> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), request.Id);

        return book.ToDto();
    }
}
