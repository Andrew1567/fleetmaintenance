using System;
using System.Collections.Generic;
using System.IO;
using FleetMaintenance.Application;
using FleetMaintenance.Domain;

namespace FleetMaintenance.App;

/// <summary>Служби застосунку для меню.</summary>
/// <param name="Vehicles">UC-05, UC-06.</param>
/// <param name="Mileage">UC-01.</param>
/// <param name="Orders">UC-02, UC-03.</param>
/// <param name="Reports">UC-04.</param>
internal sealed record AppServices(
    VehicleService Vehicles,
    MileageService Mileage,
    ServiceOrderService Orders,
    ReportService Reports);

// TODO TD-01: меню й ReportFormatter не покриті автотестами (4 год).
/// <summary>Текстове меню: читає введення, викликає служби, друкує результат.</summary>
internal sealed class Menu
{
    private readonly TextReader _in;
    private readonly TextWriter _out;
    private readonly AppServices _app;
    private readonly Dictionary<string, Action> _commands;

    internal Menu(TextReader input, TextWriter output, AppServices app)
    {
        _in = input;
        _out = output;
        _app = app;
        _commands = new Dictionary<string, Action> {
            ["1"] = RegisterMileage,
            ["2"] = PlanService,
            ["3"] = ChangeOrder,
            ["4"] = ShowReport,
            ["5"] = Search,
            ["6"] = AddVehicle,
        };
    }

    internal void Run()
    {
        bool running = true;
        while (running)
        {
            ShowMenu();
            string? choice = Ask("Ваш вибір")?.Trim();
            running = choice is not null && choice != "0";
            if (running)
            {
                Dispatch(choice!);
            }
        }
    }

    private void Dispatch(string choice)
    {
        if (!_commands.TryGetValue(choice, out Action? command))
        {
            _out.WriteLine("Невідома команда.");
            return;
        }

        try
        {
            command();
        }
        catch (DomainRuleException ex)
        {
            _out.WriteLine($"Операцію відхилено: {ex.Message}");
        }
    }

    private void ShowMenu()
    {
        _out.WriteLine();
        _out.WriteLine("1) Зареєструвати пробіг");
        _out.WriteLine("2) Запланувати обслуговування");
        _out.WriteLine("3) Змінити стан замовлення");
        _out.WriteLine("4) Звіт: машини, що потребують ТО");
        _out.WriteLine("5) Знайти ТЗ");
        _out.WriteLine("6) Додати ТЗ");
        _out.WriteLine("0) Вихід");
    }

    private string? Ask(string label)
    {
        _out.Write($"{label}: ");
        return _in.ReadLine();
    }

    private void RegisterMileage()
    {
        Result<Vehicle> r = _app.Mileage.Register(Ask("Номер ТЗ"), Ask("Новий пробіг, км"));
        Print(r, v => $"Пробіг ТЗ {v.Info.Plate}: {v.Mileage} км.");
    }

    private void PlanService()
    {
        Result<ServiceOrder> r = _app.Orders.Plan(
            Ask("Номер ТЗ"),
            Ask("Вид ТО (OilChange, Brakes, Inspection)"));
        Print(r, o => $"Замовлення №{o.Id}, вартість {Labels.Money(o.Cost)} грн.");
    }

    private void ChangeOrder()
    {
        Result<ServiceOrder> r = _app.Orders.Apply(
            Ask("Номер замовлення"),
            Ask("Дія (Start, Complete, Cancel)"));
        Print(r, o => $"Замовлення №{o.Id}: {Labels.Of(o.Status)}.");
    }

    private void ShowReport()
    {
        IReadOnlyList<DueVehicleLine> lines = _app.Reports.DueVehicles();
        foreach (string row in ReportFormatter.Format(lines, _app.Reports.AsOf))
        {
            _out.WriteLine(row);
        }
    }

    private void Search()
    {
        Result<IReadOnlyList<Vehicle>> r = _app.Vehicles.Search(
            Ask("Текст (номер або модель, Enter — усі)"),
            Ask("Тип (Car, Van, Truck, Enter — усі)"));
        Print(r, found => $"Знайдено: {found.Count}.");
        if (r.IsSuccess)
        {
            foreach (Vehicle v in r.Value)
            {
                _out.WriteLine($"{v.Info.Plate}  {v.Info.Model}  {v.Info.Type}  {v.Mileage} км");
            }
        }
    }

    private void AddVehicle()
    {
        Result<Vehicle> r = _app.Vehicles.Register(
            Ask("Номер (AA1234BB)"),
            Ask("Модель"),
            Ask("Тип (Car, Van, Truck)"),
            Ask("Пробіг, км"));
        Print(r, v => $"Додано ТЗ №{v.Id}: {v.Info.Plate}.");
    }

    private void Print<T>(Result<T> result, Func<T, string> onSuccess)
    {
        _out.WriteLine(result.IsSuccess ? onSuccess(result.Value) : $"Помилка: {result.Error}");
    }
}
