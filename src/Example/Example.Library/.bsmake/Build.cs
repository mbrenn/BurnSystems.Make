namespace Example.Library.bsmake;

File.WriteAllText(
    "Primes.cs",
    "public class Primes\n{\npublic static int[] PrimesTill20 = [2, 3, 5, 7, 11, 13, 17, 19];\n}");

Console.WriteLine("Primes.cs written");

S