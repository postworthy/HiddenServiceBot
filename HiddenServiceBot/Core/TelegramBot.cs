using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace HiddenServiceBot.Core
{
    public abstract class TelegramBot
    {
        private long trustedChatID = 0;
        protected readonly TelegramBotClient botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_API_KEY"));
        private Dictionary<string, Func<MessageEventArgs, Task>> handlers = new Dictionary<string, Func<MessageEventArgs, Task>>();

        public string ApiKey { get; } = Environment.GetEnvironmentVariable("TELEGRAM_API_KEY");
        public Task Run()
        {
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"ID={me.Id} User={me.FirstName}");

            botClient.OnMessage += HandleMessage;
            botClient.StartReceiving();
            return Task.Factory.StartNew(() => { do { Console.Read(); } while (true); });
        }

        private async void HandleMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text == ApiKey)
            {
                trustedChatID = e.Message.Chat.Id;
                await botClient.SendTextMessageAsync(e.Message.Chat, $"You may now communicate with the bot.");
                return;
            }

            if (trustedChatID > 0 && e.Message.Chat.Id == trustedChatID)
            {
                var filtered = handlers.Where(x => e.Message.Text.ToLower().StartsWith(x.Key.ToLower())).ToList();

                if (filtered?.Count > 0)
                    filtered.ForEach(async x => await x.Value(e));
                else
                    await botClient.SendTextMessageAsync(e.Message.Chat, $"The message was unhandled: {e.Message.Text}");
            }
            else
            {
                await botClient.SendTextMessageAsync(e.Message.Chat, $"Send the bot's api token to prove your identity.");
            }
        }

        protected void RegisterMessageHandler(string handlerTrigger, Func<MessageEventArgs, Task> action)
        {
            handlers.Add(handlerTrigger, action);
        }
    }
}