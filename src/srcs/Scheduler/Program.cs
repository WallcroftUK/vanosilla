using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus.MQTT;
using WingsEmu.ClusterCommunicator;

namespace WingsEmu.ClusterScheduler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            PrintHeader();
            using var stopService = new DockerGracefulStopService();
            using IHost host = CreateHostBuilder(args).Build();
            {
                await host.StartAsync();
                IMessagingService messagingService = host.Services.GetRequiredService<IMessagingService>();
                await messagingService.StartAsync();

                // warmup


                await host.WaitForShutdownAsync(stopService.CancellationToken);
                await messagingService.DisposeAsync();
            }
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(s => { s.ListenAnyIP(25000); });
                    webBuilder.UseStartup<Startup>();
                });
            return host;
        }

        private static void PrintHeader()
        {
            const string text = @"
██╗    ██╗██╗███╗   ██╗ ██████╗ ███████╗███████╗███╗   ███╗██╗   ██╗                                                            
██║    ██║██║████╗  ██║██╔════╝ ██╔════╝██╔════╝████╗ ████║██║   ██║                                                            
██║ █╗ ██║██║██╔██╗ ██║██║  ███╗███████╗█████╗  ██╔████╔██║██║   ██║                                                            
██║███╗██║██║██║╚██╗██║██║   ██║╚════██║██╔══╝  ██║╚██╔╝██║██║   ██║                                                            
╚███╔███╔╝██║██║ ╚████║╚██████╔╝███████║███████╗██║ ╚═╝ ██║╚██████╔╝                                                            
 ╚══╝╚══╝ ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝╚══════╝╚═╝     ╚═╝ ╚═════╝                                                             
                                                                                                                                
███████╗ ██████╗██╗  ██╗███████╗██████╗ ██╗   ██╗██╗     ███████╗██████╗       ███████╗███████╗██████╗ ██╗   ██╗███████╗██████╗ 
██╔════╝██╔════╝██║  ██║██╔════╝██╔══██╗██║   ██║██║     ██╔════╝██╔══██╗      ██╔════╝██╔════╝██╔══██╗██║   ██║██╔════╝██╔══██╗
███████╗██║     ███████║█████╗  ██║  ██║██║   ██║██║     █████╗  ██████╔╝█████╗███████╗█████╗  ██████╔╝██║   ██║█████╗  ██████╔╝
╚════██║██║     ██╔══██║██╔══╝  ██║  ██║██║   ██║██║     ██╔══╝  ██╔══██╗╚════╝╚════██║██╔══╝  ██╔══██╗╚██╗ ██╔╝██╔══╝  ██╔══██╗
███████║╚██████╗██║  ██║███████╗██████╔╝╚██████╔╝███████╗███████╗██║  ██║      ███████║███████╗██║  ██║ ╚████╔╝ ███████╗██║  ██║
╚══════╝ ╚═════╝╚═╝  ╚═╝╚══════╝╚═════╝  ╚═════╝ ╚══════╝╚══════╝╚═╝  ╚═╝      ╚══════╝╚══════╝╚═╝  ╚═╝  ╚═══╝  ╚══════╝╚═╝  ╚═╝
";
            string separator = new('=', Console.WindowWidth);
            string logo = text.Split('\n').Select(s => string.Format("{0," + (Console.WindowWidth / 2 + s.Length / 2) + "}\n", s))
                .Aggregate("", (current, i) => current + i);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(separator + logo + $"Version: {Assembly.GetExecutingAssembly().GetName().Version}\n" + separator);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}