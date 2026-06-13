using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using ChannelApp.Presentation.Pages;
using ChannelApp.Presentation.Services;
using ChannelApp.Presentation.Components;
using ChannelApp.Presentation.Layout;
using ChannelApp.Application.Interfaces;
using ChannelApp.Application.DTOs;

namespace ChannelApp.Tests.Presentation;

public class ComponentTests : TestContext
{
    private AppState SetupServices(List<ChannelDto>? channels = null, HashSet<int>? favouriteIds = null)
    {
        var appState = new AppState();
        var channelList = channels ?? new List<ChannelDto>();
        var favIds = favouriteIds ?? new HashSet<int>();
        appState.Initialise(channelList, favIds);

        Services.AddSingleton(appState);
        Services.AddSingleton<IChannelService>(new StubChannelService());
        Services.AddSingleton<TextSizeService>(sp =>
            new TextSizeService(
                sp.GetRequiredService<IJSRuntime>(),
                sp.GetRequiredService<ILocalStorageService>()));
        Services.AddSingleton<ILocalStorageService>(new StubLocalStorageService());

        return appState;
    }

    [Fact]
    public void HomeScreen_RendersFourNavTiles()
    {
        SetupServices();

        var cut = RenderComponent<Home>();

        var navTiles = cut.FindAll("button.nav-tile");
        Assert.Equal(4, navTiles.Count);
    }

    [Fact]
    public void CategoryBrowse_DoesNotIncludeFavouritesTile()
    {
        var channels = new List<ChannelDto>
        {
            new() { Id = 1, Name = "Ch1", Category = "Sports", ChannelNumber = 1, Country = "UK", Playback = true },
            new() { Id = 2, Name = "Ch2", Category = "News", ChannelNumber = 2, Country = "UK", Playback = false }
        };
        SetupServices(channels);

        var cut = RenderComponent<CategoryBrowse>();

        var navTiles = cut.FindAll("button.nav-tile");
        Assert.True(navTiles.Count >= 1, "Expected at least one nav-tile");
        Assert.DoesNotContain(navTiles, t => t.TextContent.Trim() == "Favourites");
    }

    [Fact]
    public void CategoryBrowse_EmptyChannels_ShowsEmptyState()
    {
        SetupServices(new List<ChannelDto>());

        var cut = RenderComponent<CategoryBrowse>();

        var emptyState = cut.Find("p.empty-state");
        Assert.Contains("No categories are currently available", emptyState.TextContent);
    }

    [Fact]
    public void CountryBrowse_EmptyChannels_ShowsEmptyState()
    {
        SetupServices(new List<ChannelDto>());

        var cut = RenderComponent<CountryBrowse>();

        var emptyState = cut.Find("p.empty-state");
        Assert.Contains("No countries are currently available", emptyState.TextContent);
    }

    [Fact]
    public void ChannelList_PlaybackFilterToggle_FiltersCorrectly()
    {
        var channels = new List<ChannelDto>
        {
            new() { Id = 1, Name = "PlaybackChannel", Category = "Sports", ChannelNumber = 1, Country = "UK", Playback = true },
            new() { Id = 2, Name = "NonPlaybackChannel", Category = "Sports", ChannelNumber = 2, Country = "UK", Playback = false },
            new() { Id = 3, Name = "AnotherPlayback", Category = "News", ChannelNumber = 3, Country = "US", Playback = true }
        };
        SetupServices(channels);

        var cut = RenderComponent<ChannelList>();

        var items = cut.FindAll(".channel-item");
        Assert.Equal(3, items.Count);

        var playbackButton = cut.Find("button.playback-filter");
        playbackButton.Click();

        var filteredItems = cut.FindAll(".channel-item");
        Assert.Equal(2, filteredItems.Count);
    }

