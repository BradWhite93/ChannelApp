namespace ChannelApp.Application.DTOs;

public class ChannelFilter
{
    public string? Category { get; set; }
    public string? Country { get; set; }
    public bool FavouritesOnly { get; set; }
    public string? SearchText { get; set; }
    public bool SortByPopularity { get; set; }
}
