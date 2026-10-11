using System;
using FleetMaintenance.Domain;
using Xunit;

namespace FleetMaintenance.Tests;

public sealed class TransitionTests
{
    private static readonly DateOnly Day = new(2026, 11, 2);

    [Theory]
    [InlineData(OrderStatus.Planned, OrderEvent.Start, OrderStatus.InProgress)]
    [InlineData(OrderStatus.Planned, OrderEvent.Cancel, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.InProgress, OrderEvent.Complete, OrderStatus.Done)]
    [InlineData(OrderStatus.InProgress, OrderEvent.Cancel, OrderStatus.Cancelled)]
    public void Next_ДозволенийПерехід_ПовертаєНовийСтан(
        OrderStatus origin,
        OrderEvent trigger,
        OrderStatus expected)
    {
        OrderStatus actual = OrderTransitions.Next(origin, trigger);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(OrderStatus.Planned, OrderEvent.Complete)]
    [InlineData(OrderStatus.InProgress, OrderEvent.Start)]
    [InlineData(OrderStatus.Done, OrderEvent.Start)]
    [InlineData(OrderStatus.Done, OrderEvent.Complete)]
    [InlineData(OrderStatus.Done, OrderEvent.Cancel)]
    [InlineData(OrderStatus.Cancelled, OrderEvent.Start)]
    [InlineData(OrderStatus.Cancelled, OrderEvent.Complete)]
    [InlineData(OrderStatus.Cancelled, OrderEvent.Cancel)]
    public void Next_ЗабороненийПерехід_КидаєВиняток(OrderStatus origin, OrderEvent trigger)
    {
        var error = Assert.Throws<InvalidOrderStateException>(
            () => OrderTransitions.Next(origin, trigger));

        Assert.Equal(origin, error.From);
        Assert.Equal(trigger, error.Trigger);
    }

    [Fact]
    public void Створення_НовеЗамовлення_ЗапланованеТаАктивне()
    {
        var order = new ServiceOrder(1, 1, ServiceKind.Brakes, Day);

        Assert.Equal(OrderStatus.Planned, order.Status);
        Assert.True(order.IsActive);
        Assert.Null(order.CompletedOn);
    }

    [Fact]
    public void Complete_ІзВиконується_СтаєDoneІЗаписуєДату()
    {
        var order = new ServiceOrder(1, 1, ServiceKind.Brakes, Day);
        order.Start();

        order.Complete(Day);

        Assert.Equal(OrderStatus.Done, order.Status);
        Assert.Equal(Day, order.CompletedOn);
        Assert.False(order.IsActive);
    }

    [Fact]
    public void Complete_ІзЗапланованого_КидаєВиняткоІНеЗмінюєСтан()
    {
        var order = new ServiceOrder(1, 1, ServiceKind.Brakes, Day);

        Assert.Throws<InvalidOrderStateException>(() => order.Complete(Day));

        Assert.Equal(OrderStatus.Planned, order.Status);
        Assert.Null(order.CompletedOn);
    }

    [Fact]
    public void Cancel_ПіслиВиконання_КидаєВиняток()
    {
        var order = new ServiceOrder(1, 1, ServiceKind.Brakes, Day);
        order.Start();
        order.Complete(Day);

        var error = Assert.Throws<InvalidOrderStateException>(() => order.Cancel());

        Assert.Contains("Cancel", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Restore_ЗбереженийСтан_ВідновлюєБезПеревірки()
    {
        var order = new ServiceOrder(1, 1, ServiceKind.Brakes, Day);

        order.Restore(OrderStatus.Cancelled, null);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.False(order.IsActive);
    }
}
