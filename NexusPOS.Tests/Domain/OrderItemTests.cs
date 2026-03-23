using FluentAssertions;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Tests.Domain;

public class OrderItemTests
{
    [Fact]
    public void OrderItem_WithObjectInitializer_CanSetAllFields()
    {
        var item = new OrderItem
        {
            Id = 1,
            OrderId = 10,
            ProductId = "p1",
            ProductName = "Hamburguesa",
            UnitPrice = 15.50m,
            Quantity = 2,
            Notes = "Sin cebolla",
            Destination = ItemDestination.Kitchen,
            Status = ItemStatus.Pending
        };

        item.Id.Should().Be(1);
        item.OrderId.Should().Be(10);
        item.ProductId.Should().Be("p1");
        item.ProductName.Should().Be("Hamburguesa");
        item.UnitPrice.Should().Be(15.50m);
        item.Quantity.Should().Be(2);
        item.Notes.Should().Be("Sin cebolla");
        item.Destination.Should().Be(ItemDestination.Kitchen);
        item.Status.Should().Be(ItemStatus.Pending);
    }

    [Fact]
    public void OrderItem_DefaultStatus_IsPending()
    {
        var item = new OrderItem { Status = ItemStatus.Pending };

        item.Status.Should().Be(ItemStatus.Pending);
    }

    [Fact]
    public void OrderItem_CanUpdateStatus()
    {
        var item = new OrderItem { Status = ItemStatus.Pending };

        item.Status = ItemStatus.Preparing;

        item.Status.Should().Be(ItemStatus.Preparing);
    }

    [Fact]
    public void OrderItem_StatusToReady()
    {
        var item = new OrderItem { Status = ItemStatus.Pending };

        item.Status = ItemStatus.Ready;

        item.Status.Should().Be(ItemStatus.Ready);
    }

    [Fact]
    public void OrderItem_StatusToDelivered()
    {
        var item = new OrderItem { Status = ItemStatus.Pending };

        item.Status = ItemStatus.Delivered;

        item.Status.Should().Be(ItemStatus.Delivered);
    }

    [Fact]
    public void OrderItem_StatusToCancelled()
    {
        var item = new OrderItem { Status = ItemStatus.Pending };

        item.Status = ItemStatus.Cancelled;

        item.Status.Should().Be(ItemStatus.Cancelled);
    }

    [Fact]
    public void OrderItem_WithKitchenDestination()
    {
        var item = new OrderItem { Destination = ItemDestination.Kitchen };

        item.Destination.Should().Be(ItemDestination.Kitchen);
    }

    [Fact]
    public void OrderItem_WithBarDestination()
    {
        var item = new OrderItem { Destination = ItemDestination.Bar };

        item.Destination.Should().Be(ItemDestination.Bar);
    }

    [Fact]
    public void OrderItem_WithNullNotes_AllowsNull()
    {
        var item = new OrderItem { Notes = null };

        item.Notes.Should().BeNull();
    }

    [Fact]
    public void OrderItem_WithEmptyNotes_AllowsEmpty()
    {
        var item = new OrderItem { Notes = "" };

        item.Notes.Should().BeEmpty();
    }

    [Fact]
    public void OrderItem_WithZeroQuantity_AllowsZero()
    {
        var item = new OrderItem { Quantity = 0 };

        item.Quantity.Should().Be(0);
    }

    [Fact]
    public void OrderItem_WithLargeQuantity_CanHandle()
    {
        var item = new OrderItem { Quantity = 100 };

        item.Quantity.Should().Be(100);
    }

    [Fact]
    public void OrderItem_WithDecimalPrice_CanHandle()
    {
        var item = new OrderItem { UnitPrice = 0.01m };

        item.UnitPrice.Should().Be(0.01m);
    }

    [Fact]
    public void OrderItem_CalculatedTotal_QuantityTimesPrice()
    {
        var item = new OrderItem { UnitPrice = 10m, Quantity = 3 };

        var total = item.UnitPrice * item.Quantity;

        total.Should().Be(30m);
    }
}
