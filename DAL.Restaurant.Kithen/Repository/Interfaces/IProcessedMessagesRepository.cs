using DAL.Restaurant.Kithen.Entities;

namespace DAL.Restaurant.Kithen.Repos.Interfaces;

public interface IProcessedMessagesRepository
{
    Task<bool> Contain(ProcessedMessage message);
    Task Add(ProcessedMessage message);
    Task Delete(ProcessedMessage message);
}