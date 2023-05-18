using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeCalcMaster.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

class Program
{
    static public int DisplayMenu()
    {
        Console.WriteLine("Prime Finder");
        Console.WriteLine();
        Console.WriteLine("1. Send Values");
        Console.WriteLine("2. View Primes");
        Console.WriteLine("5. Exit");
        var result = Console.ReadLine();
        return Convert.ToInt32(result);
    }

    static void Main(string[] args)
    {
        RunAsync().Wait();

        //int userInput = 0;
        //do
        //{
        //    userInput = DisplayMenu();
        //} while (userInput != 5);
    }

    static async Task RunAsync()
    {
        Console.Write("Lower limit:");
        int lower = int.Parse(Console.ReadLine());
        Console.Write("Upper limit:");
        int upper = int.Parse(Console.ReadLine());

        //Array
        BigInteger value = BigInteger.Pow(2, 3217) - 1;

        List<PrimeIn> primeIns = new List<PrimeIn>();


        //making a list from the in 


         List<PrimeIn> primeInsTemp = new List<PrimeIn>();

        //PrimeIn primeIn = new() { index = 0, value = value.ToString() };


        //RabbitMQ impl
        #region rabbitMQ
        ConnectionFactory factory = new();
        factory.Uri = new Uri("amqp://elias:pass@localhost:5672");
        factory.ClientProvidedName = "PrimeCalc Master";

        IConnection cnn = factory.CreateConnection();

        IModel channel = cnn.CreateModel();

        string echangeName = "DemoExchange";
        string routingKey = "demo-routing-key";
        string queueName = "DemoQueue";

        channel.ExchangeDeclare(echangeName, ExchangeType.Direct);
        channel.QueueDeclare(queueName,false,false,false, null);
        channel.QueueBind(queueName,echangeName,routingKey, null);

        //info being sent

        for (int i = 0; i <= upper - lower; i++)
        {

            primeIns.Add(new PrimeIn { index = i, value = (lower + i).ToString() });
            //add 10 elements and send
            // if 
            primeInsTemp.Add(new PrimeIn { index = i, value = (lower + i).ToString() });
            if(i == upper - lower)
            {
                var jsonString = JsonConvert.SerializeObject(primeInsTemp);
                byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
                channel.BasicPublish(echangeName, routingKey, null, bytes);
                primeInsTemp.Clear();
            }
            if(i%10 == 0)
            {
                var jsonString = JsonConvert.SerializeObject(primeInsTemp);
                byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
                channel.BasicPublish(echangeName, routingKey, null, bytes);
                primeInsTemp.Clear();
            }

        }


        channel.Close();
        cnn.Close();
        #endregion


        //now listen for incoming 

        #region rabbitMQ listen for primes
        
        factory.Uri = new Uri("amqp://elias:pass@localhost:5672");
        factory.ClientProvidedName = "PrimeCalc Master";

        cnn = factory.CreateConnection();

        channel = cnn.CreateModel();

        echangeName = "SlaveExchange";
        routingKey = "slave-routing-key";
        queueName = "SlaveQueue";

        channel.ExchangeDeclare(echangeName, ExchangeType.Direct);
        channel.QueueDeclare(queueName, false, false, false, null);
        channel.QueueBind(queueName, echangeName, routingKey, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            List<PrimeIn> primesIndexes = JsonConvert.DeserializeObject<List<PrimeIn>>(message);

            //main process finds out whether elements are prime or not and sends back to 

            foreach(var primeIn in primesIndexes)
            {
                Console.WriteLine(primeIns.ElementAt(primeIn.index).value);
            }
            //Console.WriteLine("\nEnd of input\n");
            // will report as done only when the 
            channel.BasicAck(args.DeliveryTag, false);
        };

        string consumerTag = channel.BasicConsume(queueName, false, consumer);

        Console.ReadLine();

        channel.BasicCancel(consumerTag);

        channel.Close();
        cnn.Close();

        #endregion
    }

}


