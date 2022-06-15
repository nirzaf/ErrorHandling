
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorHandling.Config
{
    public static class Utils
    {
        public static void WriteLine(string text, ConsoleColor color)
        {
            var tempColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = tempColor;
        }


    }
}
