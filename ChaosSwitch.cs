public class ChaosSwitch
{
    public int DbFailurePercent { get; set; } = 50;

    public void MaybeThrowDbError()
    {
        if (Random.Shared.Next(100) < DbFailurePercent)
            throw new TimeoutException("Simulated DB timeout (CPU spike or network blip)");
    }
}