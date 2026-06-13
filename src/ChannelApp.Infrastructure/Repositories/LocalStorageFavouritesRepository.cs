using Blazored.LocalStorage;
using ChannelApp.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChannelApp.Infrastructure.Repositories;

public class LocalStorageFavouritesRepository : IFavouritesRepository
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<LocalStorageFavouritesRepository> _logger;
    private const string Key = "channelApp.favouriteIds";

    public LocalStorageFavouritesRepository(
        ILocalStorageService localStorage,
        ILogger<LocalStorageFavouritesRepository> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<HashSet<int>> GetFavouriteIdsAsync()
    {
        try
        {
            var ids = await _localStorage.GetItemAsync<int[]>(Key);
            return ids != null ? new HashSet<int>(ids) : [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read favourite IDs from localStorage (key: {Key})", Key);
            return [];
        }
    }

    public async Task SaveFavouriteIdsAsync(HashSet<int> ids)
    {
        // Persists exactly what it's given; throws on write failure so callers can revert UI state.
        await _localStorage.SetItemAsync(Key, ids.ToArray());
    }
}
