using System;

namespace FleetMaintenance.Domain;

/// <summary>Базовий виняток порушення правила домену.</summary>
public abstract class DomainRuleException : Exception
{
    /// <summary>Створює виняток із повідомленням для користувача.</summary>
    /// <param name="message">Пояснення, яке правило порушено.</param>
    protected DomainRuleException(string message)
        : base(message)
    {
    }
}

/// <summary>Спроба виконати подію, заборонену в поточному стані.</summary>
public sealed class InvalidOrderStateException : DomainRuleException
{
    /// <summary>Створює виняток для пари «стан — подія».</summary>
    /// <param name="origin">Поточний стан замовлення.</param>
    /// <param name="trigger">Заборонена в цьому стані подія.</param>
    public InvalidOrderStateException(OrderStatus origin, OrderEvent trigger)
        : base($"Подію {trigger} не можна застосувати в стані {origin}.")
    {
        From = origin;
        Trigger = trigger;
    }

    /// <summary>Стан, у якому було відхилено подію.</summary>
    public OrderStatus From { get; }

    /// <summary>Відхилена подія.</summary>
    public OrderEvent Trigger { get; }
}

/// <summary>Спроба зареєструвати пробіг, менший за поточний.</summary>
public sealed class MileageRollbackException : DomainRuleException
{
    /// <summary>Створює виняток для пари «поточний — новий» пробіг.</summary>
    /// <param name="current">Пробіг, збережений у системі, км.</param>
    /// <param name="attempted">Менше значення, яке намагалися внести, км.</param>
    public MileageRollbackException(int current, int attempted)
        : base($"Пробіг {attempted} км менший за поточний {current} км.")
    {
    }
}
