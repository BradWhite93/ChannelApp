using Blazored.LocalStorage;
using CsCheck;

namespace ChannelApp.Tests.Infrastructure;

public class TextSizePreferenceTests
{
    private const string Key = "channelApp.textSizeMultiplier";

    [Fact]
    public void TextSizeMultiplier_SaveAndLoad_ReturnsSameValue()
    {
        Gen.OneOfConst(1.0, 1.25, 1.5)
            .Sample(multiplier =>
            {
                var storage = new InMemoryLocalStorage();

                storage.SetItemAsync(Key, multiplier).GetAwaiter().GetResult();

                var loaded = storage.GetItemAsync<double>(Key).GetAwaiter().GetResult();

                Assert.Equal(multiplier, loaded);
            });
    }
}
