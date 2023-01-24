using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Restaurant.Booking;
using Restaurant.Booking.Consumers;
using Restaurant.Kitchen.Consumers;
using Restaurant.Messaging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Tests;

public class BookingSagaTests
{
    private InMemoryTestHarness _harness = null!;
    private ISagaStateMachineTestHarness<BookingStateMachine, BookingState> _bookingStateMachineHarness = null!;
    private ServiceProvider _provider = null!;

    [OneTimeSetUp]
    public async Task Init()
    {
        _provider = new ServiceCollection()
            .AddMassTransitInMemoryTestHarness(config =>
            {
                config.AddConsumer<BookingRequestedConsumer>();
                config.AddConsumer<KitchenBookingRequestedConsumer>();

                config.AddSagaStateMachine<BookingStateMachine, BookingState>().InMemoryRepository();

                config.AddDelayedMessageScheduler();
            })
            .AddLogging()
            .AddSingleton<Booking.Restaurant>()
            .AddSingleton<Kitchen.Kitchen>()
            .BuildServiceProvider();

        _harness = _provider.GetRequiredService<InMemoryTestHarness>();
        _bookingStateMachineHarness = _harness.StateMachineSaga<BookingState, BookingStateMachine>(
            new(_provider.GetRequiredService<ILogger<BookingStateMachine>>()));

        await _harness.Start();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Test]
    public async Task Should_create_a_state_instance()
    {
        var bookingRequestedMessage = new BookingRequested(NewId.NextGuid(),
                                                           NewId.NextGuid(),
                                                           (int)NumberOfSeats.Min,
                                                           DateTime.UtcNow,
                                                           TimeSpan.Zero);
        await _harness.Bus.Publish<IBookingRequested>(bookingRequestedMessage);
        Assert.IsTrue(_bookingStateMachineHarness.Created.Select(x => x.CorrelationId == bookingRequestedMessage.OrderId).Any());
    }

    [Test]
    public async Task Consumed_booking_requested_message()
    {
        var bookingRequestedMessage = new BookingRequested(NewId.NextGuid(),
                                                           NewId.NextGuid(),
                                                           (int)NumberOfSeats.Min,
                                                           DateTime.UtcNow,
                                                           TimeSpan.Zero);
        await _harness.Bus.Publish<IBookingRequested>(bookingRequestedMessage);
        Assert.IsTrue(await _bookingStateMachineHarness.Consumed.Any<IBookingRequested>());
    }

    [Test]
    public async Task Transiting_to_awaiting_booking_approved_state()
    {
        var bookingRequestedMessage = new BookingRequested(NewId.NextGuid(),
                                                           NewId.NextGuid(),
                                                           (int)NumberOfSeats.Min,
                                                           DateTime.UtcNow,
                                                           TimeSpan.Zero);

        await _harness.Bus.Publish<IBookingRequested>(bookingRequestedMessage);
        await Task.Delay(5000);
        var instanceGuid = await _bookingStateMachineHarness.Exists(bookingRequestedMessage.OrderId, x => x.AwaitingBookingApproved);
        var instance = _bookingStateMachineHarness.Created.Contains(bookingRequestedMessage.OrderId);
        Assert.NotNull(instanceGuid);
    }
}