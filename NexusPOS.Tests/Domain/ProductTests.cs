using FluentAssertions;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void NewProduct_WithDefaultValues_HasActiveStatus()
    {
        var product = new Product();

        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void NewProduct_WithDefaultValues_HasCreatedAt()
    {
        var product = new Product();

        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Product_WithProperties_CanSetAllFields()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Hamburguesa",
            Price = 15.99m,
            Category = "Comida",
            Destination = ItemDestination.Kitchen,
            ProductType = ProductType.Physical,
            StockQuantity = 50,
            Emoji = "🍔",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        product.Id.Should().Be(1);
        product.Name.Should().Be("Hamburguesa");
        product.Price.Should().Be(15.99m);
        product.Category.Should().Be("Comida");
        product.Destination.Should().Be(ItemDestination.Kitchen);
        product.ProductType.Should().Be(ProductType.Physical);
        product.StockQuantity.Should().Be(50);
        product.Emoji.Should().Be("🍔");
        product.IsActive.Should().BeTrue();
        product.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Product_WithServiceType_SetsProductType()
    {
        var product = new Product { ProductType = ProductType.Service };

        product.ProductType.Should().Be(ProductType.Service);
    }

    [Fact]
    public void Product_WithPhysicalType_SetsProductType()
    {
        var product = new Product { ProductType = ProductType.Physical };

        product.ProductType.Should().Be(ProductType.Physical);
    }

    [Fact]
    public void Product_WithKitchenDestination_SetsDestination()
    {
        var product = new Product { Destination = ItemDestination.Kitchen };

        product.Destination.Should().Be(ItemDestination.Kitchen);
    }

    [Fact]
    public void Product_WithBarDestination_SetsDestination()
    {
        var product = new Product { Destination = ItemDestination.Bar };

        product.Destination.Should().Be(ItemDestination.Bar);
    }

    [Fact]
    public void Product_WithZeroPrice_AllowsZero()
    {
        var product = new Product { Price = 0m };

        product.Price.Should().Be(0m);
    }

    [Fact]
    public void Product_WithNullStock_AllowsNull()
    {
        var product = new Product { StockQuantity = null };

        product.StockQuantity.Should().BeNull();
    }

    [Fact]
    public void Product_WithNullEmoji_AllowsNull()
    {
        var product = new Product { Emoji = null };

        product.Emoji.Should().BeNull();
    }

    [Fact]
    public void Product_WithNegativePrice_AllowsNegative()
    {
        var product = new Product { Price = -5m };

        product.Price.Should().Be(-5m);
    }

    [Fact]
    public void Product_WithEmptyName_AllowsEmpty()
    {
        var product = new Product { Name = "" };

        product.Name.Should().BeEmpty();
    }

    [Fact]
    public void Product_WithEmptyCategory_AllowsEmpty()
    {
        var product = new Product { Category = "" };

        product.Category.Should().BeEmpty();
    }

    [Fact]
    public void Product_SetIsActiveToFalse()
    {
        var product = new Product { IsActive = false };

        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Product_WithLargePrice_CanHandleDecimal()
    {
        var product = new Product { Price = 999999.99m };

        product.Price.Should().Be(999999.99m);
    }

    [Fact]
    public void Product_WithLargeStock_CanHandleInt()
    {
        var product = new Product { StockQuantity = 999999 };

        product.StockQuantity.Should().Be(999999);
    }
}
