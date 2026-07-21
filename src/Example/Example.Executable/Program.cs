using Example.Library;

Console.Write("Type your number: ");
var numberAsString = Console.ReadLine();

if (int.TryParse(numberAsString, out var number))
{
    Console.WriteLine(
        PrimeNumber.IsPrime(number) 
            ? "The number is prime" 
            : "The number is not prime");
}

