namespace SteadybitFaultInjection.Injections;

public class InjectionHelper
{
    public static bool IsValidRate(int? rate)
    {
        if (rate == null || rate != null && (rate <= 0 || rate > 100))
        {
            return false;
        }
        return true;
    }

    public static bool ShouldExecuteBasedOnRate(int rate, out int randomValue)
    {
        Random random = new Random();
        randomValue = random.Next(1, 101);

        return randomValue <= rate;
    }
}
