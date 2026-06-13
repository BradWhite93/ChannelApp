using ChannelApp.Application.DTOs;
using ChannelApp.Application.Interfaces;
using ChannelApp.Application.Services;
using ChannelApp.Domain.Entities;

namespace ChannelApp.Tests.Application;

public class ChannelServiceEdgeCaseTests
{
    private sealed class FixedChannelRepository : IChannelRepository
    {
        private readonly List<Channel> _channels;
        public FixedChannelRepository(List<Channel> channels) => _channels = channels;
        public Task<List<Channel>> GetAllAsync() => Task.FromResult(_channels);
    }

    private sealed class InMemoryFavouritesRepository : IFavouritesRepository
    {
        public Task<HashSet<int>> GetFavouriteIdsAsync() => Task.FromResult(new HashSet<int>());
        public Task SaveFavouriteIdsAsync(HashSet<int> ids) => Task.CompletedTask;
    }

    private sealed class ThrowingFavouritesRepository : IFavouritesRepository
    {
        public Task<HashSet<int>> GetFavouriteIdsAsync() => Task.FromResult(new HashSet<int>());
        public Task SaveFavouriteIdsAsync(HashSet<int> ids) =>
            throw new InvalidOperationException("Storage failure");
    }

    private static ChannelService BuildService(
        List<Channel>? channels = null,
        IFavouritesRepository? favouritesRepo = null)
    {
        var repo = new FixedChannelRepository(channels ?? []);
        var favRepo = favouritesRepo ?? new InMemoryFavouritesRepository();
        return new ChannelService(repo, favRepo);
    }

    private static ChannelDto MakeDto(
        int id, string name, string category, string country,
        int channelNumber = 1, bool playback = false, bool isFavourite = false) =>
        new()
        {
            Id = id,
            Name = name,
            Category = category,
            Country = country,
            ChannelNumber = channelNumber,
            Playback = playback,
            IsFavourite = isFavourite,
        };

    [Fact]
    public void GetDistinctCategories_EmptyList_ReturnsEmpty()
    {
        var svc = BuildService();
        var result = svc.GetDistinctCategories([]);
        Assert.Empty(result);
    }

