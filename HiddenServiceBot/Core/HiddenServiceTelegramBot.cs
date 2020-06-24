using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;

namespace HiddenServiceBot.Core
{
    public class HiddenServiceTelegramBot : TelegramBot
    {
        public HiddenServiceTelegramBot()
        {
            //Ensure Tor & Nginx are both up and running
            Process.Start(new ProcessStartInfo() { FileName = "tor" });
            Process.Start(new ProcessStartInfo() { FileName = "sudo", Arguments = "nginx" }); //The Dockerfile should enable sudo for nginx only...

            this.RegisterMessageHandler("/help", async (e) => {
                var uritext = Environment.GetEnvironmentVariable("QUICK_START_URL");
                if (Uri.TryCreate(uritext, UriKind.Absolute, out var uri))
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "/quickstart");

                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "/start https://www.google.com:443/");
            });


            this.RegisterMessageHandler("/quickstart", async (e) => {
                var uritext = Environment.GetEnvironmentVariable("QUICK_START_URL");
                if (Uri.TryCreate(uritext, UriKind.Absolute, out var uri))
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Starting Hidden Service");

                    //Setup with a random name and a random port
                    var randomName = string.Join("", Enumerable.Range(97, 122).Take(26).OrderBy(x => Guid.NewGuid()).Select(c => Convert.ToChar(c)).Take(6));
                    uint randomPort = Enumerable.Range(10000, 20000).OrderBy(x => Guid.NewGuid()).Select(x => (uint)x).First();

                    //Do the actual forwarding
                    var hdnUrl = ServiceForwarder.CreateServiceForwarder(randomName, randomPort, uri);

                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Hidden Service Available ");
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"http://{hdnUrl}/ ");
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Type /stop_{randomName} to kill the service");
                }
                else
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Could not parse URI from command.");
                }
            });

            this.RegisterMessageHandler("/start", async (e) => {
                var uritext = e.Message.Text.ToLower().Replace("/start ", "");
                if (Uri.TryCreate(uritext, UriKind.Absolute, out var uri))
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Starting Hidden Service");

                    //Setup with a random name and a random port
                    var randomName = string.Join("", Enumerable.Range(97, 122).Take(26).OrderBy(x => Guid.NewGuid()).Select(c => Convert.ToChar(c)).Take(6));
                    uint randomPort = Enumerable.Range(10000, 20000).OrderBy(x => Guid.NewGuid()).Select(x => (uint)x).First();

                    //Do the actual forwarding
                    var hdnUrl = ServiceForwarder.CreateServiceForwarder(randomName, randomPort, uri);

                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Hidden Service Available ");
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"http://{hdnUrl}/ ");
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Type /stop_{randomName} to kill the service");
                }
                else
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Could not parse URI from command.");
                }
            });

            this.RegisterMessageHandler("/stop", async (e) => {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Stopping Hidden Service");
                var serviceName = e.Message.Text.ToLower().Replace("/stop_", "");

                //Remove and Stop
                if(ServiceForwarder.RemoveServiceForwarder(serviceName))
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Stopped Hidden Service");
                else
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Hidden Service Stop Failed");
            });
        }
    }
}
