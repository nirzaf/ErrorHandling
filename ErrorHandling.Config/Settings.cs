using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHandling.Config
{
    public static class Settings
    {
        //ToDo: Enter a valid Service Bus connection string
        public static string ConnectionString = "";
        public static string QueuePath = "errorhandling";
        public static string ForwardingQueuePath = "errorhandlingforwarding";
    }
}
