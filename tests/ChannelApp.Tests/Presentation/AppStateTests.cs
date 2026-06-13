using CsCheck;
using ChannelApp.Application.DTOs;
using ChannelApp.Presentation.Services;

namespace ChannelApp.Tests.Presentation;

public class AppStateTests
{
    private static Gen<ChannelDto> GenChannelDto(Gen<int> idGen) =>
        Gen.Select(
            idGen,
            Gen.String[1, 20],
            Gen.String[1, 20],
            Gen.Int[1, 999],
            Gen.String[1, 20],
            Gen.Bool,
            (id, name, category, channelNumber, country, playback) => new ChannelDto
            {
                Id = id,
                Name = name,
                Category = category,
                ChannelNumber = channelNumber,
                Country = country,
                Playback = playback,
                IsFavourite = false
            });

    private static Gen<ChannelDto[]> GenChannelsWithDuplicates =>
        Gen.Int[1, 10].SelectMany(idPoolSize =>
            GenChannelDto(Gen.Int[1, idPoolSize]).Array[2, 20]);

    [Fact]
    public void SetFavourite_UpdatesAllEntriesWithSameId()
    {
        GenChannelsWithDuplicates.Sample(channels =>
        {
            var appState = new AppState();
            var channelList = channels.ToList();
            appState.Initialise(channelList, new HashSet<int>());

            var targetId = channels[0].Id;

            var changeCount = 0;
            appState.OnChange += () => changeCount++;

            changeCount = 0;
            appState.SetFavourite(targetId, true);

            Assert.Equal(1, changeCount);

            foreach (var ch in appState.AllChannels)
            {
                if (ch.Id == targetId)
                {
                    Assert.True(ch.IsFavourite,
                        $"Channel Id={targetId} should be favourite after SetFavourite(id, true) " +
                        $"but found IsFavourite=false for entry with Category='{ch.Category}'");
                }
            }

            changeCount = 0;
            appState.SetFavourite(targetId, false);

            Assert.Equal(1, changeCount);

            foreach (var ch in appState.AllChannels)
            {
                if (ch.Id == targetId)
                {
                    Assert.False(ch.IsFavourite,
                        $"Channel Id={targetId} should NOT be favourite after SetFavourite(id, false) " +
                        $"but found IsFavourite=true for entry with Category='{ch.Category}'");
                }
            }
        });
    }
}
