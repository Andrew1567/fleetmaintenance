using System;
using System.Linq;

namespace FleetMaintenance.Application;

/// <summary>Результат операції без значення: успіх або текст помилки.</summary>
public class Result
{
    /// <summary>Створює результат.</summary>
    /// <param name="error">Текст помилки або null для успіху.</param>
    protected Result(string? error)
    {
        Error = error;
    }

    /// <summary>Текст помилки для користувача; null, якщо операція вдалася.</summary>
    public string? Error { get; }

    /// <summary>Чи вдалася операція.</summary>
    public bool IsSuccess => Error is null;

    /// <summary>Повертає першу помилку з набору результатів.</summary>
    /// <param name="results">Результати перевірок у порядку виконання.</param>
    /// <returns>Текст першої помилки або null, якщо всі перевірки пройдено.</returns>
    public static string? FirstError(params Result[] results)
    {
        return results.Select(r => r.Error).FirstOrDefault(e => e is not null);
    }
}

/// <summary>Результат операції зі значенням.</summary>
/// <typeparam name="T">Тип значення успішного результату.</typeparam>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, string? error)
        : base(error)
    {
        _value = value;
    }

    /// <summary>Значення успішного результату.</summary>
    /// <exception cref="InvalidOperationException">Результат помилковий.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Результат помилковий: " + Error);

    /// <summary>Створює успішний результат.</summary>
    /// <param name="value">Значення.</param>
    /// <returns>Успішний результат.</returns>
    public static Result<T> Ok(T value)
    {
        return new Result<T>(value, null);
    }

    /// <summary>Створює помилковий результат.</summary>
    /// <param name="error">Текст помилки для користувача.</param>
    /// <returns>Помилковий результат.</returns>
    public static Result<T> Fail(string error)
    {
        return new Result<T>(default, error);
    }
}
