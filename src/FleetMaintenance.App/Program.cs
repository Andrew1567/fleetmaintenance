using System;
using System.IO;
using FleetMaintenance.Application;
using FleetMaintenance.Infrastructure;

namespace FleetMaintenance.App;

/// <summary>Точка входу й композиційний корінь застосунку.</summary>
public static class Program
{
    private const int DataErrorExitCode = 2;

    /// <summary>Запускає меню. Перший аргумент — тека з даними (типово data).</summary>
    /// <param name="args">Аргументи командного рядка.</param>
    /// <returns>0 — успіх, 2 — пошкоджені або недоступні дані.</returns>
    public static int Main(string[] args)
    {
        string dataDir = args.Length > 0 ? args[0] : "data";
        TimeProvider time = CreateClock();
        var logger = new FileLogger(Path.Combine("logs", "fleet.log"), time);
        try
        {
            var env = new AppEnvironment(time, logger);
            new Menu(Console.In, Console.Out, Compose(dataDir, env)).Run();
            return 0;
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException)
        {
            logger.Error("APP", "помилка даних", ex);
            Console.Error.WriteLine($"Помилка даних: {ex.Message}");
            return DataErrorExitCode;
        }
    }

    private static AppServices Compose(string dataDir, AppEnvironment env)
    {
        var vehicles = new JsonVehicleRepository(Path.Combine(dataDir, "vehicles.json"));
        var orders = new JsonServiceOrderRepository(Path.Combine(dataDir, "orders.json"));
        var advisor = new ServiceAdvisor(new MaintenancePolicy(), new PricingPolicy());
        return new AppServices(
            new VehicleService(vehicles, env),
            new MileageService(vehicles, env),
            new ServiceOrderService(vehicles, orders, advisor, env),
            new ReportService(vehicles, advisor, env));
    }

    private static TimeProvider CreateClock()
    {
        string? text = Environment.GetEnvironmentVariable("FLEET_TODAY");
        return DateOnly.TryParseExact(text, "yyyy-MM-dd", out DateOnly day)
            ? new FixedTimeProvider(day)
            : TimeProvider.System;
    }
}
