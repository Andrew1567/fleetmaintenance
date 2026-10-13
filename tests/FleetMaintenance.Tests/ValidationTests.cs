using System;
using FleetMaintenance.Application;
using FleetMaintenance.Domain;
using Xunit;

namespace FleetMaintenance.Tests;

public sealed class ValidationTests
{
    [Theory]
    [InlineData("AA1234BB", "AA1234BB")]
    [InlineData("aa1234bb", "AA1234BB")]
    [InlineData("  KA0001XY ", "KA0001XY")]
    public void Plate_КоректнийНомер_НормалізуєтьсяДоВеликихЛітер(string input, string expected)
    {
        Result<string> result = InputValidator.Plate(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("AA123BB")]
    [InlineData("AA1234B")]
    [InlineData("1234AABB")]
    [InlineData("АА1234ВВ")]
    public void Plate_НекоректнийНомер_ПовертаєПомилку(string? input)
    {
        Result<string> result = InputValidator.Plate(input);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("152300", 152300)]
    [InlineData("5000000", 5000000)]
    public void Mileage_КоректнеЧисло_Розбирається(string input, int expected)
    {
        Result<int> result = InputValidator.Mileage(input);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("-5")]
    [InlineData("12.5")]
    [InlineData("5000001")]
    public void Mileage_НекоректнеЗначення_ПовертаєПомилку(string? input)
    {
        Result<int> result = InputValidator.Mileage(input);

        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("truck", VehicleType.Truck)]
    [InlineData(" Van ", VehicleType.Van)]
    public void Type_РегістрНеВажливий_ПовертаєТип(string input, VehicleType expected)
    {
        Result<VehicleType> result = InputValidator.Type(input);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Bike")]
    [InlineData("7")]
    public void Type_НевідомеЗначення_ПовертаєПомилкуЗПереліком(string? input)
    {
        Result<VehicleType> result = InputValidator.Type(input);

        Assert.False(result.IsSuccess);
        Assert.Contains("Truck", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Model_ЗанадтоДовга_ПовертаєПомилку()
    {
        Result<string> result = InputValidator.Model(new string('x', 41));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Model_ПорожняАбоПробіли_ПовертаєПомилку()
    {
        Assert.False(InputValidator.Model("   ").IsSuccess);
        Assert.False(InputValidator.Model(null).IsSuccess);
        Assert.Equal("Camry", InputValidator.Model(" Camry ").Value);
    }

    [Theory]
    [InlineData("Start", OrderEvent.Start)]
    [InlineData("cancel", OrderEvent.Cancel)]
    public void Trigger_КоректнаДія_Розбирається(string input, OrderEvent expected)
    {
        Assert.Equal(expected, InputValidator.Trigger(input).Value);
    }

    [Fact]
    public void FirstError_КількаПомилок_ПовертаєПершу()
    {
        Result<string> bad = InputValidator.Plate("x");
        Result<int> worse = InputValidator.Mileage("y");

        string? error = Result.FirstError(InputValidator.Mileage("1"), bad, worse);

        Assert.Equal(bad.Error, error);
    }
}
