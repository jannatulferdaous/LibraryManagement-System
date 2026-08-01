using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Books.Commands.CreateBook;

public record CreateBookCommand(
    string Title,
    string Author,
    string Isbn,
    Guid BranchId,
    int InitialCopies) : IRequest<Guid>;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Guid>
{
    private readonly IRepository<Book> _bookRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookCommandHandler(IRepository<Book> bookRepository, IUnitOfWork unitOfWork)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = Book.Create(request.Title, request.Author, request.Isbn);

        for (var i = 0; i < request.InitialCopies; i++)
            book.AddCopy(request.BranchId);

        await _bookRepository.AddAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return book.Id;
    }
}
