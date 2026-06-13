using CsCheck;
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
    public void SaveAndLoad_ReturnsSameIds()
    {
        Gen.Int[1, 9999].HashSet[0, 50]
            .Sample(ids =>
            {
                var localStorage = new InMemoryLocalStorage();
                var logger = NullLogger<LocalStorageFavouritesRepository>.Instance;
                var repo = new LocalStorageFavouritesRepository(localStorage, logger);

                repo.SaveFavouriteIdsAsync(ids).GetAwaiter().GetResult();
                var loaded = repo.GetFavouriteIdsAsync().GetAwaiter().GetResult();

                Assert.True(ids.SetEquals(loaded),
                    $"Round-trip failed. Saved {ids.Count} IDs, got back {loaded.Count}. " +
                    $"Missing: [{string.Join(",", ids.Except(loaded).Take(5))}], " +
                    $"Extra: [{string.Join(",", loaded.Except(ids).Take(5))}]");
            });
    }

    [Fact]
    public void ToggleFavourite_OnlyPersistsIdsInChannelCollection()
    {
        var genScenario = Gen.Select(
            Gen.Int[3, 20],
            Gen.Int[1, 9999],
            (channelCount, staleSeed) => (channelCount, staleSeed));

        genScenario.Sample(scenario =>
        {
            var (channelCount, staleSeed) = scenario;

            var channels = Enumerable.Range(1, channelCount)
                .Select(id => new Channel
                {
                    Id = id,
                    Name = $"Channel {id}",
                    Category = "Test",
                    Country = "TestCountry",
                    ChannelNumber = id,
                    Playback = true
                })
                .ToList();

            var spyRepo = new SpyFavouritesRepository();
            var channelRepo = new FakeChannelRepository(channels);
            var service = new ChannelService(channelRepo, spyRepo);

            service.GetAllAsync().GetAwaiter().GetResult();

            var toggleId = (staleSeed % channelCount) + 1;
            service.ToggleFavouriteAsync(toggleId).GetAwaiter().GetResult();

            var validIds = Enumerable.Range(1, channelCount).ToHashSet();
            foreach (var savedId in spyRepo.LastSaved)
            {
                Assert.True(validIds.Contains(savedId),
                    $"Stale ID {savedId} was persisted but is not in the valid channel set [1..{channelCount}]");
            }
        });
    }
}
