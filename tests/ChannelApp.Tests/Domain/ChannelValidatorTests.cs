using CsCheck;
using ChannelApp.Domain.Entities;
using ChannelApp.Domain.Validation;

namespace ChannelApp.Tests.Domain;

public class ChannelValidatorTests
{
    private static readonly Gen<string> GenNonEmptyString =
        Gen.String[1, 100];

    private static readonly Gen<string> GenValidCategory =
        Gen.String[1, 100];

    private static readonly Gen<string> GenLongCategory =
        Gen.String[101, 200];

    private static readonly Gen<int> GenPositiveId =
        Gen.Int[1, int.MaxValue];

    private static readonly Gen<int> GenNonPositiveId =
        Gen.Int[int.MinValue, 0];

    [Fact]
    public void Validate_ValidChannel_ReturnsSameInstance()
    {
        Gen.Select(GenPositiveId, GenNonEmptyString, GenValidCategory, GenNonEmptyString)
           .Sample((id, name, category, country) =>
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
           });
    }

    [Fact]
    public void Validate_ZeroOrNegativeId_Throws()
    {
        Gen.Select(GenNonPositiveId, GenNonEmptyString, GenValidCategory, GenNonEmptyString)
           .Sample((id, name, category, country) =>
           {
               var channel = new Channel
               {
                   Id = id,
                   Name = name,
                   Category = category,
                   Country = country,
               };

               Assert.Throws<ArgumentException>(() => ChannelValidator.Validate(channel));
           });
    }

    [Fact]
    public void Validate_BlankName_Throws()
    {
        var blankNames = new[] { "", " ", "\t", "\n", "   " };

        Gen.Select(GenPositiveId, Gen.OneOfConst(blankNames), GenValidCategory, GenNonEmptyString)
           .Sample((id, name, category, country) =>
           {
               var channel = new Channel
               {
                   Id = id,
                   Name = name,
                   Category = category,
                   Country = country,
               };

               Assert.Throws<ArgumentException>(() => ChannelValidator.Validate(channel));
           });
    }

    [Fact]
    public void Validate_CategoryExceedsMaxLength_Throws()
    {
        Gen.Select(GenPositiveId, GenNonEmptyString, GenLongCategory, GenNonEmptyString)
           .Sample((id, name, category, country) =>
           {
               var channel = new Channel
               {
                   Id = id,
                   Name = name,
                   Category = category,
                   Country = country,
               };

               Assert.Throws<ArgumentException>(() => ChannelValidator.Validate(channel));
           });
    }

    [Fact]
    public void Validate_BlankCountry_Throws()
    {
        var blankCountries = new[] { "", " ", "\t", "\n", "   " };

        Gen.Select(GenPositiveId, GenNonEmptyString, GenValidCategory, Gen.OneOfConst(blankCountries))
           .Sample((id, name, category, country) =>
           {
               var channel = new Channel
               {
                   Id = id,
                   Name = name,
                   Category = category,
                   Country = country,
               };

               Assert.Throws<ArgumentException>(() => ChannelValidator.Validate(channel));
           });
    }

    [Fact]
    public void Validate_NullChannel_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ChannelValidator.Validate(null!));
    }
}
