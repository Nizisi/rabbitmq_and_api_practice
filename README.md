# rabbitmq_and_api_practice
## prerequirement:
* have rabbitmq installed
* (if using visual studio) also download rabbitmq through NuGet Package Manager
* add rabbitmq package though console
## Hello_World
* write two programs in C#; a producer that sends a single message, and a consumer that listen for messages continuous and print them out.
## Work_Queue
* We encapsulate a task as a message and send it to a queue. A worker process running in the background will pop the tasks and eventually execute the job. When you run many workers the tasks will be shared between them
### Message acknowledgment
* In order to make sure a message is never lost, RabbitMQ supports message acknowledgments. An ack(nowledgement) is sent back by the consumer to tell RabbitMQ that a particular message has been received, processed and that RabbitMQ is free to delete it.
* If a consumer dies (its channel is closed, connection is closed, or TCP connection is lost) without sending an ack, RabbitMQ will understand that a message wasn't processed fully and will re-queue it. If there are other consumers online at the same time, it will then quickly redeliver it to another consumer. 
* **Acknowledgement must be sent on the same channel that received the delivery. Attempts to acknowledge using a different channel will result in a channel-level protocol exception**
### Message durability
* When RabbitMQ quits or crashes it will forget the queues and messages unless you tell it not to. Two things are required to make sure that messages aren't lost: we need to mark **both the queue and messages as durable**.
* Marking messages as persistent doesn't fully guarantee that a message won't be lost. Although it tells RabbitMQ to save the message to disk, there is still a short time window when RabbitMQ has accepted a message and hasn't saved it yet
* good for simple task, **publisher confirms** is a stronger guarantee 
### Fair Dispatch
*  use the **BasicQos** method with the **prefetchCount = 1** setting.
*  This tells RabbitMQ not to give more than one message to a worker at a time. Or, in other words, don't dispatch a new message to a worker until it has processed and acknowledged the previous one. Instead, it will dispatch it to the next worker that is not still busy.

## Publish/Subscribe
### Exchanges
* Most of the time,  the producer can only send messages to an exchange. An exchange is a very simple thing. On one side it receives messages from producers and the other side it pushes them to queues. The exchange must know exactly what to do with a message it receives.
* There are a few exchange types available: direct, topic, headers and fanout, **fanout** is focused here
### Temporary queues
* In the .NET client, when we supply no parameters to QueueDeclare() we create a non-durable, exclusive, autodelete queue with a generated name
* whenever we connect to Rabbit we need a fresh, empty queue. To do this we could create a queue with a random name, or, even better - let the server choose a random queue name for us.
* once we disconnect the consumer the queue should be automatically deleted
### Bindings
* The relationship between exchange and a queue that tell the exchange to send messages to our queue is called a binding.

