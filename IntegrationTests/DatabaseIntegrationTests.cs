using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiftOfTheGivers2.IntegrationTests
{
    public class DatabaseIntegrationTests
    {
        public class DatabaseIntegrationTests : IClassFixture<WebApplicationFactoryFixture>
        {
            private readonly WebApplicationFactoryFixture _factory;
            private readonly ApplicationDbContext _dbContext;

            public DatabaseIntegrationTests(WebApplicationFactoryFixture factory)
            {
                _factory = factory;
                _dbContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            }

            [Fact]
            public async Task Database_UserCRUDOperations_ShouldWorkCorrectly()
            {
                // Arrange
                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = "test.integration@example.com",
                    PasswordHash = "hashed_password_123",
                    FirstName = "Integration",
                    LastName = "Test",
                    PhoneNumber = "0123456789",
                    Address = "123 Integration Test Street",
                    UserType = "Volunteer",
                    IsActive = true
                };

                // Act - Create
                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();

                // Assert - Read
                var savedUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == "test.integration@example.com");

                Assert.NotNull(savedUser);
                Assert.Equal("Integration", savedUser.FirstName);
                Assert.Equal("Test", savedUser.LastName);

                // Act - Update
                savedUser.PhoneNumber = "0987654321";
                _dbContext.Users.Update(savedUser);
                await _dbContext.SaveChangesAsync();

                // Assert - Update
                var updatedUser = await _dbContext.Users.FindAsync(savedUser.UserId);
                Assert.Equal("0987654321", updatedUser.PhoneNumber);

                // Act - Delete
                _dbContext.Users.Remove(updatedUser);
                await _dbContext.SaveChangesAsync();

                // Assert - Delete
                var deletedUser = await _dbContext.Users.FindAsync(savedUser.UserId);
                Assert.Null(deletedUser);
            }

            [Fact]
            public async Task Database_DisasterIncidentWorkflow_ShouldWorkCorrectly()
            {
                // Arrange
                var user = await _dbContext.Users.FirstAsync(u => u.UserType == "Admin");

                var incident = new DisasterIncident
                {
                    IncidentId = Guid.NewGuid(),
                    UserId = user.UserId,
                    Title = "Earthquake Test Incident",
                    Description = "Test earthquake incident for integration testing",
                    IncidentType = "Earthquake",
                    Location = "Cape Town, South Africa",
                    SeverityLevel = "Critical",
                    Status = "Reported",
                    PeopleAffected = 10000,
                    ImmediateNeeds = "Search and Rescue, Medical, Shelter",
                    ReportedAt = DateTime.Now
                };

                // Act - Create incident
                _dbContext.DisasterIncidents.Add(incident);
                await _dbContext.SaveChangesAsync();

                // Assert - Incident created
                var savedIncident = await _dbContext.DisasterIncidents
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.IncidentId == incident.IncidentId);

                Assert.NotNull(savedIncident);
                Assert.Equal("Reported", savedIncident.Status);
                Assert.NotNull(savedIncident.User);
                Assert.Equal(user.UserId, savedIncident.User.UserId);

                // Act - Update incident status
                savedIncident.Status = "Verified";
                savedIncident.UpdatedAt = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                // Assert - Status updated
                var updatedIncident = await _dbContext.DisasterIncidents.FindAsync(incident.IncidentId);
                Assert.Equal("Verified", updatedIncident.Status);
            }

            [Fact]
            public async Task Database_DonationTracking_ShouldWorkCorrectly()
            {
                // Arrange
                var donor = await _dbContext.Users.FirstAsync(u => u.UserType == "Donor");
                var category = await _dbContext.ResourceCategories.FirstAsync(c => c.CategoryName == "Food");

                var donation = new Donation
                {
                    DonationId = Guid.NewGuid(),
                    UserId = donor.UserId,
                    CategoryId = category.CategoryId,
                    ItemName = "Emergency Food Packs",
                    Quantity = 250,
                    Description = "Food packs for emergency relief",
                    DonationType = "Food",
                    Status = "Pending",
                    DonatedAt = DateTime.Now
                };

                // Act - Create donation
                _dbContext.Donations.Add(donation);
                await _dbContext.SaveChangesAsync();

                // Assert - Donation created
                var savedDonation = await _dbContext.Donations
                    .Include(d => d.User)
                    .Include(d => d.Category)
                    .FirstOrDefaultAsync(d => d.DonationId == donation.DonationId);

                Assert.NotNull(savedDonation);
                Assert.Equal("Pending", savedDonation.Status);
                Assert.Equal("Donor", savedDonation.User.UserType);
                Assert.Equal("Food", savedDonation.Category.CategoryName);

                // Act - Update donation status to received
                savedDonation.Status = "Received";
                savedDonation.ReceivedAt = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                // Assert - Status updated
                var receivedDonation = await _dbContext.Donations.FindAsync(donation.DonationId);
                Assert.Equal("Received", receivedDonation.Status);
                Assert.NotNull(receivedDonation.ReceivedAt);
            }

            [Fact]
            public async Task Database_VolunteerManagement_ShouldWorkCorrectly()
            {
                // Arrange
                var volunteer = await _dbContext.Users.FirstAsync(u => u.UserType == "Volunteer");
                var task = await _dbContext.VolunteerTasks.FirstAsync(t => t.Status == "Open");

                var initialVolunteerCount = task.CurrentVolunteers;

                var assignment = new VolunteerAssignment
                {
                    AssignmentId = Guid.NewGuid(),
                    TaskId = task.TaskId,
                    UserId = volunteer.UserId,
                    AssignmentDate = DateTime.Now,
                    Status = "Assigned"
                };

                // Act - Create assignment
                _dbContext.VolunteerAssignments.Add(assignment);

                // Update task volunteer count
                task.CurrentVolunteers++;
                if (task.CurrentVolunteers >= task.RequiredVolunteers)
                {
                    task.Status = "InProgress";
                }

                await _dbContext.SaveChangesAsync();

                // Assert - Assignment created and task updated
                var savedAssignment = await _dbContext.VolunteerAssignments
                    .Include(a => a.Task)
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AssignmentId == assignment.AssignmentId);

                Assert.NotNull(savedAssignment);
                Assert.Equal("Assigned", savedAssignment.Status);
                Assert.Equal(volunteer.UserId, savedAssignment.User.UserId);
                Assert.Equal(task.TaskId, savedAssignment.Task.TaskId);

                var updatedTask = await _dbContext.VolunteerTasks.FindAsync(task.TaskId);
                Assert.Equal(initialVolunteerCount + 1, updatedTask.CurrentVolunteers);

                // Act - Complete assignment
                savedAssignment.Status = "Completed";
                savedAssignment.HoursWorked = 8.5m;
                savedAssignment.Notes = "Completed all assigned duties successfully";
                await _dbContext.SaveChangesAsync();

                // Assert - Assignment completed
                var completedAssignment = await _dbContext.VolunteerAssignments.FindAsync(assignment.AssignmentId);
                Assert.Equal("Completed", completedAssignment.Status);
                Assert.Equal(8.5m, completedAssignment.HoursWorked);
            }

            [Fact]
            public async Task Database_ComplexQueryOperations_ShouldWorkCorrectly()
            {
                // Act - Get active incidents with high severity
                var highSeverityIncidents = await _dbContext.DisasterIncidents
                    .Where(i => i.SeverityLevel == "High" || i.SeverityLevel == "Critical")
                    .Where(i => i.Status != "Resolved")
                    .OrderByDescending(i => i.ReportedAt)
                    .ToListAsync();

                // Assert
                Assert.NotNull(highSeverityIncidents);
                Assert.All(highSeverityIncidents, i =>
                    Assert.True(i.SeverityLevel == "High" || i.SeverityLevel == "Critical"));

                // Act - Get donation statistics by category
                var donationStats = await _dbContext.Donations
                    .GroupBy(d => d.Category.CategoryName)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalQuantity = g.Sum(d => d.Quantity),
                        AverageQuantity = g.Average(d => d.Quantity),
                        DonationCount = g.Count()
                    })
                    .ToListAsync();

                // Assert
                Assert.NotNull(donationStats);
                Assert.NotEmpty(donationStats);

                // Act - Get volunteer task assignments with details
                var volunteerAssignments = await _dbContext.VolunteerAssignments
                    .Include(va => va.User)
                    .Include(va => va.Task)
                    .ThenInclude(t => t.Incident)
                    .Where(va => va.Status == "Assigned")
                    .Select(va => new
                    {
                        VolunteerName = $"{va.User.FirstName} {va.User.LastName}",
                        TaskTitle = va.Task.Title,
                        IncidentTitle = va.Task.Incident.Title,
                        AssignmentDate = va.AssignmentDate
                    })
                    .ToListAsync();

                // Assert
                Assert.NotNull(volunteerAssignments);
            }
        }
    }
}
