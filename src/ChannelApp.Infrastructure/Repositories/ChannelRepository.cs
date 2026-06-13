using ChannelApp.Application.Interfaces;
using ChannelApp.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChannelApp.Infrastructure.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly HttpClient _httpClient;

    public ChannelRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Channel>> GetAllAsync()
    {
        try
        {
            var channels = await _httpClient.GetFromJsonAsync<List<Channel>>("data/channels.json");
            return channels ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CHANNEL LOAD ERROR: {ex}");
            return [];
        }
    }


}
