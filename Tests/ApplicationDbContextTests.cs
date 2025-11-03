using Microsoft.EntityFrameworkCore;
using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Xunit;

namespace GiftOfTheGivers2.Tests
{
    public class ApplicationDbContextTests
    {
        [Fact]
        public async Task DbContext_CanAddAndRetrieveUser()
        {
            // Arrange
            using var context = TestUtilities.GetInMemoryDbContext();
            var user = TestDataBuilder.CreateTestUser();

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users.FindAsync(user.UserId);
            Assert.NotNull(savedUser);
            Assert.Equal(user.Email, savedUser.Email);
            Assert.Equal(user.FirstName, savedUser.FirstName);
        }

        [Fact]
        public async Task DbContext_UserEmail_ShouldBeUnique()
        {
            // Arrange
            using var context = TestUtilities.GetInMemoryDbContext();
            var user1 = TestDataBuilder.CreateTestUser(email: "test@example.com");
            var user2 = TestDataBuilder.CreateTestUser(email: "test@example.com");

            // Act
            context.Users.Add(user1);
            await context.SaveChangesAsync();

            // Assert - Adding user with same email should cause exception
            context.Users.Add(user2);
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        [Fact]
        public async Task DbContext_CanAddDisasterIncidentWithUser()
        {
            // Arrange
            using var context = TestUtilities.GetInMemoryDbContext();
            var user = TestDataBuilder.CreateTestUser();
            var incident = TestDataBuilder.CreateTestIncident(user.UserId);

            // Act
            context.Users.Add(user);
            context.DisasterIncidents.Add(incident);
            await context.SaveChangesAsync();

            // Assert
            var savedIncident = await context.DisasterIncidents
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.IncidentId == incident.IncidentId);

            Assert.NotNull(savedIncident);
            Assert.NotNull(savedIncident.User);
            Assert.Equal(user.UserId, savedIncident.User.UserId);
        }

        [Fact]
        public async Task DbContext_CanTrackDonationStatus()
        {
            // Arrange
            using var context = TestUtilities.GetInMemoryDbContext();
            TestUtilities.SeedTestData(context);

            var user = TestDataBuilder.CreateTestUser();
            var category = await context.ResourceCategories.FirstAsync();
            var donation = TestDataBuilder.CreateTestDonation(user.UserId, category.CategoryId);

            // Act
            context.Users.Add(user);
            context.Donations.Add(donation);
            await context.SaveChangesAsync();

            // Update donation status
            donation.Status = "Received";
            donation.ReceivedAt = DateTime.Now;
            await context.SaveChangesAsync();

            // Assert
            var updatedDonation = await context.Donations.FindAsync(donation.DonationId);
            Assert.Equal("Received", updatedDonation.Status);
            Assert.NotNull(updatedDonation.ReceivedAt);
        }

        [Fact]
        public async Task DbContext_VolunteerTaskAssignment_ShouldWorkCorrectly()
        {
            // Arrange
            using var context = TestUtilities.GetInMemoryDbContext();
            var user = TestDataBuilder.CreateTestUser();
            var task = TestDataBuilder.CreateTestTask();

            // Act
            context.Users.Add(user);
            context.VolunteerTasks.Add(task);
            await context.SaveChangesAsync();

            var assignment = new VolunteerAssignment
            {
                AssignmentId = Guid.NewGuid(),
                TaskId = task.TaskId,
                UserId = user.UserId,
                AssignmentDate = DateTime.Now,
                Status = "Assigned"
            };

            context.VolunteerAssignments.Add(assignment);
            await context.SaveChangesAsync();

            // Assert
            var savedAssignment = await context.VolunteerAssignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignment.AssignmentId);

            Assert.NotNull(savedAssignment);
            Assert.NotNull(savedAssignment.Task);
            Assert.NotNull(savedAssignment.User);
            Assert.Equal(task.TaskId, savedAssignment.Task.TaskId);
            Assert.Equal(user.UserId, savedAssignment.User.UserId);
        }
    }
}

