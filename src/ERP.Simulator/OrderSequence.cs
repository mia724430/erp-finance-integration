using Integration.Core;

namespace ERP.Simulator;

/// <summary>
/// Persists a monotonically increasing counter across simulator runs, so
/// generated order numbers are guaranteed unique -- never reused, never
/// colliding -- instead of drawn at random (which could, and did, produce
/// duplicate OrderIds across separate runs).
/// </summary>
internal static class OrderSequence
{
    private const int StartingValue = 100000;
    private static readonly string StatePath = Path.Combine(PipelineFolders.State, "order-sequence.txt");

    public static List<int> Reserve(int count)
    {
        var next = ReadCurrent() + 1;
        var reserved = Enumerable.Range(next, count).ToList();

        File.WriteAllText(StatePath, reserved[^1].ToString());

        return reserved;
    }

    private static int ReadCurrent()
    {
        if (!File.Exists(StatePath))
        {
            return StartingValue;
        }

        return int.TryParse(File.ReadAllText(StatePath), out var value) ? value : StartingValue;
    }
}
