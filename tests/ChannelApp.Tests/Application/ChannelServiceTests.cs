using CsCheck;
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

    private static ChannelDto MakeDto(int id, string name, string category, string country,
        int channelNumber, bool playback = false, bool isFavourite = false)
        => new() { Id = id, Name = name, Category = category, Country = country,
                   ChannelNumber = channelNumber, Playback = playback, IsFavourite = isFavourite };

    private static readonly Gen<ChannelDto> GenDto =
        Gen.Select(
            Gen.String[1, 30], Gen.String[1, 30], Gen.String[1, 30],
            Gen.Int[1, 999], Gen.Bool, Gen.Bool, Gen.Int[1, 9999],
            (name, cat, country, num, pb, fav, id) => MakeDto(id, name, cat, country, num, pb, fav));

    private static readonly Gen<ChannelDto[]> GenDistinctIdChannels =
        Gen.Int[1, 20].SelectMany(count =>
            Gen.Select(
                Gen.String[1, 30].Array[count],
                Gen.String[1, 30].Array[count],
                Gen.String[1, 30].Array[count],
                Gen.Int[1, 999].Array[count],
                Gen.Bool.Array[count],
                Gen.Bool.Array[count],
                (names, cats, countries, nums, playbacks, favs) =>
                    Enumerable.Range(1, count)
                        .Select(id => MakeDto(id, names[id - 1], cats[id - 1], countries[id - 1], nums[id - 1], playbacks[id - 1], favs[id - 1]))
                        .ToArray()
            ));

    private static readonly Gen<ChannelDto[]> GenSharedIdChannels =
        Gen.Int[1, 30].SelectMany(count =>
            Gen.Select(
                Gen.String[1, 30].Array[count],
                Gen.String[1, 30].Array[count],
                Gen.String[1, 30].Array[count],
                Gen.Int[1, 999].Array[count],
                Gen.Bool.Array[count],
                Gen.Bool.Array[count],
                Gen.Int[1, 5].Array[count],
                (names, cats, countries, nums, playbacks, favs, ids) =>
                    Enumerable.Range(0, count)
                        .Select(i => MakeDto(ids[i], names[i], cats[i], countries[i], nums[i], playbacks[i], favs[i]))
                        .ToArray()
            ));

    [Fact]
    public void GetDistinctCategories_ReturnsUniqueValuesSortedAlphabetically()
    {
        var svc = CreateService();

        GenDto.Array[0, 30]
            .Sample(channels =>
            {
                var categories = svc.GetDistinctCategories(channels);

                var lower = categories.Select(c => c.ToLowerInvariant()).ToList();
                Assert.Equal(lower.Count, lower.Distinct().Count());

                for (int i = 0; i < categories.Count - 1; i++)
                {
                    int cmp = string.Compare(
                        categories[i], categories[i + 1],
                        StringComparison.OrdinalIgnoreCase);
                    Assert.True(cmp <= 0,
                        $"Sort order violated: '{categories[i]}' > '{categories[i + 1]}'");
                }
            });
    }

    [Fact]
    public void ApplyFilter_CategoryFilter_ReturnsOnlyMatchingCategory()
    {
        var svc = CreateService();

        Gen.Select(GenDto.Array[1, 30], Gen.String[1, 30])
            .Sample((channels, filterCat) =>
            {
                var filter = new ChannelFilter { Category = filterCat };
                var result = svc.ApplyFilter(channels, filter);

                foreach (var ch in result)
                    Assert.True(
                        string.Equals(ch.Category, filterCat, StringComparison.OrdinalIgnoreCase),
                        $"Channel category '{ch.Category}' does not match filter '{filterCat}'");

                var matchingIds = channels
                    .Where(c => string.Equals(c.Category, filterCat, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Id)
                    .ToHashSet();
                var resultIds = result.Select(c => c.Id).ToHashSet();
                foreach (var id in matchingIds)
                    Assert.Contains(id, resultIds);
            });
    }

    [Fact]
    public void ApplyFilter_CountryFilter_ReturnsOnlyMatchingCountry()
    {
        var svc = CreateService();

        Gen.Select(GenDto.Array[1, 30], Gen.String[1, 30])
            .Sample((channels, filterCountry) =>
            {
                var filter = new ChannelFilter { Country = filterCountry };
                var result = svc.ApplyFilter(channels, filter);

                foreach (var ch in result)
                    Assert.True(
                        string.Equals(ch.Country, filterCountry, StringComparison.OrdinalIgnoreCase),
                        $"Channel country '{ch.Country}' does not match filter '{filterCountry}'");

                var matchingIds = channels
                    .Where(c => string.Equals(c.Country, filterCountry, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Id)
                    .ToHashSet();
                var resultIds = result.Select(c => c.Id).ToHashSet();
                foreach (var id in matchingIds)
                    Assert.Contains(id, resultIds);
            });
    }

    [Fact]
    public void ApplyFilter_FavouritesOnly_ReturnsOnlyFavouritedChannels()
    {
        var svc = CreateService();

        GenDto.Array[0, 30]
            .Sample(channels =>
            {
                var filter = new ChannelFilter { FavouritesOnly = true };
                var result = svc.ApplyFilter(channels, filter);

                Assert.All(result, ch => Assert.True(ch.IsFavourite,
                    $"Channel Id={ch.Id} Name='{ch.Name}' is not a favourite but was returned"));
            });
    }

    [Fact]
    public void ApplyFilter_FavouritesNotSet_ReturnsAllChannels()
    {
        var svc = CreateService();

        GenDistinctIdChannels
            .Sample(channels =>
            {
                var filter = new ChannelFilter { FavouritesOnly = false };
                var result = svc.ApplyFilter(channels, filter);

                Assert.Equal(channels.Length, result.Count);
            });
    }

    [Fact]
    public void ApplyFilter_EmptyFilter_ResultsSortedByChannelNumber()
    {
        var svc = CreateService();

        GenDistinctIdChannels
            .Sample(channels =>
            {
                var filter = new ChannelFilter();
                var result = svc.ApplyFilter(channels, filter);

                Assert.Equal(channels.Length, result.Count);

                for (int i = 0; i < result.Count - 1; i++)
                {
                    Assert.True(
                        result[i].ChannelNumber <= result[i + 1].ChannelNumber,
                        $"Sort order violated at index {i}: {result[i].ChannelNumber} > {result[i + 1].ChannelNumber}");
                }
            });
    }

    [Fact]
    public void ApplyFilter_PlaybackTrue_ReturnsOnlyPlaybackChannels()
    {
        var svc = CreateService();

        GenDto.Array[0, 30]
            .Sample(channels =>
            {
                var filter = new ChannelFilter { Playback = true };
                var result = svc.ApplyFilter(channels, filter);

                Assert.All(result, ch => Assert.True(ch.Playback,
                    $"Channel Id={ch.Id} has Playback=false but was returned by Playback=true filter"));
            });
    }

    [Fact]
    public void ApplyFilter_PlaybackNull_ReturnsAllChannels()
    {
        var svc = CreateService();

        GenDistinctIdChannels
            .Sample(channels =>
            {
                var filter = new ChannelFilter { Playback = null };
                var result = svc.ApplyFilter(channels, filter);

                Assert.Equal(channels.Length, result.Count);
            });
    }

    [Fact]
    public void ApplyFilter_SearchText_ReturnsOnlyChannelsMatchingName()
    {
        var svc = CreateService();

        Gen.Select(GenDto.Array[0, 30], Gen.String[1, 15])
            .Sample((channels, searchText) =>
            {
                var filter = new ChannelFilter { SearchText = searchText };
                var result = svc.ApplyFilter(channels, filter);

                foreach (var ch in result)
                    Assert.True(
                        ch.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase),
                        $"Name '{ch.Name}' does not contain search text '{searchText}'");

                var matchingIds = channels
                    .Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Id)
                    .ToHashSet();
                var resultIds = result.Select(c => c.Id).ToHashSet();
                foreach (var id in matchingIds)
                    Assert.Contains(id, resultIds);
            });
    }

    [Fact]
    public void ApplyFilter_DuplicateChannelIds_ReturnsNoDuplicates()
    {
        var svc = CreateService();

        GenSharedIdChannels
            .Sample(channels =>
            {
                var filter = new ChannelFilter();
                var result = svc.ApplyFilter(channels, filter);

                var ids = result.Select(c => c.Id).ToList();
                var distinctCount = ids.Distinct().Count();

                Assert.True(distinctCount == ids.Count,
                    $"Duplicate Ids found in result. Total={ids.Count}, Distinct={distinctCount}");
            });
    }

    [Fact]
    public void ApplyFilter_DuplicateIdsWithCategoryFilter_ReturnsNoDuplicates()
    {
        var svc = CreateService();

        Gen.Select(GenSharedIdChannels, Gen.String[1, 20])
            .Sample((channels, filterCat) =>
            {
                var filter = new ChannelFilter { Category = filterCat };
                var result = svc.ApplyFilter(channels, filter);

                var ids = result.Select(c => c.Id).ToList();
                var distinctCount = ids.Distinct().Count();

                Assert.True(distinctCount == ids.Count,
                    $"Duplicate Ids found in filtered result. Total={ids.Count}, Distinct={distinctCount}");
            });
    }
}
