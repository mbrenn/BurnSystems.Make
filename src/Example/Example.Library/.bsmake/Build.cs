
var arguments = BurnSystems.Make.BuildAgent.ProgramArguments.Parse(args);

Console.WriteLine("Calling Primegenerator in configuration of " + arguments.Configuration);

File.WriteAllText("Test_" + arguments.Configuration + ".txt", "Done");
File.AppendAllText("Test.txt", arguments.Configuration + "\r\n");

File.WriteAllText(
    "Primes.cs",
    "public class Primes\n{\npublic static int[] PrimesTill20 = [2, 3, 5, 7, 11, 13, 17, 19];\n}");

Console.WriteLine("Primes.cs written");