## Routing
### Direct exchange
* The routing algorithm behind a direct exchange is simple - a message goes to the queues whose binding key (binding's routing key) exactly matches the routing key of the message.
* it can't do routing based on multiple criteria.
### Multiple bindings
* It is perfectly legal to bind multiple queues with the same binding key. It will be like fanout exchange
### Subscribing
* create a new binding for each severity we're interested in. The other is the same as receiev message
## Topics
### Topic exchange
* Messages sent to a topic exchange can't have an arbitrary routing_key - it must be a list of words, delimited by dots.usually they specify some features connected to the message. (ex:"stock.usd.nyse", "nyse.vmw", "quick.orange.rabbit".)
* The binding key must also be in the same form
#### special cases:
* \* (star) can substitute for exactly one word.
* \# (hash) can substitute for zero or more words.
* use case: send messages which  witha routing key that consists of three words **speed.colour.species**
  * Queue1 is bound with binding key "\*.orange.\*" and Q2 with "\*.\*.rabbit" and "lazy.#
    * Q1 is interested in all the orange animals.
    * Q2 wants to hear everything about rabbits, and everything about lazy animals.
* Topic exchange can modify to fit the use of fanout and direct exchange
## Remote procedure call (RPC)
* ued when we need to run a function on a remote computer and wait for the result
* Call method which sends an RPC request and blocks until the answer is received
### Important:
  *  When in doubt avoid RPC. If you can, you should use an asynchronous pipeline - instead of RPC-like blocking, results are asynchronously pushed to a next computation stage.
  *  thinkabout:
     *  Make sure it's obvious which function call is local and which is remote.
     *  Document your system. Make the dependencies between components clear.
     *  Handle error cases. How should the client react when the RPC server is down for a long time?
### Message properties
* Persistent: Marks a message as persistent (with a value of true) or transient (any other value). 
* DeliveryMode: those familiar with the protocol may choose to use this property instead of Persistent. They control the same thing.
* ContentType: Used to describe the mime-type of the encoding. For example for the often used JSON encoding it is a good practice to set this property to: application/json.
* ReplyTo: Commonly used to name a callback queue.
* CorrelationId: Useful to correlate RPC responses with requests.
### Correlation Id
* set it to a unique value for every request. Later, when we receive a message in the callback queue we'll look at this property, and based on that we'll be able to match a response with a request.
* No need to create callback queue for every RPC request!
*  If we see an unknown CorrelationId value, we may safely discard the message
   *  due to a possibility of a race condition on the server side
### RPC work steps:
* When the Client starts up, it creates an anonymous exclusive callback queue.
* For an RPC request, the Client sends a message with two properties: **ReplyTo**, which is set to the callback queue and **CorrelationId**, which is set to a unique value for every request.
* The request is sent to an rpc_queue queue.
* The RPC worker (aka: server) is waiting for requests on that queue. When a request appears, it does the job and sends a message with the result back to the Client, using the queue from the ReplyTo property.
* The client waits for data on the callback queue. When a message appears, it checks the CorrelationId property. If it matches the value from the request it returns the response to the application.
### side note:
* If the RPC server is too slow, you can scale up by just running another one.
* On the client side, the RPC requires sending and receiving only one message. No synchronous calls like QueueDeclare are required. As a result the RPC client needs only one network round trip for a single RPC request.
## Publisher Confirms
* a RabbitMQ extension to implement reliable publishing. When publisher confirms are enabled on a channel, messages the client publishes are confirmed asynchronously by the broker, meaning they have been taken care of on the server side.
```
var channel = connection.CreateModel();
channel.ConfirmSelect();
```
* This method must be called on every channel that you expect to use publisher confirms. Confirms should be enabled just once, not for every message published.
### Strategy #1: Publishing Messages Individually
publishing messages individually, waiting for the confirmation synchronously: simple, but very limited throughput.
### Strategy #2: Publishing Messages in Batches
publishing messages in batch, waiting for the confirmation synchronously for a batch: simple, reasonable throughput, but hard to reason about when something goes wrong.
### Strategy #3: Handling Publisher Confirms Asynchronously
asynchronous handling: best performance and use of resources, good control in case of error, but can be involved to implement correctly.
```
var channel = connection.CreateModel();
channel.ConfirmSelect();
channel.BasicAcks += (sender, ea) =>
{
  // code when message is confirmed
};
channel.BasicNacks += (sender, ea) =>
{
  //code when message is nack-ed
};
```
There are 2 callbacks: one for confirmed messages and one for nack-ed messages (messages that can be considered lost by the broker). Both callbacks have a corresponding EventArgs parameter **(ea)** containing a:
* First ea: delivery tag: the sequence number identifying the confirmed or nack-ed message. We will see shortly how to correlate it with the published message.
* second ea : multiple: this is a boolean value. If false, only one message is confirmed/nack-ed, if true, all messages with a lower or equal sequence number are confirmed/nack-ed.
### Sequence number
* The sequence number can be obtained with Channel#NextPublishSeqNo before publishing:
```
var sequenceNumber = channel.NextPublishSeqNo;
channel.BasicPublish(exchange, queue, properties, body);
```
### handling publisher confirms asynchronously steps:
* provide a way to correlate the publishing sequence number with a message.
* register confirm listeners on the channel to be notified when publisher acks/nacks arrive to perform the appropriate actions, like logging or re-publishing a nack-ed message. The sequence-number-to-message correlation mechanism may also require some cleaning during this step.
* track the publishing sequence number before publishing a message.
### handle nack-ed message:
* It can be tempting to re-publish a nack-ed message from the corresponding callback but this should be avoided
