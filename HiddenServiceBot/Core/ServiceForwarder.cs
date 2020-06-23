using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace HiddenServiceBot.Core
{
    public static class ServiceForwarder
    {
        private const string CONF = @"server {{
          listen {1};
          listen [::]:{1};

          server_name {0};

          location / {{
              proxy_pass {2};
          }}
        }}";

        public static string CreateServiceForwarder(string name, uint port, Uri destination)
        {
            if (IsFileNameValid(name))
            {
                //Create the needed updates for the Tor Hidden Service & Nginx reverse proxy
                File.WriteAllText(Path.Combine("/etc/nginx/conf.d/", name + ".conf"), string.Format(CONF, name, port, destination));
                File.AppendAllLines("/etc/tor/torrc", new[] { $"HiddenServiceDir /var/lib/tor/{name}", $"HiddenServicePort {port} 127.0.0.1:{port}" });

                //Reset Tor and Nginx configs
                Process.Start(new ProcessStartInfo() { FileName = "pkill", Arguments = "-sighup tor" });
                Process.Start(new ProcessStartInfo() { FileName = "pkill", Arguments = "-sighup nginx" });

                //Wait for Tor to create the hidden service
                while (!File.Exists($"/var/lib/tor/{name}/hostname"))
                    Thread.Sleep(100);

                //Get the onion url
                var hdnUrl = File.ReadAllText($"/var/lib/tor/{name}/hostname").Trim();

                return $"{hdnUrl}:{port}";
            }
            else
                throw new Exception("Name is not valid");
        }

        public static bool RemoveServiceForwarder(string name)
        {
            if (IsFileNameValid(name))
            {
                //Create the needed updates for the Tor Hidden Service & Nginx reverse proxy
                var path = Path.Combine("/etc/nginx/conf.d/", name + ".conf");
                if (!path.StartsWith("/etc/nginx/conf.d/")) return false;
                if (!File.Exists(path)) return false;
                var path2 = Path.Combine($"/var/lib/tor/{name}");
                if (!path2.StartsWith("/var/lib/tor/")) return false;
                if (!Directory.Exists(path2)) return false;

                File.Delete(path);
                Directory.Delete(path2, true);

                var torrc = File.ReadAllLines("/etc/tor/torrc");

                var cleanrc = new List<string>();

                for (int i = 0; i < torrc.Length; i++)
                {
                    if (torrc[i] == $"HiddenServiceDir /var/lib/tor/{name}")
                    {
                        i++; //So the next line is also skipped
                        continue;
                    }
                    else
                        cleanrc.Add(torrc[i]);
                }

                //Write cleaned up torrc
                File.WriteAllLines("/etc/tor/torrc", cleanrc);

                //Reset Tor and Nginx configs
                Process.Start(new ProcessStartInfo() { FileName = "pkill", Arguments = "-sighup tor" });
                Process.Start(new ProcessStartInfo() { FileName = "pkill", Arguments = "-sighup nginx" });

                return true;
            }
            else
                throw new Exception("Name is not valid");
        }

        private static bool IsFileNameValid(string fileName)
        {
            System.IO.FileInfo fi = null;
            try
            {
                fi = new System.IO.FileInfo(fileName);
            }
            catch (ArgumentException) { }
            catch (System.IO.PathTooLongException) { }
            catch (NotSupportedException) { }

            if (fi is null)
                return false;
            else
                return true;
        }
    }
}