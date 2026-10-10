namespace FleetMaintenance.Domain;

/// <summary>Тип транспортного засобу; визначає інтервали й ціни ТО.</summary>
public enum VehicleType
{
    /// <summary>Легковий автомобіль.</summary>
    Car,

    /// <summary>Мікроавтобус або фургон.</summary>
    Van,

    /// <summary>Вантажівка.</summary>
    Truck,
}

/// <summary>Вид планового обслуговування.</summary>
public enum ServiceKind
{
    /// <summary>Заміна мастила й фільтрів.</summary>
    OilChange,

    /// <summary>Обслуговування гальмівної системи.</summary>
    Brakes,

    /// <summary>Технічний огляд.</summary>
    Inspection,
}

/// <summary>Стан потреби в обслуговуванні; порядок значень важливий.</summary>
public enum MaintenanceStatus
{
    /// <summary>Обслуговування ще не потрібне.</summary>
    Ok,

    /// <summary>Інтервал вичерпано щонайменше на 80 %.</summary>
    DueSoon,

    /// <summary>Інтервал перевищено.</summary>
    Overdue,
}

/// <summary>Стан замовлення на обслуговування.</summary>
public enum OrderStatus
{
    /// <summary>Заплановано.</summary>
    Planned,

    /// <summary>Виконується.</summary>
    InProgress,

    /// <summary>Виконано.</summary>
    Done,

    /// <summary>Скасовано.</summary>
    Cancelled,
}

/// <summary>Подія, що змінює стан замовлення.</summary>
public enum OrderEvent
{
    /// <summary>Почати виконання.</summary>
    Start,

    /// <summary>Завершити виконання.</summary>
    Complete,

    /// <summary>Скасувати замовлення.</summary>
    Cancel,
}
