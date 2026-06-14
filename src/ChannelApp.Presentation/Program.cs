using Blazored.LocalStorage;
using ChannelApp.Application.Interfaces;
using ChannelApp.Application.Services;
using ChannelApp.Infrastructure.Repositories;
using ChannelApp.Presentation;
using ChannelApp.Presentation.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IChannelRepository, JsonChannelRepository>();
builder.Services.AddScoped<IFavouritesRepository, LocalStorageFavouritesRepository>();
builder.Services.AddScoped<IChannelService, ChannelService>();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<TextSizeService>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
