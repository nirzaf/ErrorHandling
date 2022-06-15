using ErrorHandling.Config;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ErrorHandling.Receiver
{
    class ReceiverConsole
    {
        private static QueueClient QueueClient;

        static async Task Main(string[] args)
        {
            Utils.WriteLine("ReceiverConsole", ConsoleColor.White);
            Console.WriteLine();

            await CreateQueue();

            QueueClient = new QueueClient(Settings.ConnectionString, Settings.QueuePath);

            ReceiveMessages();

            Console.ReadLine();
        }



        static void ReceiveMessages()
        {
            var options = new MessageHandlerOptions(ExRcvHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            QueueClient.RegisterMessageHandler(ProcessMessage, options);

            Utils.WriteLine("Receiving messages", ConsoleColor.Cyan);

        }

        private static async Task ExRcvHandler(ExceptionReceivedEventArgs arg)
        {
            Utils.WriteLine($"Exception: { arg.Exception.Message }", ConsoleColor.Yellow);
        }

        private static async Task ProcessMessage(Message message, CancellationToken token)
        {
            Utils.WriteLine("Received: " + message.ContentType, ConsoleColor.Cyan);

            switch (message.ContentType)
            {
                case "text/plain":
                    await ProcessTextMessage(message);
                    break;
                case "application/json":
                    await ProcessJsonMessage(message);
                    break;
                default:
                    Console.WriteLine("Received unknown message: " + message.ContentType);

                    // Comment in to abandon message
                    //await QueueClient.AbandonAsync(message.SystemProperties.LockToken);

                    // Comment in to dead letter message
                    //await QueueClient.DeadLetterAsync(message.SystemProperties.LockToken, "Unknown message type",
                    //    "The message type: " + message.ContentType + " is not known.");

                    break;
            }

        }

        private static async Task ProcessTextMessage(Message message)
        {
            var body = Encoding.UTF8.GetString(message.Body);

            Utils.WriteLine($"Text message: { body } - DeliveryCount: { message.SystemProperties.DeliveryCount }", ConsoleColor.Green);

            
            try
            {
                // Send a message to a queue
                var forwardingMessage = new Message();
                var forwardingQueueClient = new QueueClient(Settings.ConnectionString, Settings.ForwardingQueuePath);
                await forwardingQueueClient.SendAsync(forwardingMessage);
                await forwardingQueueClient.CloseAsync();

                // Complete the message if successfully processed
                await QueueClient.CompleteAsync(message.SystemProperties.LockToken);

                Utils.WriteLine("Processed message", ConsoleColor.Cyan);
            }
            catch (Exception ex)
            {
                Utils.WriteLine($"Exception: {  ex.Message }", ConsoleColor.Yellow);

                // Comment in to abandon message
                //await QueueClient.AbandonAsync(message.SystemProperties.LockToken);

                // Comment in to dead-letter the message after 5 processing attempts
                //if (message.SystemProperties.DeliveryCount > 5)
                //{
                //    await QueueClient.DeadLetterAsync(message.SystemProperties.LockToken, ex.Message, ex.ToString());
                //}
                //else
                //{
                //    // Abandon the message
                //    //await QueueClient.AbandonAsync(message.SystemProperties.LockToken);
                //}
            }
        }


        private static async Task ProcessJsonMessage(Message message)
        {
            var body = Encoding.UTF8.GetString(message.Body);
            Utils.WriteLine($"JSON message { body }" + body, ConsoleColor.Green);

            try
            {                
                dynamic data = JsonConvert.DeserializeObject(body);
                Utils.WriteLine($"      Name: { data.contact.name }", ConsoleColor.Green);
                Utils.WriteLine($"      Twitter: { data.contact.twitter }", ConsoleColor.Green);

                // Complete the message if successfully processed
                await QueueClient.CompleteAsync(message.SystemProperties.LockToken);
                Utils.WriteLine("Processed message", ConsoleColor.Cyan);
            }
            catch (Exception ex)
            {
                Utils.WriteLine($"Exception: {  ex.Message }", ConsoleColor.Yellow);

                //await QueueClient.DeadLetterAsync(message.SystemProperties.LockToken, ex.Message, ex.ToString());

            }
        }


        private static async Task CreateQueue()
        {
            var managementClient = new ManagementClient(Settings.ConnectionString);
           
            if (!await managementClient.QueueExistsAsync(Settings.QueuePath))
            {
                await managementClient.CreateQueueAsync(new QueueDescription(Settings.QueuePath)
                {
                    LockDuration = TimeSpan.FromSeconds(5)
                });
            }
            if (!await managementClient.QueueExistsAsync(Settings.ForwardingQueuePath))
            {
                await managementClient.CreateQueueAsync(Settings.ForwardingQueuePath);
            }
        }


    }
}
