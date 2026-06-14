using ChannelApp.Application.DTOs;
using ChannelApp.Application.Interfaces;
using ChannelApp.Application.Services;
using ChannelApp.Domain.Entities;

namespace ChannelApp.Tests.Application;

file sealed class StubChannelRepository : IChannelRepository
{
    public Task<List<Channel>> GetAllAsync() => Task.FromResult(new List<Channel>());
}

file sealed class StubFavouritesRepository : IFavouritesRepository
{
    public Task<HashSet<int>> GetFavouriteIdsAsync() => Task.FromResult(new HashSet<int>());
    public Task SaveFavouriteIdsAsync(HashSet<int> ids) => Task.CompletedTask;
}

public class ChannelServiceTests
{
    private static ChannelService CreateService() =>
        new ChannelService(new StubChannelRepository(), new StubFavouritesRepository());

    private static ChannelDto MakeDto(int id, string name, List<string> categories, string country,
        int channelNumber, bool isFavourite = false)
        => new() { Id = id, Name = name, Categories = categories, Country = country,
                   ChannelNumber = channelNumber, IsFavourite = isFavourite };

    [Fact]
    public void GetDistinctCategories_ReturnsUniqueValuesSortedAlphabetically()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", new List<string> { "Sports" }, "UK", 1),
            MakeDto(2, "Ch2", new List<string> { "News" }, "UK", 2),
            MakeDto(3, "Ch3", new List<string> { "Sports" }, "US", 3),
            MakeDto(4, "Ch4", new List<string> { "Entertainment" }, "UK", 4),
            MakeDto(5, "Ch5", new List<string> { "NEWS" }, "US", 5),
        };

        var result = svc.GetDistinctCategories(channels);

        Assert.Equal(3, result.Count);
        // Sorted alphabetically (case-insensitive), deduplicated
        Assert.Equal("Entertainment", result[0]);
        Assert.Equal("News", result[1]);
        Assert.Equal("Sports", result[2]);
    }

    [Fact]
    public void ApplyFilter_CategoryFilter_ReturnsOnlyMatchingCategory()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", new List<string> { "Entertainment" }, "UK", 1),
            MakeDto(2, "Sky Sports", new List<string> { "Sports" }, "UK", 2),
            MakeDto(3, "CNN", new List<string> { "News" }, "US", 3),
            MakeDto(4, "ESPN", new List<string> { "Sports" }, "US", 4),
            MakeDto(5, "ITV", new List<string> { "Entertainment" }, "UK", 5),
        };

        var filter = new ChannelFilter { Category = "Sports" };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Equal(2, result.Count);
        Assert.All(result, ch => Assert.Contains("Sports", ch.Categories));
        Assert.Contains(result, c => c.Id == 2);
        Assert.Contains(result, c => c.Id == 4);
    }

    [Fact]
    public void ApplyFilter_CategoryFilter_IsCaseInsensitive()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", new List<string> { "NEWS" }, "UK", 1),
            MakeDto(2, "Ch2", new List<string> { "news" }, "UK", 2),
            MakeDto(3, "Ch3", new List<string> { "News" }, "UK", 3),
            MakeDto(4, "Ch4", new List<string> { "Sports" }, "UK", 4),
        };

        var filter = new ChannelFilter { Category = "news" };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyFilter_CategoryFilter_MatchesChannelWithMultipleCategories()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", new List<string> { "Entertainment", "News" }, "UK", 1),
            MakeDto(2, "Sky Sports", new List<string> { "Sports" }, "UK", 2),
        };

        var filterNews = new ChannelFilter { Category = "News" };
        var resultNews = svc.ApplyFilter(channels, filterNews);
        Assert.Single(resultNews);
        Assert.Equal(1, resultNews[0].Id);

        var filterEntertainment = new ChannelFilter { Category = "Entertainment" };
        var resultEntertainment = svc.ApplyFilter(channels, filterEntertainment);
        Assert.Single(resultEntertainment);
        Assert.Equal(1, resultEntertainment[0].Id);
    }

    [Fact]
    public void ApplyFilter_CountryFilter_ReturnsOnlyMatchingCountry()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", new List<string> { "Entertainment" }, "UK", 1),
            MakeDto(2, "CNN", new List<string> { "News" }, "US", 2),
            MakeDto(3, "Sky", new List<string> { "Sports" }, "UK", 3),
            MakeDto(4, "Fox", new List<string> { "News" }, "US", 4),
        };

        var filter = new ChannelFilter { Country = "US" };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Equal(2, result.Count);
        Assert.All(result, ch => Assert.Equal("US", ch.Country, StringComparer.OrdinalIgnoreCase));
        Assert.Contains(result, c => c.Id == 2);
        Assert.Contains(result, c => c.Id == 4);
    }

    [Fact]
    public void ApplyFilter_FavouritesOnly_ReturnsOnlyFavouritedChannels()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", new List<string> { "Entertainment" }, "UK", 1, isFavourite: true),
            MakeDto(2, "CNN", new List<string> { "News" }, "US", 2, isFavourite: false),
            MakeDto(3, "Sky", new List<string> { "Sports" }, "UK", 3, isFavourite: true),
            MakeDto(4, "Fox", new List<string> { "News" }, "US", 4, isFavourite: false),
        };

        var filter = new ChannelFilter { FavouritesOnly = true };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Equal(2, result.Count);
        Assert.All(result, ch => Assert.True(ch.IsFavourite));
        Assert.Contains(result, c => c.Id == 1);
        Assert.Contains(result, c => c.Id == 3);
    }

    [Fact]
    public void ApplyFilter_FavouritesNotSet_ReturnsAllChannels()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", new List<string> { "Entertainment" }, "UK", 1, isFavourite: true),
            MakeDto(2, "CNN", new List<string> { "News" }, "US", 2, isFavourite: false),
            MakeDto(3, "Sky", new List<string> { "Sports" }, "UK", 3, isFavourite: true),
        };

        var filter = new ChannelFilter { FavouritesOnly = false };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyFilter_EmptyFilter_ResultsSortedByChannelNumber()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", new List<string> { "News" }, "UK", 50),
            MakeDto(2, "Ch2", new List<string> { "Sports" }, "UK", 10),
            MakeDto(3, "Ch3", new List<string> { "Entertainment" }, "US", 30),
            MakeDto(4, "Ch4", new List<string> { "News" }, "US", 5),
            MakeDto(5, "Ch5", new List<string> { "Sports" }, "UK", 99),
        };

        var filter = new ChannelFilter();
        var result = svc.ApplyFilter(channels, filter);

        Assert.Equal(5, result.Count);
        Assert.Equal(5, result[0].ChannelNumber);
        Assert.Equal(10, result[1].ChannelNumber);
        Assert.Equal(30, result[2].ChannelNumber);
        Assert.Equal(50, result[3].ChannelNumber);
        Assert.Equal(99, result[4].ChannelNumber);
    }

    [Fact]
    public void ApplyFilter_SearchText_ReturnsOnlyChannelsMatchingName()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", new List<string> { "Entertainment" }, "UK", 1),
            MakeDto(2, "BBC Two", new List<string> { "Entertainment" }, "UK", 2),
            MakeDto(3, "Sky Sports", new List<string> { "Sports" }, "UK", 3),
            MakeDto(4, "CNN", new List<string> { "News" }, "US", 4),
        };

        var filter = new ChannelFilter { SearchText = "BBC" };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Equal(2, result.Count);
        Assert.All(result, ch => Assert.Contains("BBC", ch.Name, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result, c => c.Id == 1);
        Assert.Contains(result, c => c.Id == 2);
    }

    [Fact]
    public void ApplyFilter_SearchText_IsCaseInsensitive()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", new List<string> { "News" }, "UK", 1),
            MakeDto(2, "bbc two", new List<string> { "News" }, "UK", 2),
            MakeDto(3, "Sky", new List<string> { "News" }, "UK", 3),
        };

        var filter = new ChannelFilter { SearchText = "bbc" };
        var result = svc.ApplyFilter(channels, filter);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ApplyFilter_DuplicateChannelIds_ReturnsNoDuplicates()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "BBC One", new List<string> { "Entertainment" }, "UK", 1),
            MakeDto(1, "BBC One HD", new List<string> { "Entertainment" }, "UK", 101),
            MakeDto(2, "CNN", new List<string> { "News" }, "US", 2),
            MakeDto(2, "CNN HD", new List<string> { "News" }, "US", 102),
            MakeDto(3, "Sky", new List<string> { "Sports" }, "UK", 3),
        };

        var filter = new ChannelFilter();
        var result = svc.ApplyFilter(channels, filter);

        var ids = result.Select(c => c.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void ApplyFilter_DuplicateIdsWithCategoryFilter_ReturnsNoDuplicates()
    {
        var svc = CreateService();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", new List<string> { "Sports" }, "UK", 1),
            MakeDto(1, "Ch1 HD", new List<string> { "Sports" }, "UK", 101),
            MakeDto(2, "Ch2", new List<string> { "Sports" }, "US", 2),
            MakeDto(3, "Ch3", new List<string> { "News" }, "UK", 3),
        };

        var filter = new ChannelFilter { Category = "Sports" };
        var result = svc.ApplyFilter(channels, filter);

        var ids = result.Select(c => c.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
