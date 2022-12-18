using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;




/*
 * we open a connection and a channel, and declare the queue from which we're going to consume. 
 * Note this matches up with the queue that Send publishes to.
 */
namespace rabbit
{
    class Receive
    {
        static void Main(string[] args) {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                /*
                 * Note that we declare the queue here as well. 
                 * Because we might start the consumer before the publisher, we want to make sure the queue exists before we try to consume messages from it.
                 */
                channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                //EventingBasicConsumer.Received event handler: the queue will push us messages asynchronously, we provide a callback.
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };


                channel.BasicConsume(queue: "hello",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

    }
}
