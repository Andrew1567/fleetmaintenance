using System.Collections.Generic;
using FleetMaintenance.Domain;

namespace FleetMaintenance.Application;

/// <summary>Сховище транспортних засобів.</summary>
public interface IVehicleRepository
{
    /// <summary>Повертає всі ТЗ.</summary>
    /// <returns>Список усіх ТЗ.</returns>
    IReadOnlyList<Vehicle> GetAll();

    /// <summary>Шукає ТЗ за номером у системі.</summary>
    /// <param name="id">Номер у системі.</param>
    /// <returns>ТЗ або null.</returns>
    Vehicle? GetById(int id);

    /// <summary>Шукає ТЗ за державним номером.</summary>
    /// <param name="plate">Державний номер у верхньому регістрі.</param>
    /// <returns>ТЗ або null.</returns>
    Vehicle? FindByPlate(string plate);

    /// <summary>Додає нове ТЗ.</summary>
    /// <param name="vehicle">ТЗ.</param>
    void Add(Vehicle vehicle);

    /// <summary>Повертає наступний вільний номер у системі.</summary>
    /// <returns>Номер, більший за всі наявні.</returns>
    int NextId();

    /// <summary>Зберігає зміни.</summary>
    void Save();
}

/// <summary>Сховище замовлень на обслуговування.</summary>
public interface IServiceOrderRepository
{
    /// <summary>Повертає всі замовлення.</summary>
    /// <returns>Список замовлень.</returns>
    IReadOnlyList<ServiceOrder> GetAll();

    /// <summary>Шукає замовлення за номером.</summary>
    /// <param name="id">Номер замовлення.</param>
    /// <returns>Замовлення або null.</returns>
    ServiceOrder? GetById(int id);

    /// <summary>Додає нове замовлення.</summary>
    /// <param name="order">Замовлення.</param>
    void Add(ServiceOrder order);

    /// <summary>Повертає наступний вільний номер замовлення.</summary>
    /// <returns>Номер, більший за всі наявні.</returns>
    int NextId();

    /// <summary>Зберігає зміни.</summary>
    void Save();
}

/// <summary>Журнал подій застосунку.</summary>
public interface IAppLogger
{
    /// <summary>Записує початок або успішне завершення сценарію.</summary>
    /// <param name="scenario">Ідентифікатор сценарію, наприклад UC-01.</param>
    /// <param name="message">Текст запису без персональних даних.</param>
    void Information(string scenario, string message);

    /// <summary>Записує відхилену операцію.</summary>
    /// <param name="scenario">Ідентифікатор сценарію.</param>
    /// <param name="message">Причина відхилення.</param>
    void Warning(string scenario, string message);

    /// <summary>Записує виняток із контекстом.</summary>
    /// <param name="scenario">Ідентифікатор сценарію.</param>
    /// <param name="message">Що виконувалося.</param>
    /// <param name="error">Виняток.</param>
    void Error(string scenario, string message, System.Exception error);
}
