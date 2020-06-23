using HiddenServiceBot.Core;
using System;

namespace HiddenServiceBot
{
    class Program
    {
        static void Main(string[] args)
        {
            (new HiddenServiceTelegramBot()).Run().Wait();
        }
    }
}
