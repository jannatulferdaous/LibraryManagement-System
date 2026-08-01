using Domain.Enums;

namespace Application.Borrowing.Strategies;

public interface IFineStrategyFactory
{
    IFineCalculationStrategy GetStrategy(MembershipType membershipType);
}

public class FineStrategyFactory : IFineStrategyFactory
{
    public IFineCalculationStrategy GetStrategy(MembershipType membershipType) => membershipType switch
    {
        MembershipType.Student => new StudentFineStrategy(),
        MembershipType.Premium => new PremiumFineStrategy(),
        MembershipType.Standard => new StandardFineStrategy(),
        _ => new StandardFineStrategy()
    };
}
