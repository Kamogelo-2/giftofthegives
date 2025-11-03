using GiftOfTheGivers2.Models;

namespace GiftOfTheGivers2.Tests
{
    public class TestDataBuilder
    {
        public static User CreateTestUser(
            string firstName = "John",
            string lastName = "Doe",
            string email = "john.doe@example.com",
            string userType = "Volunteer")
        {
            return new User
            {
                UserId = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = "hashed_password",
                PhoneNumber = "1234567890",
                Address = "123 Test Street",
                UserType = userType,
                IsActive = true,
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.Now
            };
        }

        public static DisasterIncident CreateTestIncident(
            Guid userId,
            string title = "Test Flood Incident",
            string incidentType = "Flood")
        {
            return new DisasterIncident
            {
                IncidentId = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Description = "Test description of the disaster incident",
                IncidentType = incidentType,
                Location = "Test Location",
                Latitude = 40.7128m,
                Longitude = -74.0060m,
                SeverityLevel = "Medium",
                Status = "Reported",
                PeopleAffected = 100,
                ImmediateNeeds = "Food, Water, Shelter",
                ReportedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        public static Donation CreateTestDonation(
            Guid userId,
            Guid categoryId,
            string itemName = "Test Donation Item")
        {
            return new Donation
            {
                DonationId = Guid.NewGuid(),
                UserId = userId,
                CategoryId = categoryId,
                ItemName = itemName,
                Quantity = 10,
                Description = "Test donation description",
                DonationType = "Food",
                Status = "Pending",
                DonatedAt = DateTime.Now
            };
        }

        public static VolunteerTask CreateTestTask(
            Guid? incidentId = null,
            string title = "Test Volunteer Task")
        {
            return new VolunteerTask
            {
                TaskId = Guid.NewGuid(),
                IncidentId = incidentId,
                Title = title,
                Description = "Test task description",
                TaskType = "Distribution",
                Location = "Test Location",
                RequiredVolunteers = 5,
                CurrentVolunteers = 0,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(2),
                Status = "Open",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        public static ResourceCategory CreateTestCategory(
            string categoryName = "Test Category")
        {
            return new ResourceCategory
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = categoryName,
                Description = "Test category description"
            };
        }
    }
}

