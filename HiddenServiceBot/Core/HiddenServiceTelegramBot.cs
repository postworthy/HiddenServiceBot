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
            var countryCode = Environment.GetEnvironmentVariable("COUNTRY_CODE") ?? "n/a";
            countryCode = countryCode.ToUpper();

            if (countryCode != "N/A")
                File.AppendAllLines("/etc/tor/torrc", new[] { 
                    //$"EntryNodes {{{countryCode}}} StrictNodes 1", 
                    //$"ExitNodes {{{countryCode}}} StrictNodes 1",
                    "ExcludeNodes " + string.Join(", ", "{AF}, {AX}, {AL}, {DZ}, {AS}, {AD}, {AO}, {AI}, {AQ}, {AG}, {AR}, {AM}, {AW}, {AU}, {AT}, {AZ}, {BS}, {BH}, {BD}, {BB}, {BY}, {BE}, {BZ}, {BJ}, {BM}, {BT}, {BO}, {BQ}, {BA}, {BW}, {BV}, {BR}, {IO}, {BN}, {BG}, {BF}, {BI}, {KH}, {CM}, {CA}, {CV}, {KY}, {CF}, {TD}, {CL}, {CN}, {CX}, {CC}, {CO}, {KM}, {CG}, {CD}, {CK}, {CR}, {CI}, {HR}, {CU}, {CW}, {CY}, {CZ}, {DK}, {DJ}, {DM}, {DO}, {EC}, {EG}, {SV}, {GQ}, {ER}, {EE}, {ET}, {FK}, {FO}, {FJ}, {FI}, {FR}, {GF}, {PF}, {TF}, {GA}, {GM}, {GE}, {DE}, {GH}, {GI}, {GR}, {GL}, {GD}, {GP}, {GU}, {GT}, {GG}, {GN}, {GW}, {GY}, {HT}, {HM}, {VA}, {HN}, {HK}, {HU}, {IS}, {IN}, {ID}, {IR}, {IQ}, {IE}, {IM}, {IL}, {IT}, {JM}, {JP}, {JE}, {JO}, {KZ}, {KE}, {KI}, {KP}, {KR}, {KW}, {KG}, {LA}, {LV}, {LB}, {LS}, {LR}, {LY}, {LI}, {LT}, {LU}, {MO}, {MK}, {MG}, {MW}, {MY}, {MV}, {ML}, {MT}, {MH}, {MQ}, {MR}, {MU}, {YT}, {MX}, {FM}, {MD}, {MC}, {MN}, {ME}, {MS}, {MA}, {MZ}, {MM}, {NA}, {NR}, {NP}, {NL}, {NC}, {NZ}, {NI}, {NE}, {NG}, {NU}, {NF}, {MP}, {NO}, {OM}, {PK}, {PW}, {PS}, {PA}, {PG}, {PY}, {PE}, {PH}, {PN}, {PL}, {PT}, {PR}, {QA}, {RE}, {RO}, {RU}, {RW}, {BL}, {SH}, {KN}, {LC}, {MF}, {PM}, {VC}, {WS}, {SM}, {ST}, {SA}, {SN}, {RS}, {SC}, {SL}, {SG}, {SX}, {SK}, {SI}, {SB}, {SO}, {ZA}, {GS}, {SS}, {ES}, {LK}, {SD}, {SR}, {SJ}, {SZ}, {SE}, {CH}, {SY}, {TW}, {TJ}, {TZ}, {TH}, {TL}, {TG}, {TK}, {TO}, {TT}, {TN}, {TR}, {TM}, {TC}, {TV}, {UG}, {UA}, {US}, {AE}, {GB}, {UM}, {UY}, {UZ}, {VU}, {VE}, {VN}, {VG}, {VI}, {WF}, {EH}, {YE}, {ZM}, {ZW}".Split(new string[]{", "}, StringSplitOptions.RemoveEmptyEntries).Where(x=> x != $"{{{countryCode}}}")) + "StrictNodes 1"
                });

            //Ensure Tor & Nginx are both up and running
            Process.Start(new ProcessStartInfo() { FileName = "tor" });
            Process.Start(new ProcessStartInfo() { FileName = "sudo", Arguments = "nginx" }); //The Dockerfile should enable sudo for nginx only...

            this.RegisterMessageHandler("/help", async (e) =>
            {
                var uritext = Environment.GetEnvironmentVariable("QUICK_START_URL");
                if (Uri.TryCreate(uritext, UriKind.Absolute, out var uri))
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "/quickstart");

                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "/list");
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "/start https://www.google.com:443/");
            });


            this.RegisterMessageHandler("/quickstart", async (e) =>
            {
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

            this.RegisterMessageHandler("/list", async (e) =>
            {
                //Do the actual forwarding
                var services = ServiceForwarder.ListExistingServices().ToList();
                if (services.Count > 0)
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Running Services: ");
                    services.ForEach(async x => await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"{x.uri}:{x.portinfo.Split(':').Last()}"));
                }
                else
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"No Running Services");

            });

            this.RegisterMessageHandler("/start", async (e) =>
            {
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

            this.RegisterMessageHandler("/stop", async (e) =>
            {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Stopping Hidden Service");
                var serviceName = e.Message.Text.ToLower().Replace("/stop_", "");

                //Remove and Stop
                if (ServiceForwarder.RemoveServiceForwarder(serviceName))
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Stopped Hidden Service");
                else
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Hidden Service Stop Failed");
            });
        }
    }
}
