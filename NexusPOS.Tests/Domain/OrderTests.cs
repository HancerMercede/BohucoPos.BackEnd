using FluentAssertions;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_WithValidData_ReturnsOrderWithPendingStatus()
    {
        var order = Order.Create(OrderType.Table, "t1", "John Doe");

        order.Should().NotBeNull();
        order.Status.Should().Be(OrderStatus.Pending);
        order.OrderType.Should().Be(OrderType.Table);
        order.TableId.Should().Be("t1");
        order.WaiterName.Should().Be("John Doe");
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithTableType_SetsOrderTypeToTable()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");

        order.OrderType.Should().Be(OrderType.Table);
    }

    [Fact]
    public void Create_WithBarType_SetsOrderTypeToBar()
    {
        var order = Order.Create(OrderType.Bar, "bar1", "John");

        order.OrderType.Should().Be(OrderType.Bar);
    }

    [Fact]
    public void Create_WithTakeAwayType_SetsOrderTypeToTakeAway()
    {
        var order = Order.Create(OrderType.TakeAway, null, "John");

        order.OrderType.Should().Be(OrderType.TakeAway);
    }

    [Fact]
    public void Create_WithDeliveryType_SetsOrderTypeToDelivery()
    {
        var order = Order.Create(OrderType.Delivery, null, "John");

        order.OrderType.Should().Be(OrderType.Delivery);
    }

    [Fact]
    public void Create_WithTabId_SetsTabId()
    {
        var order = Order.Create(OrderType.Table, "t1", "John", 5);

        order.TabId.Should().Be(5);
    }

    [Fact]
    public void Create_WithNullTableId_AllowsNull()
    {
        var order = Order.Create(OrderType.Delivery, null, "John");

        order.TableId.Should().BeNull();
    }

    [Fact]
    public void AddItem_WithValidData_AddsItemToCollection()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");

        order.AddItem("p1", "Product 1", 25.50m, 2, "No onions", ItemDestination.Kitchen);

        order.Items.Should().HaveCount(1);
        var item = order.Items.First();
        item.ProductId.Should().Be("p1");
        item.ProductName.Should().Be("Product 1");
        item.UnitPrice.Should().Be(25.50m);
        item.Quantity.Should().Be(2);
        item.Notes.Should().Be("No onions");
        item.Destination.Should().Be(ItemDestination.Kitchen);
        item.Status.Should().Be(ItemStatus.Pending);
    }

    [Fact]
    public void AddItem_ToBar_AddsItemWithBarDestination()
    {
        var order = Order.Create(OrderType.Bar, "bar1", "John");

        order.AddItem("p1", "Cerveza", 10m, 1, null, ItemDestination.Bar);

        order.Items.First().Destination.Should().Be(ItemDestination.Bar);
    }

    [Fact]
    public void AddItem_MultipleItems_AddsAllToCollection()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");

        order.AddItem("p1", "Product 1", 10m, 1, null, ItemDestination.Kitchen);
        order.AddItem("p2", "Product 2", 15m, 2, null, ItemDestination.Bar);

        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddItem_WithNullNotes_AllowsNull()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");

        order.AddItem("p1", "Product 1", 10m, 1, null, ItemDestination.Kitchen);

        order.Items.First().Notes.Should().BeNull();
    }

    [Fact]
    public void UpdateStatus_WithValidStatus_ChangesStatus()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");

        order.UpdateStatus(OrderStatus.InProgress);

        order.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public void UpdateStatus_ToReady_ChangesStatus()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");

        order.UpdateStatus(OrderStatus.Ready);

        order.Status.Should().Be(OrderStatus.Ready);
    }

    [Fact]
    public void UpdateStatus_ToDelivered_ChangesStatus()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");

        order.UpdateStatus(OrderStatus.Delivered);

        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void UpdateStatus_ToCancelled_ChangesStatus()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");

        order.UpdateStatus(OrderStatus.Cancelled);

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void IdDisplay_FormatsWithLeadingZeros()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");
        order.Id = 7;

        order.IdDisplay.Should().Be("ORD-007");
    }

    [Fact]
    public void IdDisplay_WithLargeId_FormatsCorrectly()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");
        order.Id = 123;

        order.IdDisplay.Should().Be("ORD-123");
    }

    [Fact]
    public void Items_InitiallyEmpty()
    {
        var order = Order.Create(OrderType.Table, "t1", "John");

        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyWaiterName_AllowsEmpty()
    {
        var order = Order.Create(OrderType.Table, "t1", "");

        order.WaiterName.Should().BeEmpty();
    }
}
