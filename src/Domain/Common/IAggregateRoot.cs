namespace Domain.Common;

// Marker interface - only aggregate roots get repositories.
// BookCopy, for example, is accessed only through Book.
public interface IAggregateRoot
{
}
