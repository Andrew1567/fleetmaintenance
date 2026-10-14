using System;
using System.Linq;
using FleetMaintenance.Application;
using FleetMaintenance.Domain;
using Xunit;

namespace FleetMaintenance.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void Домен_НеЗалежитьВідЖодногоПроєктуРішення()
    {
        var links = typeof(Vehicle).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n.StartsWith("FleetMaintenance", StringComparison.Ordinal));

        Assert.Empty(links);
    }

    [Fact]
    public void Прикладний_НеЗалежитьВідІнфраструктуриТаЗастосунку()
    {
        var links = typeof(Result).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n.Contains("Infrastructure", StringComparison.Ordinal)
                || n.EndsWith(".App", StringComparison.Ordinal));

        Assert.Empty(links);
    }

    [Fact]
    public void Домен_НеМаєПублічнихТипівІзСховищемАбоФайлами()
    {
        var names = typeof(Vehicle).Assembly.GetExportedTypes().Select(t => t.Name);

        Assert.DoesNotContain(names, n => n.Contains("Repository", StringComparison.Ordinal));
    }
}
