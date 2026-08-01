using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Books.Commands.UpdateBook;

public record UpdateBookCommand(Guid Id, string Title, string Author, string Isbn) : IRequest;

public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand>
{
    private readonly IRepository<Book> _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;

    public UpdateBookCommandHandler(IRepository<Book> bookRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), request.Id);

        book.UpdateDetails(request.Title, request.Author, request.Isbn);

        _bookRepository.Update(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache-aside invalidation - without this, GetBookByIdQuery would keep serving
        // the pre-update version for up to CacheDuration (5 min) after this succeeds.
        await _cache.RemoveAsync($"book:{request.Id}", cancellationToken);
    }
}
