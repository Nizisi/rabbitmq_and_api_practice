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
### Message durability
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
* * (star) can substitute for exactly one word.
* # (hash) can substitute for zero or more words.
* use case: send messages which  witha routing key that consists of three words **speed.colour.species**
  *Queue1 is bound with binding key "*.orange.*" and Q2 with "*.*.rabbit" and "lazy.#
    * Q1 is interested in all the orange animals.
    * Q2 wants to hear everything about rabbits, and everything about lazy animals.
* 
