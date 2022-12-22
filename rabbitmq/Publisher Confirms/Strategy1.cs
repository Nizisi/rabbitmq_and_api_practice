//This technique is very straightforward but also has a major drawback: it significantly slows down publishing,

while (ThereAreMessagesToPublish())
{
    byte[] body = ...;
    IBasicProperties properties = ...;
    channel.BasicPublish(exchange, queue, properties, body);
    // uses a 5 second timeout
    
    """
    The method returns as soon as the message has been confirmed. 
    If the message is not confirmed within the timeout or if it is nack-ed (meaning the broker could not take care of it for some reason), 
    the method will throw an exception.
    """
    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
}
