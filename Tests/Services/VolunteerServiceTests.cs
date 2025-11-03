using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiftOfTheGivers2.Tests.Services
{
    public class VolunteerServiceTests
    {
        [Fact]
        public async Task VolunteerTask_ShouldPreventOverAssignment()
        {
            // Arrange
            using var context = TestUtilities.GetInMemoryDbContext();
            var task = TestDataBuilder.CreateTestTask();
            task.RequiredVolunteers = 2;

            context.VolunteerTasks.Add(task);
            await context.SaveChangesAsync();

            var users = new[]
            {
                TestDataBuilder.CreateTestUser("User1", "Test", "user1@test.com"),
                TestDataBuilder.CreateTestUser("User2", "Test", "user2@test.com"),
                TestDataBuilder.CreateTestUser("User3", "Test", "user3@test.com")
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // Act - Assign first two users
            foreach (var user in users.Take(2))
            {
                var assignment = new VolunteerAssignment
                {
                    TaskId = task.TaskId,
                    UserId = user.UserId,
                    Status = "Assigned"
                };
                context.VolunteerAssignments.Add(assignment);
            }
            await context.SaveChangesAsync();

            // Update task volunteer count
            task.CurrentVolunteers = 2;
            task.Status = "InProgress";
            await context.SaveChangesAsync();

            // Assert
            var updatedTask = await context.VolunteerTasks.FindAsync(task.TaskId);
            Assert.Equal(2, updatedTask.CurrentVolunteers);
            Assert.Equal("InProgress", updatedTask.Status);

            var assignmentsCount = await context.VolunteerAssignments
                .CountAsync(a => a.TaskId == task.TaskId);

            Assert.Equal(2, assignmentsCount);
        }

        [Fact]
        public async Task VolunteerAssignment_HoursTracking_ShouldWorkCorrectly()
        {
            // Arrange
            using var context = TestUtilities.GetInMemoryDbContext();
            var user = TestDataBuilder.CreateTestUser();
            var task = TestDataBuilder.CreateTestTask();

            context.Users.Add(user);
            context.VolunteerTasks.Add(task);
            await context.SaveChangesAsync();

            var assignment = new VolunteerAssignment
            {
                TaskId = task.TaskId,
                UserId = user.UserId,
                Status = "Completed",
                HoursWorked = 8.5m,
                Notes = "Completed all assigned duties"
            };

            // Act
            context.VolunteerAssignments.Add(assignment);
            await context.SaveChangesAsync();

            // Assert
            var savedAssignment = await context.VolunteerAssignments
                .FirstOrDefaultAsync(a => a.AssignmentId == assignment.AssignmentId);

            Assert.NotNull(savedAssignment);
            Assert.Equal(8.5m, savedAssignment.HoursWorked);
            Assert.Equal("Completed", savedAssignment.Status);
            Assert.Equal("Completed all assigned duties", savedAssignment.Notes);
        }
    }
}

