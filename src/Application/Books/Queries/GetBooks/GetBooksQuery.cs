using Application.Books.Dtos;
using Application.Books.Mappings;
using Application.Books.Specifications;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Books.Queries.GetBooks;

public record GetBooksQuery(string? SearchTerm, Guid? BranchId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<BookDto>>;

public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, PagedResult<BookDto>>
{
    private readonly IRepository<Book> _bookRepository;

    public GetBooksQueryHandler(IRepository<Book> bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<PagedResult<BookDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        var spec = new BooksBySearchSpecification(request.SearchTerm, request.BranchId, request.Page, request.PageSize);

        var books = await _bookRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _bookRepository.CountAsync(spec, cancellationToken);

        var dtos = books.Select(b => b.ToDto()).ToList();

        return new PagedResult<BookDto>(dtos, request.Page, request.PageSize, totalCount);
    }
}
