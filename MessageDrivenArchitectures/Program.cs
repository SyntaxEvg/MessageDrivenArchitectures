using DAL.Restaurant.Kithen.Repos.Impl;
using DAL.Restaurant.Kithen.Repos.Interfaces;
using MassTransit;
using MassTransit.Audit;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Restaurant.Booking;
using Restaurant.Booking.Consumers;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        builder.Services.AddControllers();

        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<RestaurantBookingDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString(ConnectionStringsKeys.SqlServer)));

        builder.Host.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration)
                .WriteTo.File($"log/log-{DateTime.Now.ToString("dd.MM.yyyy")}.log");
        });

        builder.Services.AddMassTransit(x =>
        {
            x.AddSingleton<IMessageAuditStore, AuditStore>();
            var auditStore = builder.Services.BuildServiceProvider().GetService<IMessageAuditStore>();

            x.AddConsumer<BookingRequestedConsumer>().Endpoint(config => config.Temporary = true);
            x.AddConsumer<BookingFaultedConsumer>().Endpoint(config => config.Temporary = true);
            x.AddConsumer<BookingApprovedConsumer>().Endpoint(config => config.Temporary = true);

            x.AddSagaStateMachine<BookingStateMachine, BookingState>()
                    .Endpoint(e => e.Temporary = true)
                    .InMemoryRepository();

            x.AddDelayedMessageScheduler();

            x.UsingRabbitMq((context, config) =>
            {
                var rabbitMqConfig = builder.Configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();

                config.Host(
                    host: rabbitMqConfig?.Host,
                    virtualHost: rabbitMqConfig?.VirtualHost,
                    hostSettings =>
                    {
                        hostSettings.Username(rabbitMqConfig.User);
                        hostSettings.Password(rabbitMqConfig.Password);
                    });

                config.UsePrometheusMetrics(serviceName: "Kithen_Services");

                config.UseScheduledRedelivery(r => r.Intervals(TimeSpan.FromMinutes(10),
                                                               TimeSpan.FromMinutes(20),
                                                               TimeSpan.FromMinutes(30)));

                config.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)));

                config.UseDelayedMessageScheduler();
                config.UseInMemoryOutbox();
                config.ConfigureEndpoints(context);

                config.ConnectSendAuditObservers(auditStore);
                config.ConnectConsumeAuditObserver(auditStore);
            });
        });

        builder.Services.AddSingleton<Restaurant.Booking.Restaurant>();
        builder.Services.AddTransient<BookingState>();
        builder.Services.AddTransient<BookingStateMachine>();
        builder.Services.AddScoped<IProcessedMessagesRepository, ProcessedMessagesRepository>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapMetrics();
            endpoints.MapControllers();
        });

        app.Run();
    }
}