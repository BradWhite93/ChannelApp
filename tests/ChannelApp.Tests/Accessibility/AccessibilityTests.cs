using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

namespace ChannelApp.Tests.Accessibility;

[Trait("Category", "Accessibility")]
public class AccessibilityTests
{
    private const string BaseUrl = "https://localhost:5001";

    [Fact(Skip = "Requires Playwright browsers and a running app instance")]
    public async Task HomeScreen_HasNoAccessibilityViolations()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{BaseUrl}/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var results = await page.RunAxe();
        Assert.Empty(results.Violations);
    }

    [Fact(Skip = "Requires Playwright browsers and a running app instance")]
    public async Task CategoryBrowse_HasNoAccessibilityViolations()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{BaseUrl}/categories");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var results = await page.RunAxe();
        Assert.Empty(results.Violations);
    }

    [Fact(Skip = "Requires Playwright browsers and a running app instance")]
    public async Task ChannelList_HasNoAccessibilityViolations()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{BaseUrl}/channels");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var results = await page.RunAxe();
        Assert.Empty(results.Violations);
    }
}
