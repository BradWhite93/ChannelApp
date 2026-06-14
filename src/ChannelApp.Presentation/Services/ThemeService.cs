using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace ChannelApp.Presentation.Services;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILocalStorageService _localStorage;
    private const string StorageKey = "channelApp.theme";

    public string CurrentTheme { get; private set; } = "normal";

    public ThemeService(IJSRuntime jsRuntime, ILocalStorageService localStorage)
    {
        _jsRuntime = jsRuntime;
        _localStorage = localStorage;
    }

    public async Task LoadAsync()
    {
        try
        {
            var stored = await _localStorage.GetItemAsync<string>(StorageKey);
            CurrentTheme = stored ?? "normal";
        }
        catch
        {
            CurrentTheme = "normal";
        }

        await ApplyThemeAsync(CurrentTheme);
    }

    public async Task SetAsync(string theme)
    {
        CurrentTheme = theme;
        await _localStorage.SetItemAsync(StorageKey, theme);
        await ApplyThemeAsync(theme);
    }

    private async Task ApplyThemeAsync(string theme)
    {
        await _jsRuntime.InvokeVoidAsync("eval",
            $"document.documentElement.setAttribute('data-theme', '{theme}')");
    }
}
