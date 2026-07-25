namespace Example.Library.bsmake;

public class Build
{
    public static void Main()
    {
        File.WriteAllText(
            "Primes.cs",
"""
public class Primes
{
    public static int[] PrimesTill20 = [2, 3, 5, 7, 11, 13, 17, 19];
}
""");
        
        Console.WriteLine("Primes.cs written");
    }
}