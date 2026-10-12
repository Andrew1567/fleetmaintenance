using System;
using System.Collections.Generic;
using FleetMaintenance.Domain;

namespace FleetMaintenance.Application;

/// <summary>Інтервал між обслуговуваннями.</summary>
/// <param name="Km">Пробіг між ТО, км.</param>
/// <param name="Months">Час між ТО, повних місяців.</param>
public sealed record ServiceInterval(int Km, int Months);

/// <summary>Результат оцінки потреби в одному виді ТО.</summary>
/// <param name="Kind">Вид обслуговування.</param>
/// <param name="Status">Підсумковий стан.</param>
/// <param name="KmSince">Пробіг після останнього ТО, км.</param>
/// <param name="MonthsSince">Повних місяців після останнього ТО.</param>
public sealed record MaintenanceCheck(
    ServiceKind Kind,
    MaintenanceStatus Status,
    int KmSince,
    int MonthsSince);

/// <summary>Оцінка потреби в ТО разом із вартістю.</summary>
/// <param name="Check">Стан за пробігом і датою.</param>
/// <param name="Cost">Вартість у гривнях.</param>
public sealed record ServiceAssessment(MaintenanceCheck Check, decimal Cost);

// TODO TD-04: інтервали й ціни зашито в код; винести в конфігурацію (3 год).
/// <summary>Таблиця міжсервісних інтервалів (розділ 1.4 специфікації).</summary>
public static class IntervalTable
{
    private static readonly Dictionary<(VehicleType, ServiceKind), ServiceInterval> Rows = new()
    {
        [(VehicleType.Car, ServiceKind.OilChange)] = new(10000, 12),
        [(VehicleType.Car, ServiceKind.Brakes)] = new(30000, 24),
        [(VehicleType.Car, ServiceKind.Inspection)] = new(20000, 12),
        [(VehicleType.Van, ServiceKind.OilChange)] = new(12000, 12),
        [(VehicleType.Van, ServiceKind.Brakes)] = new(35000, 24),
        [(VehicleType.Van, ServiceKind.Inspection)] = new(25000, 12),
        [(VehicleType.Truck, ServiceKind.OilChange)] = new(20000, 6),
        [(VehicleType.Truck, ServiceKind.Brakes)] = new(40000, 12),
        [(VehicleType.Truck, ServiceKind.Inspection)] = new(30000, 12),
    };

    /// <summary>Повертає інтервал для типу ТЗ і виду ТО.</summary>
    /// <param name="type">Тип ТЗ.</param>
    /// <param name="kind">Вид обслуговування.</param>
    /// <returns>Інтервал за пробігом і за часом.</returns>
    public static ServiceInterval For(VehicleType type, ServiceKind kind)
    {
        return Rows[(type, kind)];
    }
}

// TODO TD-05: новий вид ТО потребує змін у переліченні й двох таблицях (2,5 год).
/// <summary>Правило оцінки потреби в обслуговуванні.</summary>
public interface IMaintenancePolicy
{
    /// <summary>Оцінює потребу в обслуговуванні певного виду.</summary>
    /// <param name="vehicle">ТЗ.</param>
    /// <param name="kind">Вид обслуговування.</param>
    /// <param name="today">Поточна дата.</param>
    /// <returns>Стан за пробігом і датою; гірший із двох.</returns>
    MaintenanceCheck Evaluate(Vehicle vehicle, ServiceKind kind, DateOnly today);
}

/// <summary>Правило розрахунку вартості обслуговування.</summary>
public interface IPricingPolicy
{
    /// <summary>Розраховує вартість обслуговування.</summary>
    /// <param name="type">Тип ТЗ.</param>
    /// <param name="kind">Вид обслуговування.</param>
    /// <param name="status">Стан потреби в ТО.</param>
    /// <returns>Вартість у гривнях, округлена до копійок.</returns>
    decimal Price(VehicleType type, ServiceKind kind, MaintenanceStatus status);
}

/// <summary>Міжсервісний інтервал за пробігом і датою: гірший із двох.</summary>
public sealed class MaintenancePolicy : IMaintenancePolicy
{
    /// <summary>Частка інтервалу в відсотках, з якої ТО «скоро потрібне».</summary>
    public const int DueSoonPercent = 80;

    private const int PercentScale = 100;
    private const int MonthsPerYear = 12;