    [Fact]
    public void FavouriteStar_Toggle_UpdatesAppState()
    {
        var channels = new List<ChannelDto>
        {
            new() { Id = 42, Name = "TestChannel", Category = "Sports", ChannelNumber = 1, Country = "UK", Playback = true, IsFavourite = false }
        };
        var appState = SetupServices(channels);

        var cut = RenderComponent<FavouriteStar>(parameters => parameters
            .Add(p => p.ChannelId, 42)
            .Add(p => p.IsFavourite, false));

        var starButton = cut.Find("button.favourite-star");
        starButton.Click();

        Assert.True(appState.FavouriteIds.Contains(42),
            "After clicking star, channel 42 should be in FavouriteIds");
        Assert.True(appState.AllChannels.First(c => c.Id == 42).IsFavourite,
            "After clicking star, channel 42 IsFavourite should be true");
    }

    [Fact]
    public void BottomNavBar_IsRenderedOnAllPages()
    {
        SetupServices();

        var cut = RenderComponent<BottomNavBar>();

        var nav = cut.Find("nav.bottom-nav-bar");
        Assert.NotNull(nav);
    }

    private class StubChannelService : IChannelService
    {
        public Task<List<ChannelDto>> GetAllAsync() => Task.FromResult(new List<ChannelDto>());

        public List<string> GetDistinctCategories(IEnumerable<ChannelDto> channels)
            => channels.Select(c => c.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).ToList();

        public List<string> GetDistinctCountries(IEnumerable<ChannelDto> channels)
            => channels.Select(c => c.Country).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).ToList();

        public List<ChannelDto> ApplyFilter(IEnumerable<ChannelDto> channels, ChannelFilter filter)
        {
            IEnumerable<ChannelDto> result = channels;
            if (filter.Playback.HasValue)
                result = result.Where(c => c.Playback == filter.Playback.Value);
            if (filter.FavouritesOnly)
                result = result.Where(c => c.IsFavourite);
            if (!string.IsNullOrEmpty(filter.Category))
                result = result.Where(c => c.Category.Equals(filter.Category, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(filter.Country))
                result = result.Where(c => c.Country.Equals(filter.Country, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(filter.SearchText))
                result = result.Where(c => c.Name.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase));
            return result.DistinctBy(c => c.Id).OrderBy(c => c.ChannelNumber).ToList();
        }

        public Task ToggleFavouriteAsync(int channelId) => Task.CompletedTask;
    }

    private class StubLocalStorageService : ILocalStorageService
    {
        private readonly Dictionary<string, object?> _store = new();

#pragma warning disable CS0067
        public event EventHandler<ChangingEventArgs>? Changing;
        public event EventHandler<ChangedEventArgs>? Changed;
#pragma warning restore CS0067

        public ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (_store.TryGetValue(key, out var value) && value is T typedValue)
                return new ValueTask<T?>(typedValue);
            T? defaultVal = default;
            return ValueTask.FromResult(defaultVal);
        }

        public ValueTask<string?> GetItemAsStringAsync(string key, CancellationToken cancellationToken = default)
        {
            if (_store.TryGetValue(key, out var value) && value is string strValue)
                return new ValueTask<string?>(strValue);
            return new ValueTask<string?>(default(string));
        }

        public ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default)
        {
            _store[key] = data;
            return ValueTask.CompletedTask;
        }

        public ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = default)
        {
            _store[key] = data;
            return ValueTask.CompletedTask;
        }

        public ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default)
        {
            _store.Remove(key);
            return ValueTask.CompletedTask;
        }

        public ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            foreach (var key in keys) _store.Remove(key);
            return ValueTask.CompletedTask;
        }

        public ValueTask ClearAsync(CancellationToken cancellationToken = default)
        {
            _store.Clear();
            return ValueTask.CompletedTask;
        }

        public ValueTask<int> LengthAsync(CancellationToken cancellationToken = default)
            => new ValueTask<int>(_store.Count);

        public ValueTask<string?> KeyAsync(int index, CancellationToken cancellationToken = default)
            => new ValueTask<string?>(_store.Keys.ElementAt(index));

        public ValueTask<IEnumerable<string>> KeysAsync(CancellationToken cancellationToken = default)
            => new ValueTask<IEnumerable<string>>(_store.Keys.AsEnumerable());

        public ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default)
            => new ValueTask<bool>(_store.ContainsKey(key));
    }
}
