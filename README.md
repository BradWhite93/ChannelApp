# Channel App

A Progressive Web App (PWA) for browsing TV channels, built with Blazor WebAssembly and .NET 8. Designed for elderly users with large text, large touch targets, and simple one-task-per-screen navigation.

## Features

- Browse channels by category, country, or view all
- Mark channels as favourites with persistent local storage
- Text size adjustment (Normal, Large, Largest) via settings overlay
- Dark mode toggle (normal dark purple-grey theme / true dark mode)
- Settings accessible via ⚙️ cog icon (top-right) — opens as a bottom-sheet overlay
- Navigation home via 🏠 icon (top-left, always visible) and bottom nav bar
- Playback filter and search
- Fully offline after initial install — no server dependency
- Installable as a standalone app on Android phones

## Architecture

The solution follows Clean Architecture with four layers enforced at compile time:

| Layer | Project | Depends On |
|-------|---------|-----------|
| Domain | `ChannelApp.Domain` | (none) |
| Application | `ChannelApp.Application` | Domain |
| Infrastructure | `ChannelApp.Infrastructure` | Application |
| Presentation | `ChannelApp.Presentation` | Application + Infrastructure (composition root only) |

See [ARCHITECTURE.md](ARCHITECTURE.md) for details.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run locally

```bash
dotnet run --project src/ChannelApp.Presentation
```

Open `https://localhost:7109` in your browser.

### Run tests

```bash
dotnet test tests/ChannelApp.Tests
```

### Publish for deployment

```bash
dotnet publish src/ChannelApp.Presentation -c Release -o publish_output
```

The published output in `publish_output/wwwroot` can be served by any static file host.

## Tech Stack

- **Blazor WebAssembly** — C# in the browser, PWA support built-in
- **Blazored.LocalStorage** — persistent favourites, text size, and theme preferences
- **CsCheck** — property-based testing
- **bUnit** — Blazor component testing
- **xUnit** — test runner

## Channel Data

Channel data is bundled as a static JSON file at `wwwroot/data/channels.json`. Edit this file to add or modify channels. The schema per record:

```json
{
  "id": 1,
  "name": "BBC One",
  "category": "Entertainment",
  "channelNumber": 1,
  "country": "United Kingdom",
  "playback": true
}
```

A channel can appear in multiple categories by including it as multiple records with different `category` values sharing the same `id`.

## License

[MIT](LICENSE)
