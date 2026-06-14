using System.Text.Json;
using ChannelApp.Application.Interfaces;
using ChannelApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChannelApp.Infrastructure.Repositories;

public class JsonChannelRepository : IChannelRepository
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JsonChannelRepository> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonChannelRepository(HttpClient httpClient, ILogger<JsonChannelRepository> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Channel>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("data/channels-enriched.json");
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var channels = await JsonSerializer.DeserializeAsync<List<Channel>>(stream, _jsonOptions);
            return channels ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load channels from data/channels.json");
            return [];
        }
    }
}
