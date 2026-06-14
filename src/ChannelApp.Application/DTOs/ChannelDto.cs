namespace ChannelApp.Application.DTOs;

public class ChannelDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required List<string> Categories { get; set; }
    public int ChannelNumber { get; set; }
    public required string Country { get; set; }
    public bool Playback { get; set; }
    public bool IsFavourite { get; set; }
}
