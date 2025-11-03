using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GiftOfTheGivers2.Data;

namespace GiftOfTheGivers2.IntegrationTests
{
    public class WebApplicationFactoryFixture : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();

                    // Seed the database with test data
                    SeedTestData(db);
                }
            });
        }

        private static void SeedTestData(ApplicationDbContext context)
        {
            // Clear existing data
            context.Users.RemoveRange(context.Users);
            context.DisasterIncidents.RemoveRange(context.DisasterIncidents);
            context.ResourceCategories.RemoveRange(context.ResourceCategories);
            context.Donations.RemoveRange(context.Donations);
            context.VolunteerTasks.RemoveRange(context.VolunteerTasks);
            context.VolunteerAssignments.RemoveRange(context.VolunteerAssignments);

            // Seed resource categories
            var categories = new[]
            {
                new Models.ResourceCategory { CategoryId = Guid.NewGuid(), CategoryName = "Food", Description = "Food items" },
                new Models.ResourceCategory { CategoryId = Guid.NewGuid(), CategoryName = "Clothing", Description = "Clothing items" },
                new Models.ResourceCategory { CategoryId = Guid.NewGuid(), CategoryName = "Medical", Description = "Medical supplies" }
            };
            context.ResourceCategories.AddRange(categories);

            // Seed test users
            var users = new[]
            {
                new Models.User {
                    UserId = Guid.NewGuid(),
                    Email = "admin@gotg.org",
                    PasswordHash = "hashed_password",
                    FirstName = "Admin",
                    LastName = "User",
                    UserType = "Admin"
                },
                new Models.User {
                    UserId = Guid.NewGuid(),
                    Email = "volunteer@test.com",
                    PasswordHash = "hashed_password",
                    FirstName = "John",
                    LastName = "Volunteer",
                    UserType = "Volunteer"
                },
                new Models.User {
                    UserId = Guid.NewGuid(),
                    Email = "donor@test.com",
                    PasswordHash = "hashed_password",
                    FirstName = "Jane",
                    LastName = "Donor",
                    UserType = "Donor"
                }
            };
            context.Users.AddRange(users);

            context.SaveChanges();

            // Seed disaster incidents
            var incidents = new[]
            {
                new Models.DisasterIncident
                {
                    IncidentId = Guid.NewGuid(),
                    UserId = users[0].UserId,
                    Title = "Flood in Durban",
                    Description = "Severe flooding affecting multiple areas",
                    IncidentType = "Flood",
                    Location = "Durban, South Africa",
                    SeverityLevel = "High",
                    Status = "Reported",
                    PeopleAffected = 5000,
                    ImmediateNeeds = "Food, Water, Shelter",
                    ReportedAt = DateTime.Now.AddDays(-2)
                },
                new Models.DisasterIncident
                {
                    IncidentId = Guid.NewGuid(),
                    UserId = users[0].UserId,
                    Title = "Fire in Johannesburg",
                    Description = "Large residential fire in informal settlement",
                    IncidentType = "Fire",
                    Location = "Johannesburg, South Africa",
                    SeverityLevel = "Critical",
                    Status = "InProgress",
                    PeopleAffected = 200,
                    ImmediateNeeds = "Shelter, Clothing, Food",
                    ReportedAt = DateTime.Now.AddDays(-1)
                }
            };
            context.DisasterIncidents.AddRange(incidents);

            context.SaveChanges();

            // Seed volunteer tasks
            var tasks = new[]
            {
                new Models.VolunteerTask
                {
                    TaskId = Guid.NewGuid(),
                    IncidentId = incidents[0].IncidentId,
                    Title = "Food Distribution",
                    Description = "Distribute food packages to affected families",
                    TaskType = "Distribution",
                    Location = "Durban Central",
                    RequiredVolunteers = 10,
                    CurrentVolunteers = 3,
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddDays(2),
                    Status = "Open"
                },
                new Models.VolunteerTask
                {
                    TaskId = Guid.NewGuid(),
                    IncidentId = incidents[1].IncidentId,
                    Title = "Emergency Shelter Setup",
                    Description = "Set up temporary shelters for fire victims",
                    TaskType = "Shelter",
                    Location = "Johannesburg West",
                    RequiredVolunteers = 15,
                    CurrentVolunteers = 8,
                    StartDate = DateTime.Now.AddHours(2),
                    EndDate = DateTime.Now.AddDays(1),
                    Status = "InProgress"
                }
            };
            context.VolunteerTasks.AddRange(tasks);

            context.SaveChanges();

            // Seed donations
            var donations = new[]
            {
                new Models.Donation
                {
                    DonationId = Guid.NewGuid(),
                    UserId = users[2].UserId,
                    CategoryId = categories[0].CategoryId,
                    ItemName = "Canned Food",
                    Quantity = 100,
                    Description = "Various canned food items",
                    DonationType = "Food",
                    Status = "Received",
                    DonatedAt = DateTime.Now.AddDays(-3),
                    ReceivedAt = DateTime.Now.AddDays(-2)
                },
                new Models.Donation
                {
                    DonationId = Guid.NewGuid(),
                    UserId = users[2].UserId,
                    CategoryId = categories[1].CategoryId,
                    ItemName = "Winter Clothing",
                    Quantity = 50,
                    Description = "Warm clothing for winter",
                    DonationType = "Clothing",
                    Status = "Pending",
                    DonatedAt = DateTime.Now.AddDays(-1)
                }
            };
            context.Donations.AddRange(donations);

            context.SaveChanges();

            // Seed volunteer assignments
            var assignments = new[]
            {
                new Models.VolunteerAssignment
                {
                    AssignmentId = Guid.NewGuid(),
                    TaskId = tasks[0].TaskId,
                    UserId = users[1].UserId,
                    AssignmentDate = DateTime.Now.AddDays(-1),
                    Status = "Assigned",
                    HoursWorked = null
                }
            };
            context.VolunteerAssignments.AddRange(assignments);

            context.SaveChanges();
        }
    }
}

