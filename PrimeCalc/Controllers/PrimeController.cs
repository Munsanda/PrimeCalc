using Microsoft.AspNetCore.Mvc;
using PrimeCalc.Models;
using System;
using System.Numerics;

namespace PrimeCalc.Controllers
{
    public static class PrimeExtensions
    {
        // Random generator (thread safe)
        private static ThreadLocal<Random> s_Gen = new ThreadLocal<Random>(
          () => {
              return new Random();
          }
        );

        // Random generator (thread safe)
        private static Random Gen
        {
            get
            {
                return s_Gen.Value;
            }
        }

        public static Boolean IsProbablyPrime(this BigInteger value, int witnesses = 10)
        {
            if (value <= 1)
                return false;

            if (witnesses <= 0)
                witnesses = 10;

            BigInteger d = value - 1; //d is equal to the big integer provided - 1
            int s = 0; // s = number of iterations

            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            Byte[] bytes = new Byte[value.ToByteArray().LongLength];
            BigInteger a;

            for (int i = 0; i < witnesses; i++)
            {
                do
                {
                    Gen.NextBytes(bytes);

                    a = new BigInteger(bytes);
                }
                while (a < 2 || a >= value - 2);

                BigInteger x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1)
                    continue;

                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, value);

                    if (x == 1)
                        return false;
                    if (x == value - 1)
                        break;
                }

                if (x != value - 1)
                    return false;
            }

            return true;
        }

        public static BigInteger fromString(this string s)
        {
            BigInteger.TryParse(s, out BigInteger result);
            return result;
        }

        public static List<PrimeOut> CheckPrime(this  List< PrimeIn> arrayIn, List<PrimeOut> arrayOut)
        {
            

            Parallel.For(0, arrayIn.Count,  i =>
            {
                bool test = arrayIn.ElementAt(i).value.fromString().IsProbablyPrime(10);

                if (test) {
                    PrimeOut primeOut = new PrimeOut()
                    {
                        index = arrayIn.ElementAt(i).index,
                        isPrime = test
                    };

                    arrayOut.Add(primeOut);
                }
            });

            return arrayOut;
        }
    }





    [ApiController]
    [Route("[controller]")]
    public class PrimeController : ControllerBase
    {

        private readonly ILogger<PrimeController> _logger;

        public PrimeController(ILogger<PrimeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<List<PrimeOut>> Post(List<PrimeIn> prime)
        {

            List<PrimeOut> arrayOut = new List<PrimeOut>();
            prime.CheckPrime(arrayOut); // converts from string to big intger
            return arrayOut;
 
        }
    }
}