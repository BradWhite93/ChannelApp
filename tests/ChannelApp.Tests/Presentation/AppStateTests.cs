using ChannelApp.Application.DTOs;
using ChannelApp.Presentation.Services;

namespace ChannelApp.Tests.Presentation;

public class AppStateTests
{
    private static ChannelDto MakeDto(int id, string name, string category, int channelNumber, string country, bool playback = false)
        => new()
        {
            Id = id,
            Name = name,
            Category = category,
            ChannelNumber = channelNumber,
            Country = country,
            Playback = playback,
            IsFavourite = false
        };

    [Fact]
    public void SetFavourite_UpdatesAllEntriesWithSameId()
    {
        var appState = new AppState();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", "Sports", 1, "UK"),
            MakeDto(1, "Ch1", "News", 2, "UK"),   // duplicate ID, different category
            MakeDto(2, "Ch2", "News", 3, "US"),
            MakeDto(3, "Ch3", "Entertainment", 4, "UK"),
        };
        appState.Initialise(channels, new HashSet<int>());

        var changeCount = 0;
        appState.OnChange += () => changeCount++;

        // Set favourite for id=1
        changeCount = 0;
        appState.SetFavourite(1, true);

        Assert.Equal(1, changeCount);

        foreach (var ch in appState.AllChannels)
        {
            if (ch.Id == 1)
            {
                Assert.True(ch.IsFavourite,
                    $"Channel Id=1 should be favourite after SetFavourite(1, true) " +
                    $"but found IsFavourite=false for entry with Category='{ch.Category}'");
            }
        }

        // Unfavourite id=1
        changeCount = 0;
        appState.SetFavourite(1, false);

        Assert.Equal(1, changeCount);

        foreach (var ch in appState.AllChannels)
        {
            if (ch.Id == 1)
            {
                Assert.False(ch.IsFavourite,
                    $"Channel Id=1 should NOT be favourite after SetFavourite(1, false) " +
                    $"but found IsFavourite=true for entry with Category='{ch.Category}'");
            }
        }
    }

    [Fact]
    public void SetFavourite_DoesNotAffectOtherChannels()
    {
        var appState = new AppState();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", "Sports", 1, "UK"),
            MakeDto(2, "Ch2", "News", 2, "UK"),
            MakeDto(3, "Ch3", "Entertainment", 3, "US"),
        };
        appState.Initialise(channels, new HashSet<int>());

        appState.SetFavourite(2, true);

        Assert.False(appState.AllChannels.First(c => c.Id == 1).IsFavourite);
        Assert.True(appState.AllChannels.First(c => c.Id == 2).IsFavourite);
        Assert.False(appState.AllChannels.First(c => c.Id == 3).IsFavourite);
    }

    [Fact]
    public void Initialise_WithFavouriteIds_StoresFavouriteIdsSet()
    {
        var appState = new AppState();
        var channels = new List<ChannelDto>
        {
            MakeDto(1, "Ch1", "Sports", 1, "UK"),
            MakeDto(2, "Ch2", "News", 2, "UK"),
            MakeDto(3, "Ch3", "Entertainment", 3, "US"),
        };
        appState.Initialise(channels, new HashSet<int> { 1, 3 });

        Assert.Contains(1, appState.FavouriteIds);
        Assert.DoesNotContain(2, appState.FavouriteIds);
        Assert.Contains(3, appState.FavouriteIds);
        Assert.True(appState.IsLoaded);
    }
}
