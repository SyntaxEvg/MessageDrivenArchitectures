using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Restaurant.Notification;

internal sealed class Program
{
    private static void Main(string[] args) => CreateHostBuilder(args).Build().Run();
    private static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
               .UseSerilog()
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.ConfigureKestrel(options =>
                   {
                       options.ListenLocalhost(5002);
                   });
                   webBuilder.UseStartup<Startup>();
               });
}