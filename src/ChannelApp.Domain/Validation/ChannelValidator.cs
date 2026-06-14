using ChannelApp.Domain.Entities;

namespace ChannelApp.Domain.Validation;

public static class ChannelValidator
{
    public static Channel Validate(Channel channel)
    {
        if (channel is null)
            throw new ArgumentNullException(nameof(channel), "Channel must not be null.");

        if (channel.Id <= 0)
            throw new ArgumentException(
                $"Channel Id must be greater than 0, but was {channel.Id}.",
                nameof(channel));

        if (string.IsNullOrWhiteSpace(channel.Name))
            throw new ArgumentException(
                "Channel Name must not be null or whitespace.",
                nameof(channel));

        if (channel.Categories is null || channel.Categories.Count == 0)
            throw new ArgumentException(
                "Channel Categories must not be null or empty.",
                nameof(channel));

        foreach (var category in channel.Categories)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException(
                    "Each category must not be null or whitespace.",
                    nameof(channel));

            if (category.Length > 100)
                throw new ArgumentException(
                    $"Each category must not exceed 100 characters, but was {category.Length} characters.",
                    nameof(channel));
        }

        if (string.IsNullOrWhiteSpace(channel.Country))
            throw new ArgumentException(
                "Channel Country must not be null or whitespace.",
                nameof(channel));

        return channel;
    }

    public static IReadOnlyList<Channel> ValidateAll(IEnumerable<Channel> channels)
    {
        if (channels is null)
            throw new ArgumentNullException(nameof(channels), "Channel collection must not be null.");

        var result = new List<Channel>();

        foreach (var channel in channels)
        {
            Validate(channel); // throws on first invalid record
            result.Add(channel);
        }

        return result.AsReadOnly();
    }
}
