using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiftOfTheGivers2.Tests.Services
{
    public class DonationServiceTests
    {
        [Fact]
        public async Task Donation_TotalQuantity_ShouldBeCalculatedCorrectly()
        {
            // Arrange
            using var context = TestUtilities.GetInMemoryDbContext();
            TestUtilities.SeedTestData(context);

            var user = TestDataBuilder.CreateTestUser();
            var category = await context.ResourceCategories.FirstAsync();

            var donations = new[]
            {
                TestDataBuilder.CreateTestDonation(user.UserId, category.CategoryId, "Item1"),
                TestDataBuilder.CreateTestDonation(user.UserId, category.CategoryId, "Item2"),
                TestDataBuilder.CreateTestDonation(user.UserId, category.CategoryId, "Item3")
            };

            // Act
            context.Users.Add(user);
            context.Donations.AddRange(donations);
            await context.SaveChangesAsync();

            // Assert
            var totalQuantity = await context.Donations
                .Where(d => d.UserId == user.UserId)
                .SumAsync(d => d.Quantity);

            Assert.Equal(30, totalQuantity); // 3 donations × 10 quantity each
        }

        [Fact]
        public async Task Donation_StatusFlow_ShouldWorkCorrectly()
        {
            // Arrange
            using var context = TestUtilities.GetInMemoryDbContext();
            TestUtilities.SeedTestData(context);

            var user = TestDataBuilder.CreateTestUser();
            var category = await context.ResourceCategories.FirstAsync();
            var donation = TestDataBuilder.CreateTestDonation(user.UserId, category.CategoryId);

            // Act & Assert - Initial state
            context.Users.Add(user);
            context.Donations.Add(donation);
            await context.SaveChangesAsync();

            Assert.Equal("Pending", donation.Status);
            Assert.Null(donation.ReceivedAt);

            // Act & Assert - Mark as received
            donation.Status = "Received";
            donation.ReceivedAt = DateTime.Now;
            await context.SaveChangesAsync();

            var updatedDonation = await context.Donations.FindAsync(donation.DonationId);
            Assert.Equal("Received", updatedDonation.Status);
            Assert.NotNull(updatedDonation.ReceivedAt);
        }
    }
}
