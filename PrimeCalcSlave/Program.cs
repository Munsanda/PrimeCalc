//RabbitMQ impl
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using PrimeCalcSlave.Models;
using Newtonsoft.Json;
using PrimeCalcSlave;


class Program
{
    static void Main(string[] args)
    {
        #region rabbitMQ

        //ConnectionFactory factory = new ConnectionFactory()
        //{
        //    HostName = "host.docker.internal",
        //    Port = 5672,
        //    //DispatchConsumersAsync = true,
        //    UserName = "user",
        //    Password = "pass"
        //    Password = "pass"
        //};


        ConnectionFactory factory = new();
        factory = new ConnectionFactory { Uri = new Uri("amqp://elias:pass@host.docker.internal:5672/") };
        factory.ClientProvidedName = "PrimeCalc Master";

        IConnection cnn = factory.CreateConnection();

        IModel channel = cnn.CreateModel();

        string echangeName = "DemoExchange";
        string routingKey = "demo-routing-key";
        string queueName = "DemoQueue";

        channel.ExchangeDeclare(echangeName, ExchangeType.Direct);
        channel.QueueDeclare(queueName, false, false, false, null);
        channel.QueueBind(queueName, echangeName, routingKey, null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            List<PrimeIn> primeIns = JsonConvert.DeserializeObject<List<PrimeIn>>(message);
            List<PrimeOut> primeOuts = new List<PrimeOut>();

            //main process finds out whether elements are prime or not and sends back to 

            //bool test = primeIn.value.fromString().IsProbablyPrime(10);

            primeIns.CheckPrime(primeOuts);


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


