using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GiftOfTheGivers2.IntegrationTests
{
    public class ServiceIntegrationTests : IClassFixture<WebApplicationFactoryFixture>
    {
        private readonly WebApplicationFactoryFixture _factory;
        private readonly ApplicationDbContext _dbContext;

        public ServiceIntegrationTests(WebApplicationFactoryFixture factory)
        {
            _factory = factory;
            _dbContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        [Fact]
        public async Task DisasterManagement_EndToEndWorkflow_ShouldWorkCorrectly()
        {
            // Arrange - Create a complete disaster management scenario
            var adminUser = await _dbContext.Users.FirstAsync(u => u.UserType == "Admin");

            // Step 1: Report a new incident
            var newIncident = new DisasterIncident
            {
                IncidentId = Guid.NewGuid(),
                UserId = adminUser.UserId,
                Title = "Integration Test Hurricane",
                Description = "Hurricane affecting coastal areas - integration test",
                IncidentType = "Hurricane",
                Location = "Coastal Region, South Africa",
                SeverityLevel = "Critical",
                Status = "Reported",
                PeopleAffected = 8000,
                ImmediateNeeds = "Evacuation, Shelter, Food, Medical",
                ReportedAt = DateTime.Now
            };

            _dbContext.DisasterIncidents.Add(newIncident);
            await _dbContext.SaveChangesAsync();

            // Step 2: Create volunteer tasks for the incident
            var rescueTask = new VolunteerTask
            {
                TaskId = Guid.NewGuid(),
                IncidentId = newIncident.IncidentId,
                Title = "Search and Rescue Operations",
                Description = "Coordinate search and rescue in affected areas",
                TaskType = "Rescue",
                Location = "Coastal Region",
                RequiredVolunteers = 20,
                CurrentVolunteers = 0,
                StartDate = DateTime.Now.AddHours(1),
                EndDate = DateTime.Now.AddDays(2),
                Status = "Open"
            };

            var distributionTask = new VolunteerTask
            {
                TaskId = Guid.NewGuid(),
                IncidentId = newIncident.IncidentId,
                Title = "Emergency Supply Distribution",
                Description = "Distribute emergency supplies to affected communities",
                TaskType = "Distribution",
                Location = "Coastal Evacuation Centers",
                RequiredVolunteers = 15,
                CurrentVolunteers = 0,
                StartDate = DateTime.Now.AddHours(4),
                EndDate = DateTime.Now.AddDays(3),
                Status = "Open"
            };

            _dbContext.VolunteerTasks.AddRange(rescueTask, distributionTask);
            await _dbContext.SaveChangesAsync();

            // Step 3: Process donations for the incident
            var donor = await _dbContext.Users.FirstAsync(u => u.UserType == "Donor");
            var medicalCategory = await _dbContext.ResourceCategories.FirstAsync(c => c.CategoryName == "Medical");

            var medicalDonation = new Donation
            {
                DonationId = Guid.NewGuid(),
                UserId = donor.UserId,
                CategoryId = medicalCategory.CategoryId,
                ItemName = "Emergency Medical Kits",
                Quantity = 100,
                Description = "First aid and emergency medical supplies",
                DonationType = "Medical",
                Status = "Received",
                DonatedAt = DateTime.Now,
                ReceivedAt = DateTime.Now
            };

            _dbContext.Donations.Add(medicalDonation);
            await _dbContext.SaveChangesAsync();

            // Step 4: Assign volunteers to tasks
            var volunteer = await _dbContext.Users.FirstAsync(u => u.UserType == "Volunteer");

            var rescueAssignment = new VolunteerAssignment
            {
                AssignmentId = Guid.NewGuid(),
                TaskId = rescueTask.TaskId,
                UserId = volunteer.UserId,
                AssignmentDate = DateTime.Now,
                Status = "Assigned"
            };

            _dbContext.VolunteerAssignments.Add(rescueAssignment);

            // Update task volunteer counts
            rescueTask.CurrentVolunteers++;
            await _dbContext.SaveChangesAsync();

            // Step 5: Verify the complete workflow
            var savedIncident = await _dbContext.DisasterIncidents
                .Include(i => i.User)
                .FirstAsync(i => i.IncidentId == newIncident.IncidentId);

            var incidentTasks = await _dbContext.VolunteerTasks
                .Where(t => t.IncidentId == newIncident.IncidentId)
                .ToListAsync();

            var incidentDonations = await _dbContext.Donations
                .Where(d => d.Status == "Received")
                .ToListAsync();

            var volunteerAssignments = await _dbContext.VolunteerAssignments
                .Where(va => va.Task.IncidentId == newIncident.IncidentId)
                .ToListAsync();

            // Assert
            Assert.NotNull(savedIncident);
            Assert.Equal("Integration Test Hurricane", savedIncident.Title);
            Assert.Equal(2, incidentTasks.Count);
            Assert.Contains(incidentDonations, d => d.ItemName == "Emergency Medical Kits");
            Assert.Single(volunteerAssignments);
            Assert.Equal(volunteer.UserId, volunteerAssignments[0].UserId);
        }

        [Fact]
        public async Task DataConsistency_AcrossMultipleOperations_ShouldBeMaintained()
        {
            // Arrange
            var initialUserCount = await _dbContext.Users.CountAsync();
            var initialIncidentCount = await _dbContext.DisasterIncidents.CountAsync();
            var initialDonationCount = await _dbContext.Donations.CountAsync();

            // Act - Perform multiple operations in sequence
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = $"consistency.test{Guid.NewGuid()}@example.com",
                PasswordHash = "hashed_password",
                FirstName = "Consistency",
                LastName = "Test",
                UserType = "Volunteer"
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            var newIncident = new DisasterIncident
            {
                IncidentId = Guid.NewGuid(),
                UserId = newUser.UserId,
                Title = "Consistency Test Incident",
                Description = "Testing data consistency across operations",
                IncidentType = "Test",
                Location = "Test Location",
                SeverityLevel = "Low",
                Status = "Reported",
                PeopleAffected = 10
            };

            _dbContext.DisasterIncidents.Add(newIncident);
            await _dbContext.SaveChangesAsync();

            var category = await _dbContext.ResourceCategories.FirstAsync();
            var newDonation = new Donation
            {
                DonationId = Guid.NewGuid(),
                UserId = newUser.UserId,
                CategoryId = category.CategoryId,
                ItemName = "Consistency Test Donation",
                Quantity = 5,
                Description = "Test donation for consistency check",
                DonationType = "Food",
                Status = "Pending"
            };

            _dbContext.Donations.Add(newDonation);
            await _dbContext.SaveChangesAsync();

            // Assert - Verify data consistency
            var finalUserCount = await _dbContext.Users.CountAsync();
            var finalIncidentCount = await _dbContext.DisasterIncidents.CountAsync();
            var finalDonationCount = await _dbContext.Donations.CountAsync();

            Assert.Equal(initialUserCount + 1, finalUserCount);
            Assert.Equal(initialIncidentCount + 1, finalIncidentCount);
            Assert.Equal(initialDonationCount + 1, finalDonationCount);

            // Verify relationships are maintained
            var userIncidents = await _dbContext.DisasterIncidents
                .Where(i => i.UserId == newUser.UserId)
                .ToListAsync();

            var userDonations = await _dbContext.Donations
                .Where(d => d.UserId == newUser.UserId)
                .ToListAsync();

            Assert.Single(userIncidents);
            Assert.Single(userDonations);
        }
    }
    
    }

