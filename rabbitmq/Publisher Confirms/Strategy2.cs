"""
Waiting for a batch of messages to be confirmed improves throughput drastically over waiting for a confirm for individual message (up to 20-30 times with a remote RabbitMQ node). 
One drawback is that we do not know exactly what went wrong in case of failure, 
so we may have to keep a whole batch in memory to log something meaningful or to re-publish the messages.
And this solution is still synchronous, so it blocks the publishing of messages.
"""

var batchSize = 100;
var outstandingMessageCount = 0;
while (ThereAreMessagesToPublish())
{
    byte[] body = ...;
    IBasicProperties properties = ...;
    channel.BasicPublish(exchange, queue, properties, body);
    outstandingMessageCount++;
    if (outstandingMessageCount == batchSize)
    {
        channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
        outstandingMessageCount = 0;
    }
}
if (outstandingMessageCount > 0)
{
    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
