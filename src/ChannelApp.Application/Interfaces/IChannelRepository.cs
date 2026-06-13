namespace ChannelApp.Application.Interfaces;

using ChannelApp.Domain.Entities;

public interface IChannelRepository
{
    Task<List<Channel>> GetAllAsync();
}

