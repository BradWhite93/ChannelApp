using ChannelApp.Domain.Entities;
using ChannelApp.Domain.Validation;

namespace ChannelApp.Tests.Domain;

public class ChannelValidatorTests
{
    [Theory]
    [InlineData(1, "BBC One", "Entertainment", "UK")]
    [InlineData(42, "CNN", "News", "US")]
    [InlineData(999, "Sky Sports", "Sports", "GB")]
    public void Validate_ValidChannel_ReturnsSameInstance(int id, string name, string category, string country)
    {
        var channel = new Channel
        {
            Id = id,
            Name = name,
            Category = category,
            Country = country,
        };

        var result = ChannelValidator.Validate(channel);

        Assert.Same(channel, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Validate_ZeroOrNegativeId_Throws(int id)
    {
        var channel = new Channel
        {
            Id = id,
            Name = "Valid Name",
            Category = "News",
            Country = "UK",
        };

        Assert.Throws<ArgumentException>(() => ChannelValidator.Validate(channel));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("   ")]
    public void Validate_BlankName_Throws(string name)
    {
        var channel = new Channel
        {
            Id = 1,
            Name = name,
            Category = "News",
            Country = "UK",
        };

        Assert.Throws<ArgumentException>(() => ChannelValidator.Validate(channel));
    }

    [Fact]
    public void Validate_CategoryExceedsMaxLength_Throws()
    {
        var longCategory = new string('A', 101);

        var channel = new Channel
        {
            Id = 1,
            Name = "BBC One",
            Category = longCategory,
            Country = "UK",
        };

        Assert.Throws<ArgumentException>(() => ChannelValidator.Validate(channel));
    }

    [Fact]
    public void Validate_CategoryAtMaxLength_DoesNotThrow()
    {
        var maxCategory = new string('B', 100);

        var channel = new Channel
        {
            Id = 1,
            Name = "BBC One",
            Category = maxCategory,
            Country = "UK",
        };

        var result = ChannelValidator.Validate(channel);

        Assert.Same(channel, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("   ")]
    public void Validate_BlankCountry_Throws(string country)
    {
        var channel = new Channel
        {
            Id = 1,
            Name = "BBC One",
            Category = "News",
            Country = country,
        };

        Assert.Throws<ArgumentException>(() => ChannelValidator.Validate(channel));
    }

    [Fact]
    public void Validate_NullChannel_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ChannelValidator.Validate(null!));
    }
}
