# Architecture

## Overview

The Channel App follows Clean Architecture with four layers arranged in a strict dependency hierarchy. Each layer is implemented as a separate .NET project within the `ChannelApp.sln` solution. Dependencies flow inward only — outer layers depend on inner layers, never the reverse.

## Layers

### Domain Layer (`ChannelApp.Domain`)

- **Dependencies:** None
- **Example class:** `Channel` (entity representing a TV channel with Id, Name, Category, Country, ChannelNumber, and Playback fields)
- **Example class:** `ChannelValidator` (static validation logic that enforces data schema constraints)

### Application Layer (`ChannelApp.Application`)

- **Dependencies:** Domain only
- **Example class:** `ChannelService` (use-case orchestrator handling filtering, sorting, de-duplication, and favourite toggling)
- **Example interface:** `IChannelRepository` (defines the contract for loading channel data)

### Infrastructure Layer (`ChannelApp.Infrastructure`)

- **Dependencies:** Application only
- **Example class:** `JsonChannelRepository` (loads channel data from a bundled JSON file via HttpClient)
- **Example interface implemented:** `IFavouritesRepository` (via `LocalStorageFavouritesRepository`, persists favourite IDs to browser localStorage)

### Presentation Layer (`ChannelApp.Presentation`)

- **Dependencies:** Application + Infrastructure (Infrastructure is referenced only in the composition root)
- **Example component:** `Home.razor` (the landing page with navigation tiles)
- **Example service:** `AppState` (scoped service holding in-memory channel collection and firing change events for cross-component reactivity)
- **Example service:** `ThemeService` (manages normal/dark theme mode, persisted to localStorage)

## Dependency Rules

| Layer          | Allowed Dependencies                                      |
|----------------|-----------------------------------------------------------|
| Domain         | (none)                                                    |
| Application    | Domain only                                               |
| Infrastructure | Application only                                          |
| Presentation   | Application + Infrastructure (composition root only)      |

## Enforcement

Dependency rules are enforced at compile time via `<ProjectReference>` entries in each `.csproj` file. Any attempt to add a forbidden reference produces a build error.

- `ChannelApp.Domain.csproj` has no `<ProjectReference>` entries.
- `ChannelApp.Application.csproj` references only `ChannelApp.Domain`.
- `ChannelApp.Infrastructure.csproj` references only `ChannelApp.Application`.
- `ChannelApp.Presentation.csproj` references `ChannelApp.Application` and `ChannelApp.Infrastructure`.

The only file in the Presentation layer that imports from `ChannelApp.Infrastructure` is `Program.cs` (the composition root), which wires interface bindings to their concrete implementations via .NET dependency injection.
