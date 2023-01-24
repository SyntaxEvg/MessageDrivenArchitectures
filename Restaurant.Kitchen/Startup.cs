using MassTransit;
using MassTransit.Audit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Restaurant.Kitchen.Consumers;
using Serilog;

namespace Restaurant.Kitchen;

public sealed class Startup
{
    public Startup()
    {
        Configuration = BuildConfiguration();
    }

    internal IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        Log.Logger = BuildLogger();

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>().ConsoleTitle;

        services.AddControllers();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<KitchenBookingRequestedConsumer>().Endpoint(cfg => cfg.Temporary = true);

            x.AddDelayedMessageScheduler();

            x.UsingRabbitMq((context, config) =>
            {
                var rabbitMqConfig = Configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();

                config.Host(host: rabbitMqConfig.Host,
                            virtualHost: rabbitMqConfig.VirtualHost,
                            hostSettings =>
                            {
                                hostSettings.Username(rabbitMqConfig.User);
                                hostSettings.Password(rabbitMqConfig.Password);
                            });

                config.UseScheduledRedelivery(r => r.Intervals(TimeSpan.FromMinutes(10),
                                                               TimeSpan.FromMinutes(20),
                                                               TimeSpan.FromMinutes(30)));

                config.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)));

                config.UseDelayedMessageScheduler();
                config.UseInMemoryOutbox();
                config.ConfigureEndpoints(context);

                config.UsePrometheusMetrics(serviceName: "mt_publish_total");

                x.AddSingleton<IMessageAuditStore, AuditStore>();
                var auditStore = services.BuildServiceProvider().GetService<IMessageAuditStore>();
                config.ConnectSendAuditObservers(auditStore);
                config.ConnectConsumeAuditObserver(auditStore);
            });
        });

        services.Configure<MassTransitHostOptions>(options =>
        {
            options.WaitUntilStarted = true;
            options.StartTimeout = TimeSpan.FromSeconds(30);
            options.StopTimeout = TimeSpan.FromMinutes(1);
        });

        services.AddSingleton<Kitchen>();
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();;

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapMetrics();
            endpoints.MapControllers();
        });
    }

    private IConfiguration BuildConfiguration()
        => new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Startup>()
                .AddEnvironmentVariables().Build();
    private ILogger BuildLogger()
        => new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .WriteTo.File($"log/log-{DateTime.Now.ToString("dd.MM.yyyy")}.log")
            .CreateLogger();
}