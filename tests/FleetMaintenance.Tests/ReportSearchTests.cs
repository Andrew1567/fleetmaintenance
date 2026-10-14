using System;
using System.Collections.Generic;
using System.Linq;
using FleetMaintenance.Application;
using FleetMaintenance.Domain;
using Xunit;

namespace FleetMaintenance.Tests;

public sealed class ReportSearchTests
{
    private static readonly DateOnly Recent = new(2026, 10, 1);
    private readonly Fixture _f = new();

    [Fact]
    public void Report_НемаПотрібногоТО_ПорожнійПерелік()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 100);

        IReadOnlyList<DueVehicleLine> lines = _f.ReportService().DueVehicles();

        Assert.Empty(lines);
    }

    [Fact]
    public void Report_ОднаПрострочена_ОдинРядокЗВартістю()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 10500);

        IReadOnlyList<DueVehicleLine> lines = _f.ReportService().DueVehicles();

        DueVehicleLine line = Assert.Single(lines);
        Assert.Equal(ServiceKind.OilChange, line.Check.Kind);
        Assert.Equal(MaintenanceStatus.Overdue, line.Check.Status);
        Assert.Equal(1380m, line.Cost);
    }

    [Fact]
    public void Report_Кілька_СпершуПрострочені_ПотімЗаНомером()
    {
        _f.AddVehicle("CC3333DD", VehicleType.Car, Recent, 8500);
        _f.AddVehicle("BB2222CC", VehicleType.Car, Recent, 10500);
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 9000);

        IReadOnlyList<DueVehicleLine> lines = _f.ReportService().DueVehicles();

        string[] plates = lines.Select(l => l.Plate).ToArray();
        Assert.Equal(new[] { "BB2222CC", "AA1111BB", "CC3333DD" }, plates);
    }

    [Fact]
    public void TotalCost_КількаРядків_Підсумовує()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 10500);
        _f.AddVehicle("BB2222CC", VehicleType.Van, Recent, 13000);

        IReadOnlyList<DueVehicleLine> lines = _f.ReportService().DueVehicles();

        Assert.Equal(1380m + 1725m, ReportService.TotalCost(lines));
    }

    [Fact]
    public void Search_НічогоНеЗнайдено_ПорожнійСписок()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 100);

        Result<IReadOnlyList<Vehicle>> result = _f.VehicleService().Search("zzz", null);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void Search_ЧастинаНомера_ПовертаєОдинЗапис()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 100);
        _f.AddVehicle("KB2222CC", VehicleType.Car, Recent, 100);

        Result<IReadOnlyList<Vehicle>> result = _f.VehicleService().Search("kb22", "");

        Assert.Single(result.Value);
    }

    [Fact]
    public void Search_ЗаТипом_ПовертаєВідсортованийСписок()
    {
        _f.AddVehicle("BB2222CC", VehicleType.Van, Recent, 100);
        _f.AddVehicle("CC3333DD", VehicleType.Car, Recent, 100);
        _f.AddVehicle("AA1111BB", VehicleType.Van, Recent, 100);

        Result<IReadOnlyList<Vehicle>> result = _f.VehicleService().Search(null, "van");

        string[] plates = result.Value.Select(v => v.Info.Plate).ToArray();
        Assert.Equal(new[] { "AA1111BB", "BB2222CC" }, plates);
    }

    [Fact]
    public void Search_НевідомийТип_ПовертаєПомилку()
    {
        Result<IReadOnlyList<Vehicle>> result = _f.VehicleService().Search(null, "Bike");

        Assert.False(result.IsSuccess);
    }
}
