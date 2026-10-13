using System;
using System.Linq;
using FleetMaintenance.Domain;

namespace FleetMaintenance.Infrastructure;

/// <summary>Перетворення між доменними об'єктами та файловими DTO.</summary>
public static class Mappers
{
    /// <summary>Будує доменне ТЗ із DTO.</summary>
    /// <param name="dto">Збережені дані.</param>
    /// <returns>Відновлене ТЗ із пробігом та історією.</returns>
    public static Vehicle ToVehicle(VehicleDto dto)
    {
        var info = new VehicleInfo(dto.Plate, dto.Model, Enum.Parse<VehicleType>(dto.Type));
        var registered = new ServicePoint(dto.RegisteredOn, dto.RegisteredMileage);
        var vehicle = new Vehicle(dto.Id, info, registered);
        vehicle.RegisterMileage(dto.Mileage);
        foreach (ServiceRecordDto r in dto.History)
        {
            var point = new ServicePoint(r.Date, r.Mileage);
            vehicle.AddServiceRecord(new ServiceRecord(Enum.Parse<ServiceKind>(r.Kind), point));
        }

        return vehicle;
    }

    /// <summary>Будує DTO із доменного ТЗ.</summary>
    /// <param name="vehicle">ТЗ.</param>
    /// <returns>Дані для запису у файл.</returns>
    public static VehicleDto ToVehicleDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            Plate = vehicle.Info.Plate,
            Model = vehicle.Info.Model,
            Type = vehicle.Info.Type.ToString(),
            RegisteredOn = vehicle.Registered.Date,
            RegisteredMileage = vehicle.Registered.Mileage,
            Mileage = vehicle.Mileage,
            History = vehicle.History
                .Select(r => new ServiceRecordDto
                {
                    Kind = r.Kind.ToString(),
                    Date = r.Point.Date,
                    Mileage = r.Point.Mileage,
                })
                .ToList(),
        };
    }

    /// <summary>Будує доменне замовлення із DTO.</summary>
    /// <param name="dto">Збережені дані.</param>
    /// <returns>Відновлене замовлення.</returns>
    public static ServiceOrder ToOrder(OrderDto dto)
    {
        var order = new ServiceOrder(
            dto.Id,
            dto.VehicleId,
            Enum.Parse<ServiceKind>(dto.Kind),
            dto.PlannedOn)
        {
            Cost = dto.Cost,
        };
        order.Restore(Enum.Parse<OrderStatus>(dto.Status), dto.CompletedOn);
        return order;
    }

    /// <summary>Будує DTO із доменного замовлення.</summary>
    /// <param name="order">Замовлення.</param>
    /// <returns>Дані для запису у файл.</returns>
    public static OrderDto ToOrderDto(ServiceOrder order)
    {
        return new OrderDto
        {
            Id = order.Id,
            VehicleId = order.VehicleId,
            Kind = order.Kind.ToString(),
            Status = order.Status.ToString(),
            PlannedOn = order.PlannedOn,
            CompletedOn = order.CompletedOn,
            Cost = order.Cost,
        };
    }
}
