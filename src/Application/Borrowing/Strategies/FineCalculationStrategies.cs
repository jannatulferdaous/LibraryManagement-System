namespace Application.Borrowing.Strategies;

public interface IFineCalculationStrategy
{
    decimal CalculateFine(DateTime dueDate, DateTime returnDate);
}

public class StandardFineStrategy : IFineCalculationStrategy
{
    private const decimal DailyRate = 5.0m;

    public decimal CalculateFine(DateTime dueDate, DateTime returnDate)
    {
        var overdueDays = (returnDate.Date - dueDate.Date).Days;
        return overdueDays > 0 ? overdueDays * DailyRate : 0m;
    }
}

public class StudentFineStrategy : IFineCalculationStrategy
{
    private const decimal DailyRate = 2.0m;
    private const decimal MaxFine = 50.0m;

    public decimal CalculateFine(DateTime dueDate, DateTime returnDate)
    {
        var overdueDays = (returnDate.Date - dueDate.Date).Days;
        return overdueDays > 0 ? Math.Min(overdueDays * DailyRate, MaxFine) : 0m;
    }
}

public class PremiumFineStrategy : IFineCalculationStrategy
{
    // Premium members get a 3-day grace period before any fine accrues.
    private const int GraceDays = 3;
    private const decimal DailyRate = 3.0m;

    public decimal CalculateFine(DateTime dueDate, DateTime returnDate)
    {
        var overdueDays = (returnDate.Date - dueDate.Date).Days - GraceDays;
        return overdueDays > 0 ? overdueDays * DailyRate : 0m;
    }
}
