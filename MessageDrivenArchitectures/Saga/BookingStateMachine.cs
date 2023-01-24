using MassTransit;
using Restaurant.Messaging;

namespace Restaurant.Booking;

public sealed class BookingStateMachine : MassTransitStateMachine<BookingState>
{
    private readonly ILogger _logger;

    public BookingStateMachine(ILogger<BookingStateMachine> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _logger = logger;

        InstanceState(x => x.CurrentState);

        Event(() => BookingRequested, x => x.CorrelateById(context => context.Message.OrderId)
                                            .SelectId(context => context.Message.OrderId));
        Event(() => TableBooked, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => KitchenReady, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => GuestArrived, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => TableBookedFault, x => x.CorrelateById(context => context.Message.Message.OrderId));
        Event(() => KitchenReadyFault, x => x.CorrelateById(context => context.Message.Message.OrderId));
        Event(() => BookingRequestedFault, x => x.CorrelateById(context => context.Message.Message.OrderId));
        Event(() => GuestArrivedFault, x => x.CorrelateById(context => context.Message.Message.OrderId));

        CompositeEvent(() => BookingApproved,
            x => x.ReadyEventStatus, KitchenReady, TableBooked);

        Schedule(() => BookingExpired,
            x => x.ExpirationId, x =>
            {
                x.Delay = TimeSpan.FromSeconds(1);
                x.Received = e => e.CorrelateById(context => context.Message.OrderId);
            });
        Schedule(() => GuestArrivalExpired,
            x => x.ExpirationId, x =>
            {
                x.Delay = TimeSpan.FromSeconds(1);
                x.Received = e => e.CorrelateById(context => context.Message.OrderId);
            });

        Initially(
            When(BookingRequested)
                .Then(context =>
                {
                    context.Instance.CorrelationId = context.Data.OrderId;
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.ClientId = context.Data.ClientId;
                    context.Instance.GuestArrivalVia = context.Data.ArriveVia;
                    _logger.LogInformation("Booking machine state transisted in [AwaitingBookingApproved] state.");
                })
                .Schedule(BookingExpired,
                    context => new BookingExpired(context.Instance),
                    context => TimeSpan.FromSeconds(10))
                .TransitionTo(AwaitingBookingApproved)
        );

        During(AwaitingBookingApproved,
            When(BookingApproved)
                .Unschedule(BookingExpired)
                .Publish(context =>
                    (INotify)new Notify(context.Instance.OrderId,
                                        context.Instance.ClientId,
                                        "стол успешно забронирован"))
                .Publish(context => (IBookingApproved)new BookingApproved(context.Instance.OrderId, context.Instance.ClientId))
                .Schedule(GuestArrivalExpired,
                    context => new GuestArrivalExpired(context.Instance),
                    context => context.Instance.GuestArrivalVia)
                .TransitionTo(AwaitingGuestArrived),

            When(TableBooked)
                .Then(context => context.Instance.TableId = context.Data.TableId),

            When(TableBookedFault).TransitionTo(BookingFault),
            When(KitchenReadyFault).TransitionTo(BookingFault),
            When(BookingRequestedFault).TransitionTo(BookingFault),
            When(GuestArrivedFault).TransitionTo(BookingFault),

            When(BookingExpired.Received)
                .Then(context => _logger.LogInformation("[Order: {orderId}] - booking expired.", context.Instance.OrderId))
                .Publish(context => (IBookingFaulted)new BookingFaulted(context.Instance.OrderId, context.Instance.TableId))
                .Finalize()
        );

        During(AwaitingGuestArrived,
            When(GuestArrived)
                .Unschedule(GuestArrivalExpired)
                .Then(context => _logger.LogInformation("[Order: {orderId}] - guest arrived.", context.Instance.OrderId))
                .Finalize(),

            When(GuestArrivalExpired.Received)
                .Then(context => _logger.LogInformation("[Order: {orderId}] - guest didn't arrive.", context.Instance.OrderId))
                .Publish(context => (IBookingFaulted)new BookingFaulted(context.Instance.OrderId, context.Instance.TableId))
                .Publish(context => (INotify)new Notify(context.Instance.OrderId,
                                               context.Instance.ClientId,
                                               "ваша бронь снята по истечению времени"))
                .Finalize()
        );

        During(BookingFault,
            When(BookingFault.Enter)
                .Then(context => _logger.LogInformation("[Order: {orderId}] - error happend.", context.Instance.OrderId))
                .Then(context => _logger.LogInformation("[Order: {orderId}] - booking cancelled.", context.Instance.OrderId))
                .Publish(context => (IBookingFaulted)new BookingFaulted(context.Instance.OrderId, context.Instance.TableId))
                .Publish(context => (INotify)new Notify(context.Instance.OrderId,
                                                        context.Instance.ClientId,
                                                        "приносим извинения, стол забронировать не получилось"))
                .Finalize());

        SetCompletedWhenFinalized();
    }

    public State AwaitingBookingApproved { get; private set; }
    public State AwaitingGuestArrived { get; private set; }
    public State BookingFault { get; private set; }

    public Event<ITableBooked> TableBooked { get; private set; }
    public Event<IKitchenReady> KitchenReady { get; private set; }
    public Event<IBookingRequested> BookingRequested { get; private set; }
    public Event<IGuestArrived> GuestArrived { get; private set; }

    public Event BookingApproved { get; private set; }

    public Event<Fault<ITableBooked>> TableBookedFault { get; private set; }
    public Event<Fault<IKitchenReady>> KitchenReadyFault { get; private set; }
    public Event<Fault<IBookingRequested>> BookingRequestedFault { get; private set; }
    public Event<Fault<IGuestArrived>> GuestArrivedFault { get; private set; }

    public Schedule<BookingState, IBookingExpired> BookingExpired { get; private set; }
    public Schedule<BookingState, IGuestArrivalExpired> GuestArrivalExpired { get; private set; }
}