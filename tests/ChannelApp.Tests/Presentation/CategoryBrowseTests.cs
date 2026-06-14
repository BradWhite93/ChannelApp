using ChannelApp.Application.DTOs;
using ChannelApp.Application.Interfaces;
using ChannelApp.Application.Services;
using ChannelApp.Domain.Entities;

namespace ChannelApp.Tests.Presentation;

public class CategoryBrowseTests
{
    [Fact]
    public void GetDistinctCategories_NeverIncludesFavourites()
    {
        var svc = new ChannelService(new StubChannelRepo(), new StubFavRepo());

        var channels = new List<ChannelDto>
        {
            new() { Id = 1, Name = "Ch1", Category = "Sports", ChannelNumber = 1, Country = "UK", Playback = true },
            new() { Id = 2, Name = "Ch2", Category = "News", ChannelNumber = 2, Country = "US", Playback = false },
            new() { Id = 3, Name = "Ch3", Category = "Entertainment", ChannelNumber = 3, Country = "UK", Playback = true },
            new() { Id = 4, Name = "Ch4", Category = "Music", ChannelNumber = 4, Country = "US", Playback = false },
        };

        var categories = svc.GetDistinctCategories(channels);

        Assert.DoesNotContain("Favourites", categories, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetDistinctCategories_EmptyList_ReturnsEmpty()
    {
        var svc = new ChannelService(new StubChannelRepo(), new StubFavRepo());

        var categories = svc.GetDistinctCategories(new List<ChannelDto>());

        Assert.Empty(categories);
    }

    [Fact]
    public void GetDistinctCategories_ReturnsAlphabeticallySorted()
    {
        var svc = new ChannelService(new StubChannelRepo(), new StubFavRepo());

        var channels = new List<ChannelDto>
        {
            new() { Id = 1, Name = "Ch1", Category = "Zen", ChannelNumber = 1, Country = "UK", Playback = true },
            new() { Id = 2, Name = "Ch2", Category = "Action", ChannelNumber = 2, Country = "US", Playback = false },
            new() { Id = 3, Name = "Ch3", Category = "Music", ChannelNumber = 3, Country = "UK", Playback = true },
        };

        var categories = svc.GetDistinctCategories(channels);

        Assert.Equal(3, categories.Count);
        Assert.Equal("Action", categories[0]);
        Assert.Equal("Music", categories[1]);
        Assert.Equal("Zen", categories[2]);
    }

    private sealed class StubChannelRepo : IChannelRepository
    {
        public Task<List<Channel>> GetAllAsync() => Task.FromResult(new List<Channel>());
    }

    private sealed class StubFavRepo : IFavouritesRepository
    {
        public Task<HashSet<int>> GetFavouriteIdsAsync() => Task.FromResult(new HashSet<int>());
        public Task SaveFavouriteIdsAsync(HashSet<int> ids) => Task.CompletedTask;
    }
}