    /// <inheritdoc />
    public MaintenanceCheck Evaluate(Vehicle vehicle, ServiceKind kind, DateOnly today)
    {
        ServiceInterval interval = IntervalTable.For(vehicle.Info.Type, kind);
        ServicePoint last = vehicle.LastServiceOrRegistration(kind);
        int kmSince = vehicle.Mileage - last.Mileage;
        int monthsSince = MonthsBetween(last.Date, today);
        MaintenanceStatus byKm = Classify(kmSince, interval.Km);
        MaintenanceStatus byDate = Classify(monthsSince, interval.Months);
        MaintenanceStatus worst = byKm > byDate ? byKm : byDate;
        return new MaintenanceCheck(kind, worst, kmSince, monthsSince);
    }

    /// <summary>Класифікує витрачену частину інтервалу.</summary>
    /// <param name="elapsed">Витрачено: км або місяців.</param>
    /// <param name="limit">Інтервал у тих самих одиницях.</param>
    /// <returns>Overdue понад інтервал, DueSoon від 80 %, інакше Ok.</returns>
    public static MaintenanceStatus Classify(int elapsed, int limit)
    {
        if (elapsed > limit)
        {
            return MaintenanceStatus.Overdue;
        }

        bool soon = (long)elapsed * PercentScale >= (long)limit * DueSoonPercent;
        return soon ? MaintenanceStatus.DueSoon : MaintenanceStatus.Ok;
    }

    /// <summary>Рахує повні місяці між двома датами.</summary>
    /// <param name="origin">Початкова дата.</param>
    /// <param name="to">Кінцева дата.</param>
    /// <returns>Кількість повних місяців; неповний місяць не враховується.</returns>
    public static int MonthsBetween(DateOnly origin, DateOnly to)
    {
        int months = ((to.Year - origin.Year) * MonthsPerYear) + to.Month - origin.Month;
        return to.Day < origin.Day ? months - 1 : months;
    }
}

/// <summary>Вартість = базова ціна × коефіцієнт типу × надбавка за прострочення.</summary>
public sealed class PricingPolicy : IPricingPolicy
{
    private const decimal OverdueFactor = 1.15m;
    private const int PriceDecimals = 2;

    private static readonly Dictionary<ServiceKind, decimal> BasePrice = new()
    {
        [ServiceKind.OilChange] = 1200m,
        [ServiceKind.Brakes] = 3500m,
        [ServiceKind.Inspection] = 1800m,
    };

    private static readonly Dictionary<VehicleType, decimal> TypeFactor = new()
    {
        [VehicleType.Car] = 1.00m,
        [VehicleType.Van] = 1.25m,
        [VehicleType.Truck] = 1.60m,
    };

    /// <inheritdoc />
    public decimal Price(VehicleType type, ServiceKind kind, MaintenanceStatus status)
    {
        decimal price = BasePrice[kind] * TypeFactor[type];
        if (status == MaintenanceStatus.Overdue)
        {
            price *= OverdueFactor;
        }

        return Math.Round(price, PriceDecimals, MidpointRounding.AwayFromZero);
    }
}

/// <summary>Об'єднує оцінку потреби в ТО й розрахунок вартості.</summary>
public sealed class ServiceAdvisor
{
    private readonly IMaintenancePolicy _maintenance;
    private readonly IPricingPolicy _pricing;

    /// <summary>Створює радника із заданими правилами.</summary>
    /// <param name="maintenance">Правило оцінки потреби в ТО.</param>
    /// <param name="pricing">Правило розрахунку вартості.</param>
    public ServiceAdvisor(IMaintenancePolicy maintenance, IPricingPolicy pricing)
    {
        _maintenance = maintenance;
        _pricing = pricing;
    }

    /// <summary>Оцінює потребу в ТО й вартість.</summary>
    /// <param name="vehicle">ТЗ.</param>
    /// <param name="kind">Вид обслуговування.</param>
    /// <param name="today">Поточна дата.</param>
    /// <returns>Стан і вартість.</returns>
    public ServiceAssessment Assess(Vehicle vehicle, ServiceKind kind, DateOnly today)
    {
        MaintenanceCheck check = _maintenance.Evaluate(vehicle, kind, today);
        decimal cost = _pricing.Price(vehicle.Info.Type, kind, check.Status);
        return new ServiceAssessment(check, cost);
    }
}
