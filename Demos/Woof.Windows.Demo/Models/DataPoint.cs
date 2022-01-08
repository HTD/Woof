namespace Woof.Windows.Demo.Models;

public class DataPoint {

    public int Id { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }

    public DataPoint() { }

    public DataPoint(int id, Random prng) {
        Id = id;
        X = Generate(prng);
        Y = Generate(prng);
        Z = Generate(prng);
    }

    public static IEnumerable<DataPoint> GetSome(int n, int startId = 1) {
        Random? prng = new();
        for (int i = 0; i < n; i++) yield return new DataPoint(i + startId, prng);
    }

    public static async IAsyncEnumerable<DataPoint> GetSomeAsync(int n, int startId = 1) {
        foreach (DataPoint? item in GetSome(n, startId)) {
            await Lag.WaitAsync(0.1);
            yield return item;
        }
    }

    public override string ToString() => $"Id={Id}: [X={X}, Y={Y}, Z={Z}]";

    private static double Generate(Random prng) => 1 - 2 * prng.NextDouble();

}