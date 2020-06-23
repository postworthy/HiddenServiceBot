using HiddenServiceBot.Core;
using System;

namespace HiddenServiceBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new HiddenServiceTelegramBot();

            bot.Run();

            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }
    }
}
