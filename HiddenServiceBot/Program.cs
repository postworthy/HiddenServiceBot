using HiddenServiceBot.Core;
using System;

namespace HiddenServiceBot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                (new HiddenServiceTelegramBot()).Run().Wait();
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }
    }
}
