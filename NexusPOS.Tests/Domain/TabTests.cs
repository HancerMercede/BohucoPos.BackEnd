using FluentAssertions;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Tests.Domain;

public class TabTests
{
    [Fact]
    public void Open_WithValidData_ReturnsTabWithOpenStatus()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.Should().NotBeNull();
        tab.Status.Should().Be(TabStatus.Open);
        tab.Location.Should().Be("Mesa 1");
        tab.CustomerName.Should().Be("John");
        tab.WaiterId.Should().Be("waiter1");
        tab.WaiterName.Should().Be("John Doe");
        tab.OpenedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Open_DefaultTaxRate_Is18Percent()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.TaxRate.Should().Be(0.18m);
    }

    [Fact]
    public void Open_CustomTaxRate_UsesProvidedValue()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe", 0.10m);

        tab.TaxRate.Should().Be(0.10m);
    }

    [Fact]
    public void AddOrder_WhenOpen_AddsOrderToCollection()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        var order = Order.Create(OrderType.Table, "t1", "John");

        tab.AddOrder(order);

        tab.Orders.Should().Contain(order);
    }

    [Fact]
    public void AddOrder_WhenClosed_ThrowsInvalidOperationException()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.Close(PaymentMethod.Cash);
        var order = Order.Create(OrderType.Delivery, "t1", "John");

        var act = () => tab.AddOrder(order);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot add order to a tab that is not open*");
    }

    [Fact]
    public void RequestBill_WhenOpen_ChangesStatusToPending()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.RequestBill();

        tab.Status.Should().Be(TabStatus.Pending);
    }

    [Fact]
    public void RequestBill_WhenClosed_ThrowsInvalidOperationException()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.Close(PaymentMethod.Cash);

        var act = () => tab.RequestBill();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot request bill for a tab that is not open*");
    }

    [Fact]
    public void ContinueOrdering_WhenPending_ChangesStatusToOpen()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.RequestBill();

        tab.ContinueOrdering();

        tab.Status.Should().Be(TabStatus.Open);
    }

    [Fact]
    public void ContinueOrdering_WhenNotPending_ThrowsInvalidOperationException()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        var act = () => tab.ContinueOrdering();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Tab must be pending to continue ordering*");
    }

    [Fact]
    public void Close_WhenOpen_ChangesStatusToClosed()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.Close(PaymentMethod.Card);

        tab.Status.Should().Be(TabStatus.Closed);
        tab.ClosedAt.Should().NotBeNull();
        tab.PaymentMethod.Should().Be(PaymentMethod.Card);
    }

    [Fact]
    public void Close_WhenPending_ChangesStatusToClosed()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.RequestBill();

        tab.Close(PaymentMethod.Cash);

        tab.Status.Should().Be(TabStatus.Closed);
    }

    [Fact]
    public void Close_WhenClosed_ThrowsInvalidOperationException()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.Close(PaymentMethod.Cash);

        var act = () => tab.Close(PaymentMethod.Card);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Tab must be open or pending to close*");
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.Cancel("Customer left");

        tab.Status.Should().Be(TabStatus.Cancelled);
        tab.ClosedAt.Should().NotBeNull();
        tab.Notes.Should().Be("Customer left");
    }

    [Fact]
    public void UpdateNotes_UpdatesNotesField()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.UpdateNotes("VIP customer");

        tab.Notes.Should().Be("VIP customer");
    }

    [Fact]
    public void IdDisplay_FormatsWithLeadingZeros()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        var reflection = typeof(Tab).GetProperty("Id");
        reflection!.SetValue(tab, 5);

        tab.IdDisplay.Should().Be("TAB-005");
    }

    [Fact]
    public void Subtotal_WithNoOrders_ReturnsZero()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.Subtotal.Should().Be(0);
    }

    [Fact]
    public void Total_WithOrders_CalculatesCorrectly()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe", 0.10m);
        var order = Order.Create(OrderType.Table, "t1", "John");
        order.AddItem("p1", "Product 1", 100m, 2, null, ItemDestination.Kitchen);
        
        tab.AddOrder(order);

        tab.Subtotal.Should().Be(200m);
        tab.Tax.Should().Be(20m);
        tab.Total.Should().Be(220m);
    }

    [Fact]
    public void CloseDirect_WhenOpen_ChangesStatusToClosed()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.CloseDirect(PaymentMethod.Card);

        tab.Status.Should().Be(TabStatus.Closed);
        tab.ClosedAt.Should().NotBeNull();
        tab.PaymentMethod.Should().Be(PaymentMethod.Card);
    }

    [Fact]
    public void CloseDirect_WhenPending_ChangesStatusToClosed()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.RequestBill();

        tab.CloseDirect(PaymentMethod.Transfer);

        tab.Status.Should().Be(TabStatus.Closed);
    }

    [Fact]
    public void CloseDirect_WhenClosed_ChangesStatusToClosed()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.Close(PaymentMethod.Cash);

        tab.CloseDirect(PaymentMethod.Card);

        tab.Status.Should().Be(TabStatus.Closed);
    }

    [Fact]
    public void AddOrder_WhenPending_ThrowsInvalidOperationException()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.RequestBill();
        var order = Order.Create(OrderType.Table, "t1", "John");

        var act = () => tab.AddOrder(order);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot add order to a tab that is not open*");
    }

    [Fact]
    public void RequestBill_WhenPending_ThrowsInvalidOperationException()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.RequestBill();

        var act = () => tab.RequestBill();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot request bill for a tab that is not open*");
    }

    [Fact]
    public void Subtotal_WithMultipleOrders_CalculatesCorrectly()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe", 0.10m);
        
        var order1 = Order.Create(OrderType.Table, "t1", "John");
        order1.AddItem("p1", "Product 1", 50m, 2, null, ItemDestination.Kitchen);
        
        var order2 = Order.Create(OrderType.Table, "t1", "John");
        order2.AddItem("p2", "Product 2", 30m, 3, null, ItemDestination.Bar);
        
        tab.AddOrder(order1);
        tab.AddOrder(order2);

        tab.Subtotal.Should().Be(190m);
        tab.Tax.Should().Be(19m);
        tab.Total.Should().Be(209m);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ChangesStatusToCancelled()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.Cancel("First cancel");

        tab.Cancel("Second cancel");

        tab.Status.Should().Be(TabStatus.Cancelled);
    }

    [Fact]
    public void Open_WithZeroTaxRate_UsesZeroValue()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe", 0m);

        tab.TaxRate.Should().Be(0m);
    }

    [Fact]
    public void Total_WithZeroTaxRate_EqualsSubtotal()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe", 0m);
        var order = Order.Create(OrderType.Table, "t1", "John");
        order.AddItem("p1", "Product 1", 100m, 1, null, ItemDestination.Kitchen);
        
        tab.AddOrder(order);

        tab.Tax.Should().Be(0m);
        tab.Total.Should().Be(100m);
    }

    [Fact]
    public void UpdateNotes_WhenClosed_UpdatesNotesField()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");
        tab.Close(PaymentMethod.Cash);

        tab.UpdateNotes("VIP customer");

        tab.Notes.Should().Be("VIP customer");
    }

    [Fact]
    public void Cancel_WithNullReason_SetsNotesToNull()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.Cancel();

        tab.Status.Should().Be(TabStatus.Cancelled);
        tab.Notes.Should().BeNull();
    }

    [Fact]
    public void Total_WithMultipleItemsInOrder_CalculatesCorrectly()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe", 0.10m);
        var order = Order.Create(OrderType.Table, "t1", "John");
        order.AddItem("p1", "Product 1", 25m, 2, "No onions", ItemDestination.Kitchen);
        order.AddItem("p2", "Product 2", 15m, 3, null, ItemDestination.Bar);
        
        tab.AddOrder(order);

        tab.Subtotal.Should().Be(95m);
        tab.Tax.Should().Be(9.5m);
        tab.Total.Should().Be(104.5m);
    }

    [Fact]
    public void Close_WithCash_ChangesStatusToClosed()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.Close(PaymentMethod.Cash);

        tab.Status.Should().Be(TabStatus.Closed);
        tab.PaymentMethod.Should().Be(PaymentMethod.Cash);
    }

    [Fact]
    public void Close_WithTransfer_ChangesStatusToClosed()
    {
        var tab = Tab.Open("Mesa 1", "John", "waiter1", "John Doe");

        tab.Close(PaymentMethod.Transfer);

        tab.Status.Should().Be(TabStatus.Closed);
        tab.PaymentMethod.Should().Be(PaymentMethod.Transfer);
    }
}
