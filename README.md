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
