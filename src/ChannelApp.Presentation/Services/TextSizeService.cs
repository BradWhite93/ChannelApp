using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace ChannelApp.Presentation.Services;

public class TextSizeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILocalStorageService _localStorage;
    private const string StorageKey = "channelApp.textSizeMultiplier";

    public double CurrentMultiplier { get; private set; } = 1.0;

    public TextSizeService(IJSRuntime jsRuntime, ILocalStorageService localStorage)
    {
        _jsRuntime = jsRuntime;
        _localStorage = localStorage;
    }

    public async Task LoadAsync()
    {
        try
        {
            var stored = await _localStorage.GetItemAsync<double?>(StorageKey);
            CurrentMultiplier = stored ?? 1.0;
        }
        catch
        {
            CurrentMultiplier = 1.0;
        }

        await ApplyCssPropertyAsync(CurrentMultiplier);
    }

    public async Task SetAsync(double multiplier)
    {
        CurrentMultiplier = multiplier;
        await _localStorage.SetItemAsync(StorageKey, multiplier);
        await ApplyCssPropertyAsync(multiplier);
    }

    private async Task ApplyCssPropertyAsync(double multiplier)
    {
        var value = multiplier.ToString(System.Globalization.CultureInfo.InvariantCulture);
        await _jsRuntime.InvokeVoidAsync("eval", $"document.documentElement.style.setProperty('--text-scale', '{value}')");
    }
}