    [Fact]
    public void GetDistinctCategories_SingleChannel_ReturnsSingleCategory()
    {
        var svc = BuildService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", "Entertainment", "UK"),
        };
        var result = svc.GetDistinctCategories(channels);
        Assert.Equal(new[] { "Entertainment" }, result);
    }

    [Fact]
    public void GetDistinctCategories_AllSameCategory_ReturnsSingleEntry()
    {
        var svc = BuildService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One",   "News", "UK"),
            MakeDto(2, "BBC Two",   "News", "UK"),
            MakeDto(3, "CNN",       "News", "US"),
        };
        var result = svc.GetDistinctCategories(channels);
        Assert.Equal(new[] { "News" }, result);
    }

    [Fact]
    public void GetDistinctCategories_MixedCase_TreatedAsSameCategory()
    {
        var svc = BuildService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", "news",  "UK"),
            MakeDto(2, "Ch2", "NEWS",  "UK"),
            MakeDto(3, "Ch3", "News",  "UK"),
        };
        var result = svc.GetDistinctCategories(channels);
        Assert.Single(result);
    }

    [Fact]
    public void GetDistinctCountries_EmptyList_ReturnsEmpty()
    {
        var svc = BuildService();
        var result = svc.GetDistinctCountries([]);
        Assert.Empty(result);
    }

    [Fact]
    public void GetDistinctCountries_SingleChannel_ReturnsSingleCountry()
    {
        var svc = BuildService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", "Entertainment", "UK"),
        };
        var result = svc.GetDistinctCountries(channels);
        Assert.Equal(new[] { "UK" }, result);
    }

    [Fact]
    public void GetDistinctCountries_AllSameCountry_ReturnsSingleEntry()
    {
        var svc = BuildService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", "Entertainment", "UK"),
            MakeDto(2, "BBC Two", "Drama",         "UK"),
            MakeDto(3, "ITV",     "Entertainment", "UK"),
        };
        var result = svc.GetDistinctCountries(channels);
        Assert.Equal(new[] { "UK" }, result);
    }

    [Fact]
    public void GetDistinctCountries_MixedCase_TreatedAsSameCountry()
    {
        var svc = BuildService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", "News", "uk"),
            MakeDto(2, "Ch2", "News", "UK"),
            MakeDto(3, "Ch3", "News", "Uk"),
        };
        var result = svc.GetDistinctCountries(channels);
        Assert.Single(result);
    }

    [Fact]
    public void ApplyFilter_CategoryAndPlayback_ReturnsOnlyMatchingChannels()
    {
        var svc = BuildService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", "Sports", "UK", channelNumber: 1, playback: true),
            MakeDto(2, "Ch2", "Sports", "UK", channelNumber: 2, playback: false),
            MakeDto(3, "Ch3", "News",   "UK", channelNumber: 3, playback: true),
            MakeDto(4, "Ch4", "News",   "UK", channelNumber: 4, playback: false),
        };

        var filter = new ChannelFilter { Category = "Sports", Playback = true };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public void ApplyFilter_SearchAndFavourites_ReturnsOnlyMatchingFavourites()
    {
        var svc = BuildService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC News",    "News", "UK", isFavourite: true),
            MakeDto(2, "Sky News",    "News", "UK", isFavourite: false),
            MakeDto(3, "BBC Sports",  "Sport","UK", isFavourite: true),
            MakeDto(4, "ITV",         "Entertainment", "UK", isFavourite: true),
        };

        var filter = new ChannelFilter { SearchText = "BBC", FavouritesOnly = true };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == 1);
        Assert.Contains(result, c => c.Id == 3);
    }

    [Fact]
    public void ApplyFilter_NoChannelsMatchFilter_ReturnsEmptyList()
    {
        var svc = BuildService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", "Entertainment", "UK"),
            MakeDto(2, "ITV",     "Entertainment", "UK"),
        };

        var filter = new ChannelFilter { Category = "Sports" };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Empty(result);
    }

    [Fact]
    public void ApplyFilter_EmptyChannelList_ReturnsEmptyList()
    {
        var svc = BuildService();
        var filter = new ChannelFilter { Category = "Sports", SearchText = "BBC" };
        var result = svc.ApplyFilter([], filter);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ToggleFavouriteAsync_StorageThrows_RevertsIsFavouriteToFalse()
    {
        var domainChannels = new List<Channel>
        {
            new() { Id = 1, Name = "BBC One", Category = "Entertainment", Country = "UK", ChannelNumber = 1 }
        };

        var svc = BuildService(domainChannels, new ThrowingFavouritesRepository());

        var channels = await svc.GetAllAsync();
        var channel = channels.Single(c => c.Id == 1);

        Assert.False(channel.IsFavourite, "Channel should start as not-favourite.");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.ToggleFavouriteAsync(1));

        Assert.False(channel.IsFavourite,
            "IsFavourite must be reverted to false after a storage failure.");
    }

    [Fact]
    public async Task ToggleFavouriteAsync_StorageThrows_RevertsAlreadyFavouritedChannel()
    {
        var domainChannels = new List<Channel>
        {
            new() { Id = 1, Name = "BBC One", Category = "Entertainment", Country = "UK", ChannelNumber = 1 },
            new() { Id = 2, Name = "ITV",     Category = "Entertainment", Country = "UK", ChannelNumber = 2 },
        };

        var favRepo = new SeededThrowingFavouritesRepository(seedIds: [1]);
        var svc = BuildService(domainChannels, favRepo);

        var channels = await svc.GetAllAsync();
        var ch1 = channels.Single(c => c.Id == 1);

        Assert.True(ch1.IsFavourite, "Channel 1 should start as favourite (seeded).");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.ToggleFavouriteAsync(1));

        Assert.True(ch1.IsFavourite,
            "IsFavourite must be reverted back to true after a storage failure.");
    }

    private sealed class SeededThrowingFavouritesRepository : IFavouritesRepository
    {
        private readonly HashSet<int> _seedIds;
        public SeededThrowingFavouritesRepository(HashSet<int> seedIds) => _seedIds = seedIds;
        public Task<HashSet<int>> GetFavouriteIdsAsync() => Task.FromResult(new HashSet<int>(_seedIds));
        public Task SaveFavouriteIdsAsync(HashSet<int> ids) =>
            throw new InvalidOperationException("Storage failure");
    }
}
