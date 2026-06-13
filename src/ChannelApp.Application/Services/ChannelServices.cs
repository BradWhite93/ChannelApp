using ChannelApp.Application.DTOs;
using ChannelApp.Application.Interfaces;
using ChannelApp.Domain.Validation;

namespace ChannelApp.Application.Services;

public class ChannelService : IChannelService
{
    private readonly IChannelRepository _channelRepo;
    private readonly IFavouritesRepository _favouritesRepo;
    private List<ChannelDto> _channels = [];
    private HashSet<int> _favouriteIds = [];

    public ChannelService(IChannelRepository channelRepo, IFavouritesRepository favouritesRepo)
    {
        _channelRepo = channelRepo;
        _favouritesRepo = favouritesRepo;
    }

    public async Task<List<ChannelDto>> GetAllAsync()
    {
        var domainChannels = await _channelRepo.GetAllAsync();

        ChannelValidator.ValidateAll(domainChannels);

        _favouriteIds = await _favouritesRepo.GetFavouriteIdsAsync();

        _channels = domainChannels.Select(c => new ChannelDto
        {
            Id = c.Id,
            Name = c.Name,
            Category = c.Category,
            Country = c.Country,
            ChannelNumber = c.ChannelNumber,
            Playback = c.Playback,
            IsFavourite = _favouriteIds.Contains(c.Id)
        }).ToList();

        return _channels;
    }

    public List<string> GetDistinctCategories(IEnumerable<ChannelDto> channels)
    {
        return channels
            .Select(c => c.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(cat => cat, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public List<string> GetDistinctCountries(IEnumerable<ChannelDto> channels)
    {
        return channels
            .Select(c => c.Country)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(country => country, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public List<ChannelDto> ApplyFilter(IEnumerable<ChannelDto> channels, ChannelFilter filter)
    {
        IEnumerable<ChannelDto> result = channels;

        if (!string.IsNullOrEmpty(filter.Category))
            result = result.Where(c =>
                string.Equals(c.Category, filter.Category, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(filter.Country))
            result = result.Where(c =>
                string.Equals(c.Country, filter.Country, StringComparison.OrdinalIgnoreCase));

        if (filter.Playback.HasValue)
            result = result.Where(c => c.Playback == filter.Playback.Value);

        if (filter.FavouritesOnly)
            result = result.Where(c => c.IsFavourite);

        if (!string.IsNullOrEmpty(filter.SearchText))
            result = result.Where(c =>
                c.Name.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase));

        var seen = new HashSet<int>();
        var deduped = new List<ChannelDto>();
        foreach (var channel in result)
        {
            if (seen.Add(channel.Id))
                deduped.Add(channel);
        }

        return deduped.OrderBy(c => c.ChannelNumber).ToList();
    }

    public async Task ToggleFavouriteAsync(int channelId)
    {
        var affected = _channels.Where(c => c.Id == channelId).ToList();

        bool newState = affected.Count > 0 && !affected[0].IsFavourite;

        foreach (var channel in affected)
            channel.IsFavourite = newState;

        bool wasInSet = _favouriteIds.Contains(channelId);
        if (newState)
            _favouriteIds.Add(channelId);
        else
            _favouriteIds.Remove(channelId);

        try
        {
            await _favouritesRepo.SaveFavouriteIdsAsync(_favouriteIds);
        }
        catch
        {
            // Revert in-memory state so UI stays consistent after a storage failure
            foreach (var channel in affected)
                channel.IsFavourite = !newState;

            if (wasInSet)
                _favouriteIds.Add(channelId);
            else
                _favouriteIds.Remove(channelId);

            throw;
        }
    }
}
