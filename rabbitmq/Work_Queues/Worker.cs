using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

class Worker
{
    public static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            //RabbitMQ doesn't allow you to redefine an existing queue with different parameters, so changed
            channel.QueueDeclare(queue: "task_queue",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);

                //fake task to simulate execution time:
                int dots = message.Split('.').Length - 1;
                Thread.Sleep(dots * 1000);

                Console.WriteLine(" [x] Done");
                
                //Message acknowledgment
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            };
            channel.BasicConsume(queue: "task_queue",
                                 autoAck: false,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");

            Console.ReadLine();
        }
    }
}
