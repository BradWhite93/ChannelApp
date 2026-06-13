using ChannelApp.Application.DTOs;

namespace ChannelApp.Application.Interfaces;

public interface IChannelService
{
    Task<List<ChannelDto>> GetAllAsync();
    List<string> GetDistinctCategories(IEnumerable<ChannelDto> channels);
    List<string> GetDistinctCountries(IEnumerable<ChannelDto> channels);
    List<ChannelDto> ApplyFilter(IEnumerable<ChannelDto> channels, ChannelFilter filter);
    Task ToggleFavouriteAsync(int channelId);
}
