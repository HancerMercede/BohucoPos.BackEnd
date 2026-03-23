using FluentAssertions;
using NexusPOS.Domain.Entities;

namespace NexusPOS.Tests.Domain;

public class UserTests
{
    [Fact]
    public void NewUser_WithDefaultValues_HasActiveStatus()
    {
        var user = new User();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void NewUser_WithDefaultValues_HasWaiterRole()
    {
        var user = new User();

        user.Role.Should().Be("Waiter");
    }

    [Fact]
    public void NewUser_WithDefaultValues_HasCreatedAt()
    {
        var user = new User();

        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void User_WithProperties_CanSetAllFields()
    {
        var user = new User
        {
            Id = 1,
            Username = "johndoe",
            PasswordHash = "hashedpassword",
            Role = "Admin",
            FullName = "John Doe",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        user.Id.Should().Be(1);
        user.Username.Should().Be("johndoe");
        user.PasswordHash.Should().Be("hashedpassword");
        user.Role.Should().Be("Admin");
        user.FullName.Should().Be("John Doe");
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void User_CanSetIsActiveToFalse()
    {
        var user = new User { IsActive = false };

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void User_CanChangeRole()
    {
        var user = new User { Role = "Manager" };

        user.Role.Should().Be("Manager");
    }

    [Fact]
    public void User_WithEmptyPasswordHash_AllowsEmpty()
    {
        var user = new User { PasswordHash = "" };

        user.PasswordHash.Should().BeEmpty();
    }

    [Fact]
    public void User_WithEmptyFullName_AllowsEmpty()
    {
        var user = new User { FullName = "" };

        user.FullName.Should().BeEmpty();
    }

    [Fact]
    public void User_WithEmptyUsername_AllowsEmpty()
    {
        var user = new User { Username = "" };

        user.Username.Should().BeEmpty();
    }
}
