using DAL.Restaurant.Kithen.Entities;
using DAL.Restaurant.Kithen.Repos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Restaurant.Booking;

namespace DAL.Restaurant.Kithen.Repos.Impl;

public class ProcessedMessagesRepository : IProcessedMessagesRepository
{
    private readonly RestaurantBookingDbContext _dbContext;

    public ProcessedMessagesRepository(RestaurantBookingDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        _dbContext = dbContext;
    }

    public async Task Add(ProcessedMessage message)
    {
        await _dbContext.ProcessedMessages.AddAsync(message);
        await _dbContext.SaveChangesAsync();
    }
    public async Task<bool> Contain(ProcessedMessage message)
        => await _dbContext.ProcessedMessages.FirstOrDefaultAsync(m => m.OrderId == message.OrderId
                                                                    && m.MessageId == message.MessageId) is not null;
    public async Task Delete(ProcessedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        _dbContext.ProcessedMessages.Remove(message);
        await _dbContext.SaveChangesAsync();
    }
}