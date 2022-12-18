using System;
using RabbitMQ.Client;
using System.Text;

namespace rabbit
{
    class Send
    {
        static void Main(string[] args)
        {
            /*create a connection to the server:
             * var: declare implicitly typed local variable,tells the compiler to figure out the type of the variable at compilation time
             * The connection abstracts the socket connection, and takes care of protocol version negotiation and authentication and so on for us
             */


            var factory = new ConnectionFactory() { HostName = "localhost" }; //connect to a RabbitMQ node on the local machine - hence the localhost.
                                                                              //If we wanted to connect to a node on a different machine we'd simply specify its hostname or IP address here.

            // create a channel, which is where most of the API for getting things done resides.
            //The using statement defines a scope at the end of which an object will be disposed.
            using (var connection = factory.CreateConnection())
            
            using (var channel = connection.CreateModel())
            {
                /*
                 *  Name
                 *  Durable (the queue will survive a broker restart)
                 *  Exclusive (used by only one connection and the queue will be deleted when that connection closes)
                 *  Auto-delete (queue that has had at least one consumer is deleted when last consumer unsubscribes)
                 *  Arguments (optional; used by plugins and broker-specific features such as message TTL, queue length limit, etc)
                 */
                channel.QueueDeclare(queue: "hello",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);



                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);


                
                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        
        }
    }
}
