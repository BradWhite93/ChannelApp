using System.Reflection;
using ChannelApp.Domain.Entities;
using ChannelApp.Application.DTOs;

namespace ChannelApp.Tests.Architecture;

public class LayerDependencyTests
{
    [Fact]
    public void DomainLayer_HasNoForbiddenReferences()
    {
        var domainAssembly = typeof(Channel).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .ToList();

        Assert.DoesNotContain("ChannelApp.Application", referencedAssemblies);
        Assert.DoesNotContain("ChannelApp.Infrastructure", referencedAssemblies);
        Assert.DoesNotContain("ChannelApp.Presentation", referencedAssemblies);
    }

    [Fact]
    public void ApplicationLayer_HasNoForbiddenReferences()
    {
        var appAssembly = typeof(ChannelDto).Assembly;
        var referencedAssemblies = appAssembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .ToList();

        Assert.DoesNotContain("ChannelApp.Infrastructure", referencedAssemblies);
        Assert.DoesNotContain("ChannelApp.Presentation", referencedAssemblies);
    }
}
