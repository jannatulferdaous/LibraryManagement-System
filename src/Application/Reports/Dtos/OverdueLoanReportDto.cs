namespace Application.Reports.Dtos;

public record OverdueLoanReportDto(
    Guid LoanId,
    string MemberName,
    string MemberEmail,
    string BookTitle,
    DateTime DueDate,
    int DaysOverdue);
