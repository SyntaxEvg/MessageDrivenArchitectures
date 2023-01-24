using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Restaurant.Booking;
using Restaurant.Booking.Consumers;
using Restaurant.Kitchen.Consumers;
using Restaurant.Messaging;
using Restaurant.Notification;
using Restaurant.Notification.Consumers;
using System;
using System.Threading.Tasks;

namespace Restaurant.Tests;

public class ConsumersTests
{
    private ITestHarness _harness = null!;

    [OneTimeSetUp]
    public async Task Init()
    {
        var serviceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(config =>
            {
                config.AddConsumer<BookingRequestedConsumer>();
                config.AddConsumer<BookingFaultedConsumer>();
                config.AddConsumer<BookingApprovedConsumer>();
                config.AddConsumer<KitchenBookingRequestedConsumer>();
                config.AddConsumer<NotifyConsumer>();
            })
            .AddSingleton<Booking.Restaurant>()
            .AddSingleton<Kitchen.Kitchen>()
            .AddSingleton<Notifier>()
            .BuildServiceProvider(true);

        _harness = serviceProvider.GetTestHarness();

        await _harness.Start();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _harness.Stop();
    }

    [Test]
    public async Task Booking_requested_consumer_listening()
    {
        var bookingRequested = new BookingRequested(NewId.NextGuid(),
                                 NewId.NextGuid(),
                                 (int)NumberOfSeats.Min,
                                 DateTime.UtcNow,
                                 TimeSpan.FromSeconds(5));

        await _harness.Bus.Publish<IBookingRequested>(bookingRequested);

        Assert.IsTrue(await _harness.Consumed.Any<IBookingRequested>());
    }

    [Test]
    public async Task Booking_requested_consumer_published_table_booked_message()
    {
        var bookingRequested = new BookingRequested(NewId.NextGuid(),
                                                    NewId.NextGuid(),
                                                    (int)NumberOfSeats.Min,
                                                    DateTime.UtcNow,
                                                    TimeSpan.FromSeconds(5));

        await _harness.Bus.Publish<IBookingRequested>(bookingRequested);

        Assert.IsTrue(await _harness.Published.Any<ITableBooked>(x
            => x.Context.Message.OrderId == bookingRequested.OrderId));
    }

    [Test]
    public async Task Booking_approved_consumer_listening()
    {
        var bookingApproved = new BookingApproved(NewId.NextGuid(), NewId.NextGuid());

        await _harness.Bus.Publish<IBookingApproved>(bookingApproved);

        Assert.IsTrue(await _harness.Consumed.Any<IBookingApproved>());
    }

    [Test]
    public async Task Booking_approved_consumer_published_guest_arrived_message()
    {
        var bookingApproved = new BookingApproved(NewId.NextGuid(), NewId.NextGuid());

        await _harness.Bus.Publish<IBookingApproved>(bookingApproved);

        Assert.IsTrue(await _harness.Published.Any<IGuestArrived>(x
            => x.Context.Message.OrderId == bookingApproved.OrderId));
    }

    [Test]
    public async Task Booking_faulted_consumer_listening()
    {
        var consumer = _harness.GetConsumerHarness<BookingFaultedConsumer>();
        var bookingFaulted = new BookingFaulted(NewId.NextGuid(), NewId.NextGuid());

        await _harness.Bus.Publish<IBookingFaulted>(bookingFaulted);

        Assert.IsTrue(await consumer.Consumed.Any<IBookingFaulted>());
    }

    [Test]
    public async Task Kitchen_booking_requested_consumer_listening()
    {
        var consumer = _harness.GetConsumerHarness<KitchenBookingRequestedConsumer>();
        var bookingRequested = new BookingRequested(NewId.NextGuid(),
                                                    NewId.NextGuid(),
                                                    (int)NumberOfSeats.Min,
                                                    DateTime.UtcNow,
                                                    TimeSpan.FromSeconds(5));

        await _harness.Bus.Publish<IBookingRequested>(bookingRequested);

        Assert.IsTrue(await consumer.Consumed.Any<IBookingRequested>());
    }

    [Test]
    public async Task Kitchen_booking_requested_consumer_published_kitchen_ready_message()
    {
        var bookingRequested = new BookingRequested(NewId.NextGuid(),
                                                    NewId.NextGuid(),
                                                    (int)NumberOfSeats.Min,
                                                    DateTime.UtcNow,
                                                    TimeSpan.FromSeconds(5));

        await _harness.Bus.Publish<IBookingRequested>(bookingRequested);

        Assert.IsTrue(await _harness.Published.Any<IKitchenReady>(x
            => x.Context.Message.OrderId == bookingRequested.OrderId));
    }

    [Test]
    public async Task Notify_consumer_listening()
    {
        var consumer = _harness.GetConsumerHarness<NotifyConsumer>();
        var notify = new Notify(NewId.NextGuid(),
                                          NewId.NextGuid(),
                                          string.Empty);

        await _harness.Bus.Publish<INotify>(notify);

        Assert.IsTrue(await consumer.Consumed.Any<INotify>());
    }
}