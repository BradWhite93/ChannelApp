namespace ChannelApp.Application.Interfaces;

public interface IFavouritesRepository
{
    Task<HashSet<int>> GetFavouriteIdsAsync();
    Task SaveFavouriteIdsAsync(HashSet<int> ids);
}

