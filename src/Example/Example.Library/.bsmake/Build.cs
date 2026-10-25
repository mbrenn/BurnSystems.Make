
using BurnSystems.Make.BuildAgent;

var make= new BsMake(args);


await make.Verb(Verbs.Clean, async () =>
{
    Console.WriteLine("Cleaning node modules");
    await NpmHelper.DeleteNodeModules();
    File.Delete("Test.txt");
    File.Delete("Test_.txt");
    File.Delete("Test_Debug.txt");
    File.Delete("Test_Release.txt");
    File.Delete("Primes.cs");
});

await make.Verb(Verbs.Build, async () =>
{

    if (await NpmHelper.InstallIfNotInstalled())
    {
        Console.WriteLine("Npm Modules got installed");
    }
    else
    {
        Console.WriteLine("Npm Modules are already installed");
    }

    Console.WriteLine("Calling Primegenerator in configuration of " + make.Arguments.Configuration);

    File.WriteAllText("Test_" + make.Arguments.Configuration + ".txt", "Done");
    File.AppendAllText("Test.txt", make.Arguments.Configuration + "\r\n");

    File.WriteAllText(
        "Primes.cs",
        "public class Primes\n{\npublic static int[] PrimesTill20 = [2, 3, 5, 7, 11, 13, 17, 19];\n}");

    Console.WriteLine("Primes.cs written");

});
