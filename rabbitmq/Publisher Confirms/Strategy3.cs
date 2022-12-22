//for broker
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


//before publish,  sequence number can be obtained with Channel#NextPublishSeqNo
var sequenceNumber = channel.NextPublishSeqNo;
channel.BasicPublish(exchange, queue, properties, body);


//uses a dictionary to correlate the publishing sequence number with the string body of the message:
//also clean the dictionary when confirms arrive and do something like logging a warning when messages are nack-ed
var outstandingConfirms = new ConcurrentDictionary<ulong, string>();

void cleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
{
    if (multiple)
    {
        var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
        foreach (var entry in confirmed)
        {
            outstandingConfirms.TryRemove(entry.Key, out _);
        }
    }
    else
    {
        outstandingConfirms.TryRemove(sequenceNumber, out _);
    }
}

channel.BasicAcks += (sender, ea) => cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
channel.BasicNacks += (sender, ea) =>
{
    outstandingConfirms.TryGetValue(ea.DeliveryTag, out string body);
    Console.WriteLine($"Message with body {body} has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
    cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
};

// ... publishing code
