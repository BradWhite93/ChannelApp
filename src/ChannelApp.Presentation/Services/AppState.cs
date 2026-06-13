using ChannelApp.Application.DTOs;

namespace ChannelApp.Presentation.Services;

public class AppState
{
    public List<ChannelDto> AllChannels { get; private set; } = [];
    public HashSet<int> FavouriteIds { get; private set; } = [];
    public bool IsLoaded { get; private set; }
    public string? LoadError { get; private set; }

    public event Action? OnChange;

    public void Initialise(List<ChannelDto> channels, HashSet<int> favouriteIds)
    {
        AllChannels = channels;
        FavouriteIds = favouriteIds;
        IsLoaded = true;
        LoadError = null;
        OnChange?.Invoke();
    }

    public void SetFavourite(int channelId, bool isFavourite)
    {
        if (isFavourite)
        {
            FavouriteIds.Add(channelId);
        }
        else
        {
            FavouriteIds.Remove(channelId);
        }

        foreach (var channel in AllChannels)
        {
            if (channel.Id == channelId)
            {
                channel.IsFavourite = isFavourite;
            }
        }

        OnChange?.Invoke();
    }

    public void SetLoadError(string message)
    {
        LoadError = message;
        OnChange?.Invoke();
    }
}
