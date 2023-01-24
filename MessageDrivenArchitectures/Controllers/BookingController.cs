using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Messaging;

namespace Restaurant.Booking.Controllers;

[Route("[controller]")]
[ApiController]
public class BookingController : ControllerBase
{
    private readonly IBus _bus;
    private readonly ILogger _logger;

    public BookingController(IBus bus, ILogger<BookingController> logger)
    {
        ArgumentNullException.ThrowIfNull(bus, nameof(bus));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _bus = bus;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Book([FromBody]BookingRequestDto request, CancellationToken stoppingToken)
    {
        Guid? response = null;
        try
        {
            var dateTimeNow = DateTime.UtcNow;
            var arrivedAfter = dateTimeNow - request.ArrivalTime;
            var orderId = NewId.NextGuid();
            await _bus.Publish<IBookingRequested>(new BookingRequested(orderId,
                                                                       NewId.NextGuid(),
                                                                       request.NumberOfSeats,
                                                                       dateTimeNow,
                                                                       arrivedAfter));
            _logger.LogInformation("Received request for booking.OrderId: {orderId} ArrivalTime: {arrivalTime}", orderId, request.ArrivalTime);
            response = orderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.StackTrace);
        }
        return Ok(response);
    }
}