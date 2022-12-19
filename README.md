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
