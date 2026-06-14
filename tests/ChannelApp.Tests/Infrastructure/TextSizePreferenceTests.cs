using Blazored.LocalStorage;

namespace ChannelApp.Tests.Infrastructure;

public class TextSizePreferenceTests
{
    private const string Key = "channelApp.textSizeMultiplier";

    [Theory]
    [InlineData(1.0)]
    [InlineData(1.25)]
    [InlineData(1.5)]
    public void TextSizeMultiplier_SaveAndLoad_ReturnsSameValue(double multiplier)
    {
        var storage = new InMemoryLocalStorage();

        storage.SetItemAsync(Key, multiplier).GetAwaiter().GetResult();

        var loaded = storage.GetItemAsync<double>(Key).GetAwaiter().GetResult();

        Assert.Equal(multiplier, loaded);
    }
}
