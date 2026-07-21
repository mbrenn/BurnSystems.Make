namespace Example.Library;

public class PrimeNumber
{
    public static bool IsPrime(int number)
    {
        switch (number)
        {
            case 1:
                return false;
            case 2:
                return true;
        }

        var limit = Math.Ceiling(Math.Sqrt(number));

        for (var i = 2; i <= limit; ++i)  
            if (number % i == 0)  
                return false;
        
        return true;
    }
}