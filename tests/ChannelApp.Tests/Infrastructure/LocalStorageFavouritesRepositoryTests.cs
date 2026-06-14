using ChannelApp.Application.DTOs;
using ChannelApp.Application.Interfaces;
using ChannelApp.Application.Services;
using ChannelApp.Domain.Entities;
using ChannelApp.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChannelApp.Tests.Infrastructure;

file sealed class FakeChannelRepository : IChannelRepository
{
    private readonly List<Channel> _channels;

    public FakeChannelRepository(List<Channel> channels) => _channels = channels;

    public Task<List<Channel>> GetAllAsync() => Task.FromResult(_channels);
}

file sealed class SpyFavouritesRepository : IFavouritesRepository
{
    private HashSet<int> _stored = new();

    public HashSet<int> LastSaved => _stored;

    public Task<HashSet<int>> GetFavouriteIdsAsync() => Task.FromResult(new HashSet<int>(_stored));

    public Task SaveFavouriteIdsAsync(HashSet<int> ids)
    {
        _stored = new HashSet<int>(ids);
        return Task.CompletedTask;
    }
}

public class LocalStorageFavouritesRepositoryTests
{
    [Fact]
    public void SaveAndLoad_EmptySet_ReturnsEmpty()
    {
        var localStorage = new InMemoryLocalStorage();
        var logger = NullLogger<LocalStorageFavouritesRepository>.Instance;
        var repo = new LocalStorageFavouritesRepository(localStorage, logger);

        var ids = new HashSet<int>();
        repo.SaveFavouriteIdsAsync(ids).GetAwaiter().GetResult();
        var loaded = repo.GetFavouriteIdsAsync().GetAwaiter().GetResult();

        Assert.True(ids.SetEquals(loaded));
    }

    [Fact]
    public void SaveAndLoad_SingleId_ReturnsSameId()
    {
        var localStorage = new InMemoryLocalStorage();
        var logger = NullLogger<LocalStorageFavouritesRepository>.Instance;
        var repo = new LocalStorageFavouritesRepository(localStorage, logger);

        var ids = new HashSet<int> { 42 };
        repo.SaveFavouriteIdsAsync(ids).GetAwaiter().GetResult();
        var loaded = repo.GetFavouriteIdsAsync().GetAwaiter().GetResult();

        Assert.True(ids.SetEquals(loaded));
    }

    [Fact]
    public void SaveAndLoad_MultipleIds_ReturnsSameIds()
    {
        var localStorage = new InMemoryLocalStorage();
        var logger = NullLogger<LocalStorageFavouritesRepository>.Instance;
        var repo = new LocalStorageFavouritesRepository(localStorage, logger);

        var ids = new HashSet<int> { 1, 5, 10, 100, 9999 };
        repo.SaveFavouriteIdsAsync(ids).GetAwaiter().GetResult();
        var loaded = repo.GetFavouriteIdsAsync().GetAwaiter().GetResult();

        Assert.True(ids.SetEquals(loaded));
    }

    [Fact]
    public void SaveAndLoad_OverwritesPreviousValue()
    {
        var localStorage = new InMemoryLocalStorage();
        var logger = NullLogger<LocalStorageFavouritesRepository>.Instance;
        var repo = new LocalStorageFavouritesRepository(localStorage, logger);

        var firstIds = new HashSet<int> { 1, 2, 3 };
        repo.SaveFavouriteIdsAsync(firstIds).GetAwaiter().GetResult();

        var secondIds = new HashSet<int> { 10, 20 };
        repo.SaveFavouriteIdsAsync(secondIds).GetAwaiter().GetResult();

        var loaded = repo.GetFavouriteIdsAsync().GetAwaiter().GetResult();
        Assert.True(secondIds.SetEquals(loaded));
    }

    [Fact]
    public void ToggleFavourite_OnlyPersistsIdsInChannelCollection()
    {
        var channels = Enumerable.Range(1, 5)
            .Select(id => new Channel
            {
                Id = id,
                Name = $"Channel {id}",
                Categories = new List<string> { "Test" },
                Country = "TestCountry",
                ChannelNumber = id,
            })
            .ToList();

        var spyRepo = new SpyFavouritesRepository();
        var channelRepo = new FakeChannelRepository(channels);
        var service = new ChannelService(channelRepo, spyRepo);

        service.GetAllAsync().GetAwaiter().GetResult();

        // Toggle channel 3
        service.ToggleFavouriteAsync(3).GetAwaiter().GetResult();

        var validIds = new HashSet<int> { 1, 2, 3, 4, 5 };
        foreach (var savedId in spyRepo.LastSaved)
        {
            Assert.Contains(savedId, validIds);
        }

        Assert.Contains(3, spyRepo.LastSaved);
    }

    [Fact]
    public void ToggleFavourite_MultipleTimes_TogglesCorrectly()
    {
        var channels = Enumerable.Range(1, 3)
            .Select(id => new Channel
            {
                Id = id,
                Name = $"Channel {id}",
                Categories = new List<string> { "Test" },
                Country = "TestCountry",
                ChannelNumber = id,
            })
            .ToList();

        var spyRepo = new SpyFavouritesRepository();
        var channelRepo = new FakeChannelRepository(channels);
        var service = new ChannelService(channelRepo, spyRepo);

        service.GetAllAsync().GetAwaiter().GetResult();

        // Toggle on
        service.ToggleFavouriteAsync(2).GetAwaiter().GetResult();
        Assert.Contains(2, spyRepo.LastSaved);

        // Toggle off
        service.ToggleFavouriteAsync(2).GetAwaiter().GetResult();
        Assert.DoesNotContain(2, spyRepo.LastSaved);
    }
}
