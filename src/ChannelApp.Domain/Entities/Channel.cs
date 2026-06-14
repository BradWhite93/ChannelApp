namespace ChannelApp.Domain.Entities
{
    public class Channel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required List<string> Categories { get; set; }
        public required string Country { get; set; }
        public int ChannelNumber { get; set; }
        public bool Playback { get; set; }
    }
}
