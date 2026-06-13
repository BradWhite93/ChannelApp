using CsCheck;
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

        var genDto = Gen.Select(
            Gen.Int[1, 999],
            Gen.String[1, 20],
            Gen.String[1, 20],
            Gen.Int[1, 999],
            Gen.String[1, 20],
            Gen.Bool,
            (id, name, cat, num, country, pb) => new ChannelDto
            {
                Id = id,
                Name = name,
                Category = cat,
                ChannelNumber = num,
                Country = country,
                Playback = pb,
                IsFavourite = false
            });

        genDto.Array[0, 30].Sample(channels =>
        {
            var categories = svc.GetDistinctCategories(channels);
            Assert.DoesNotContain("Favourites", categories, StringComparer.OrdinalIgnoreCase);
        });
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
