using ErrorHandling.Config;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErrorHandling.DeadLetterReceiver
{
    class DeadLetterReceiverConsole
    {
        private static QueueClient QueueClient;

        static void Main(string[] args)
        {
            Utils.WriteLine("DeadLetterReceiverConsole", ConsoleColor.White);
            Console.WriteLine();

            Thread.Sleep(3000);


            var deadLetterPath = EntityNameHelper.FormatDeadLetterPath(Settings.QueuePath);

            Utils.WriteLine($"Dead letter queue path { deadLetterPath }", ConsoleColor.Cyan);

            QueueClient = new QueueClient(Settings.ConnectionString, deadLetterPath);

            ReceiveDeadLetterMessages();

            Utils.WriteLine("Receiving dead letter messages", ConsoleColor.Cyan);
            Console.WriteLine();

            Console.ReadLine();
        }

        static void ReceiveDeadLetterMessages()
        {
            var options = new MessageHandlerOptions(ExRcvHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };

            QueueClient.RegisterMessageHandler(ProcessDeadLetterMessage, options);

            Utils.WriteLine("Receiving messages", ConsoleColor.Cyan);

        }

        private static async Task ProcessDeadLetterMessage(Message message, CancellationToken token)
        {
            Utils.WriteLine("Received dead letter message", ConsoleColor.Cyan);
            Utils.WriteLine($"    Content type: { message.ContentType }", ConsoleColor.Green);
            Utils.WriteLine($"    DeadLetterReason: { message.UserProperties["DeadLetterReason"] }", ConsoleColor.Green);
            Utils.WriteLine($"    DeadLetterErrorDescription: { message.UserProperties["DeadLetterErrorDescription"] }", ConsoleColor.Green);



            

            Console.WriteLine();
        }

        private static async Task ExRcvHandler(ExceptionReceivedEventArgs arg)
        {
            Utils.WriteLine($"Exception: { arg.Exception.Message }", ConsoleColor.Yellow);
        }


    }
}
