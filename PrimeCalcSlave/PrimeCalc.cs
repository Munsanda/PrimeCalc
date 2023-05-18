using Newtonsoft.Json;
using PrimeCalcSlave.Models;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PrimeCalcSlave
{

    internal static class PrimeExtensions
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

            while (d != 0 && d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            Byte[] bytes = new Byte[value.GetByteCount()];
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

        public static void CheckPrime(this List<PrimeIn> arrayIn, List<PrimeOut> arrayOut)
        {


            Parallel.For(0, arrayIn.Count, i =>
            {
                bool test = arrayIn.ElementAt(i).value.fromString().IsProbablyPrime(10);

                if (test)
                {
                    PrimeOut primeOut = new PrimeOut()
                    {
                        index = arrayIn.ElementAt(i).index,
                        isPrime = test
                    };

               
                    arrayOut.Add(primeOut);
                }
            });
            //RabbitMQ impl
            #region rabbitMQ
            ConnectionFactory factory = new();
            factory = new ConnectionFactory { Uri = new Uri("amqp://elias:pass@host.docker.internal:5672/") };
            factory.ClientProvidedName = "PrimeCalc Slave 1";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();

            string echangeName = "SlaveExchange";
            string routingKey = "slave-routing-key";
            string queueName = "SlaveQueue";

            channel.ExchangeDeclare(echangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, echangeName, routingKey, null);

            //info being sent
            var jsonString = JsonConvert.SerializeObject(arrayOut);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
            channel.BasicPublish(echangeName, routingKey, null, bytes);

            channel.Close();
            cnn.Close();
            #endregion

        }
    }
}
