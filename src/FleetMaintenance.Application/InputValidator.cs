using System;
using System.Globalization;
using System.Text.RegularExpressions;
using FleetMaintenance.Domain;

namespace FleetMaintenance.Application;

/// <summary>Барикада валідації: єдине місце перевірки тексту з консолі.</summary>
public static class InputValidator
{
    private const int MaxMileage = 5_000_000;
    private const int MaxModelLength = 40;

    private static readonly Regex PlateFormat = new(
        "^[A-Z]{2}[0-9]{4}[A-Z]{2}$",
        RegexOptions.CultureInvariant);

    /// <summary>Перевіряє й нормалізує державний номер.</summary>
    /// <param name="text">Введений текст.</param>
    /// <returns>Номер у верхньому регістрі або помилка.</returns>
    public static Result<string> Plate(string? text)
    {
        string plate = (text ?? string.Empty).Trim().ToUpperInvariant();
        return PlateFormat.IsMatch(plate)
            ? Result<string>.Ok(plate)
            : Result<string>.Fail("Номер має формат AA1234BB (латинські літери).");
    }

    /// <summary>Перевіряє марку й модель.</summary>
    /// <param name="text">Введений текст.</param>
    /// <returns>Назва без крайніх пробілів або помилка.</returns>
    public static Result<string> Model(string? text)
    {
        string model = (text ?? string.Empty).Trim();
        if (model.Length == 0 || model.Length > MaxModelLength)
        {
            return Result<string>.Fail($"Модель: від 1 до {MaxModelLength} символів.");
        }

        return Result<string>.Ok(model);
    }

    /// <summary>Перевіряє пробіг.</summary>
    /// <param name="text">Введений текст.</param>
    /// <returns>Пробіг у км або помилка.</returns>
    public static Result<int> Mileage(string? text)
    {
        bool parsed = int.TryParse(
            text,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out int km);
        return parsed && km <= MaxMileage
            ? Result<int>.Ok(km)
            : Result<int>.Fail($"Пробіг: ціле число від 0 до {MaxMileage}.");
    }

    /// <summary>Перевіряє номер замовлення.</summary>
    /// <param name="text">Введений текст.</param>
    /// <returns>Номер замовлення або помилка.</returns>
    public static Result<int> OrderId(string? text)
    {
        bool parsed = int.TryParse(
            text,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out int id);
        return parsed
            ? Result<int>.Ok(id)
            : Result<int>.Fail("Номер замовлення: невід'ємне ціле число.");
    }

    /// <summary>Перевіряє тип ТЗ.</summary>
    /// <param name="text">Введений текст: Car, Van або Truck.</param>
    /// <returns>Тип ТЗ або помилка.</returns>
    public static Result<VehicleType> Type(string? text)
    {
        return ParseEnum<VehicleType>(text, "Тип ТЗ");
    }

    /// <summary>Перевіряє вид обслуговування.</summary>
    /// <param name="text">Введений текст: OilChange, Brakes або Inspection.</param>
    /// <returns>Вид ТО або помилка.</returns>
    public static Result<ServiceKind> Kind(string? text)
    {
        return ParseEnum<ServiceKind>(text, "Вид ТО");
    }

    /// <summary>Перевіряє подію зміни стану.</summary>
    /// <param name="text">Введений текст: Start, Complete або Cancel.</param>
    /// <returns>Подія або помилка.</returns>
    public static Result<OrderEvent> Trigger(string? text)
    {
        return ParseEnum<OrderEvent>(text, "Дія");
    }

    private static Result<T> ParseEnum<T>(string? text, string label)
        where T : struct, Enum
    {
        bool parsed = Enum.TryParse(text?.Trim(), true, out T value);
        string allowed = string.Join(", ", Enum.GetNames<T>());
        return parsed && Enum.IsDefined(value)
            ? Result<T>.Ok(value)
            : Result<T>.Fail($"{label}: допустимі значення {allowed}.");
    }
}
